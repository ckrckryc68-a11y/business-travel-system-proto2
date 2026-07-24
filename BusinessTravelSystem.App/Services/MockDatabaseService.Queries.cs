using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BusinessTravelSystem.App.Models;
using Microsoft.JSInterop;

namespace BusinessTravelSystem.App.Services;

public sealed partial class MockDatabaseService
{
    public async Task<IReadOnlyList<MockTripSearchRecord>> GetTripSearchRecordsAsync(
        long currentTravellerId,
        bool isAdministrator)
    {
        await InitializeAsync();

        var lookups = Rows("LOOKUP").Select(AsObject).ToDictionary(row => Number(row, "LOOKUP_ID"));
        var travellers = Rows("TRAVELLER").Select(AsObject).ToDictionary(row => Number(row, "TRAVELLER_ID"));
        var departments = Rows("DEPARTMENT").Select(AsObject).ToDictionary(row => Number(row, "DEPARTMENT_ID"));
        var destinations = Rows("DESTINATION").Select(AsObject).ToDictionary(row => Number(row, "DESTINATION_ID"));
        var tripDestinations = Rows("TRIP_DESTINATION")
            .Select(AsObject)
            .GroupBy(row => Number(row, "TRIP_ID"))
            .ToDictionary(group => group.Key, group => group.OrderBy(row => Date(row, "ARRIVAL_AT")).ToArray());
        var expenses = Rows("TRIP_EXPENSE")
            .Select(AsObject)
            .GroupBy(row => Number(row, "TRIP_ID"))
            .ToDictionary(group => group.Key, group => group.ToArray());
        var arrangements = Rows("ARRANGEMENT")
            .Select(AsObject)
            .GroupBy(row => Number(row, "TRIP_ID"))
            .ToDictionary(group => group.Key, group => group.ToArray());
        var processes = Rows("TRIP_PROCESS")
            .Select(AsObject)
            .GroupBy(row => Number(row, "TRIP_ID"))
            .ToDictionary(group => group.Key, group => group.OrderBy(row => Number(row, "SEQUENCE_NO_SNAPSHOT")).ToArray());

        string LookupName(long id) =>
            lookups.TryGetValue(id, out var lookup) ? Text(lookup, "LOOKUP_NAME") : "Unknown";

        string LookupCode(long id) =>
            lookups.TryGetValue(id, out var lookup) ? Text(lookup, "LOOKUP_CODE") : string.Empty;

        var records = new List<MockTripSearchRecord>();

        foreach (var trip in Rows("TRIP").Select(AsObject).OrderByDescending(row => Date(row, "CREATED_AT")))
        {
            var tripId = Number(trip, "TRIP_ID");
            var travellerId = Number(trip, "TRAVELLER_ID");
            var applicantId = Number(trip, "APPLICANT_ID");
            travellers.TryGetValue(travellerId, out var traveller);
            travellers.TryGetValue(applicantId, out var applicant);

            var destinationRows = tripDestinations.GetValueOrDefault(tripId, Array.Empty<JsonObject>());
            var primaryDestination = destinationRows.FirstOrDefault();
            var destinationName = primaryDestination is null
                ? "Destination not yet selected"
                : destinations.TryGetValue(Number(primaryDestination, "DESTINATION_ID"), out var destination)
                    ? Text(destination, "DESTINATION_NAME")
                    : "Unknown destination";

            var category = LookupName(Number(trip, "TRIP_CATEGORY_ID"));
            var status = LookupName(Number(trip, "STATUS_ID"));
            var tripExpenses = expenses.GetValueOrDefault(tripId, Array.Empty<JsonObject>());
            var estimatedCost = tripExpenses.Sum(row => Decimal(row, "BASE_AMOUNT"));
            var expenseTypeNames = tripExpenses
                .Select(row => LookupName(Number(row, "EXPENSE_TYPE_ID")))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToArray();

            var tripArrangements = arrangements.GetValueOrDefault(tripId, Array.Empty<JsonObject>());
            var hasContract = tripArrangements.Any(row =>
                LookupCode(Number(row, "ARRANGEMENT_TYPE_ID")).Equals("CONTRACT", StringComparison.OrdinalIgnoreCase));

            var activeProcess = processes
                .GetValueOrDefault(tripId, Array.Empty<JsonObject>())
                .FirstOrDefault(row =>
                    LookupCode(Number(row, "STEP_STATUS_ID")).Equals("ACTIVE", StringComparison.OrdinalIgnoreCase));

            var assignedTravellerId = activeProcess is null
                ? 0
                : NullableNumber(activeProcess, "ASSIGNED_TO_TRAVELLER_ID") ?? 0;

            var needsMyApproval = activeProcess is not null &&
                (isAdministrator || assignedTravellerId == currentTravellerId);

            var reviewer = assignedTravellerId > 0 && travellers.TryGetValue(assignedTravellerId, out var assignedTraveller)
                ? Text(assignedTraveller, "FULL_NAME")
                : "Unassigned";

            var travellerName = traveller is null ? "Unknown Traveller" : Text(traveller, "FULL_NAME");
            var applicantName = applicant is null ? travellerName : Text(applicant, "FULL_NAME");
            var department = traveller is not null &&
                             departments.TryGetValue(Number(traveller, "DEPARTMENT_ID"), out var departmentRow)
                ? Text(departmentRow, "DEPARTMENT_NAME")
                : "Unknown Department";

            var start = primaryDestination is null ? null : Date(primaryDestination, "ARRIVAL_AT");
            var end = destinationRows.LastOrDefault() is { } lastDestination
                ? Date(lastDestination, "DEPARTURE_AT")
                : start;

            var title = Text(trip, "TITLE");
            var purpose = Text(trip, "PURPOSE");
            var budgetJustification = NullableText(trip, "BUDGET_JUSTIFICATION");

            records.Add(new MockTripSearchRecord(
                TripId: tripId,
                TripNumber: NullableText(trip, "TRIP_NO") ?? Text(trip, "APPLICATION_NO"),
                Traveler: travellerName,
                Destination: destinationName,
                Category: category,
                Status: status,
                Summary: title,
                Title: category,
                Purpose: purpose,
                BudgetAllocation: string.IsNullOrWhiteSpace(budgetJustification) ? "No" : "Yes",
                CostCenter: LookupName(Number(trip, "COST_CENTER_ID")),
                ContractRequired: hasContract ? "Yes" : "No",
                Description: budgetJustification ?? purpose,
                Applicant: applicantName,
                Department: department,
                DateFiled: FormatDate(Date(trip, "CREATED_AT")),
                TravelDates: FormatDateRange(start, end),
                Venue: destinationName,
                EstimatedCost: estimatedCost,
                CostBreakdown: expenseTypeNames.Length == 0
                    ? "No expense details"
                    : string.Join(", ", expenseTypeNames),
                Reviewer: reviewer,
                LastUpdated: FormatDate(Date(trip, "UPDATED_AT") ?? Date(trip, "CREATED_AT")),
                NeedsMyApproval: needsMyApproval));
        }

        return records;
    }
}
