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
    public async Task<MockRecordMutationResult> CreateTripAsync(MockTripDraft draft, long actorTravellerId)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(draft.Title) || string.IsNullOrWhiteSpace(draft.Purpose))
        {
            return new(false, "Title and purpose are required.");
        }

        if (draft.DepartureAt < draft.ArrivalAt)
        {
            return new(false, "Departure cannot be earlier than arrival.");
        }

        if (!Rows("TRAVELLER").Select(AsObject).Any(row =>
                Number(row, "TRAVELLER_ID") == draft.TravellerId && IsActive(row)))
        {
            return new(false, "The selected traveller does not exist or is inactive.");
        }

        var now = DateTimeOffset.Now;
        var tripId = NextId("TRIP", "TRIP_ID");
        var applicationNo = $"APP-{now:yyyy}-{tripId:000000}";
        var draftStatusId = LookupId("TRIP_STATUS", "DRAFT");
        var processNotStartedId = LookupId("PROCESS_STATUS", "NOT_STARTED");
        var processActiveId = LookupId("PROCESS_STATUS", "ACTIVE");

        var trip = new JsonObject
        {
            ["TRIP_ID"] = tripId,
            ["APPLICATION_NO"] = applicationNo,
            ["TRIP_NO"] = null,
            ["APPLICANT_ID"] = actorTravellerId,
            ["TRAVELLER_ID"] = draft.TravellerId,
            ["IS_PRIMARY"] = "Y",
            ["TITLE"] = draft.Title.Trim(),
            ["PURPOSE"] = draft.Purpose.Trim(),
            ["TRIP_CATEGORY_ID"] = draft.TripCategoryId,
            ["BUDGET_JUSTIFICATION"] = NullIfWhiteSpace(draft.BudgetJustification),
            ["COST_CENTER_ID"] = draft.CostCenterId,
            ["STATUS_ID"] = draftStatusId,
            ["SUBMITTED_AT"] = null,
            ["CREATED_AT"] = Iso(now),
            ["UPDATED_AT"] = null
        };

        Rows("TRIP").Add(trip);

        Rows("TRIP_DESTINATION").Add(new JsonObject
        {
            ["TRIP_DESTINATION_ID"] = NextId("TRIP_DESTINATION", "TRIP_DESTINATION_ID"),
            ["TRIP_ID"] = tripId,
            ["DESTINATION_ID"] = draft.DestinationId,
            ["ARRIVAL_AT"] = Iso(draft.ArrivalAt),
            ["DEPARTURE_AT"] = Iso(draft.DepartureAt),
            ["REMARKS"] = null,
            ["CREATED_AT"] = Iso(now),
            ["UPDATED_AT"] = null
        });

        foreach (var step in Rows("WORKFLOW_STEP").Select(AsObject).OrderBy(row => Number(row, "SEQUENCE_NO")))
        {
            var sequence = Number(step, "SEQUENCE_NO");
            Rows("TRIP_PROCESS").Add(new JsonObject
            {
                ["TRIP_PROCESS_ID"] = NextId("TRIP_PROCESS", "TRIP_PROCESS_ID"),
                ["TRIP_ID"] = tripId,
                ["WORKFLOW_STEP_ID"] = Number(step, "WORKFLOW_STEP_ID"),
                ["SEQUENCE_NO_SNAPSHOT"] = sequence,
                ["STEP_STATUS_ID"] = sequence == 1 ? processActiveId : processNotStartedId,
                ["ASSIGNED_TO_TRAVELLER_ID"] = sequence == 1
                    ? draft.TravellerId
                    : FindApproverForRole(Number(step, "APPROVER_ROLE_ID")),
                ["STARTED_AT"] = sequence == 1 ? Iso(now) : null,
                ["COMPLETED_AT"] = null
            });
        }

        AddTripAction(tripId, "CREATE", actorTravellerId, draftStatusId, draftStatusId, "Draft application created.");
        AddAudit("TRIP", tripId, "CREATE", null, trip, actorTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }

    public async Task<MockRecordMutationResult> UpdateTripAsync(long tripId, JsonObject changes, long actorTravellerId)
    {
        await InitializeAsync();

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        if (trip is null)
        {
            return new(false, "Trip not found.");
        }

        var statusCode = LookupCode(Number(trip, "STATUS_ID"));
        if (!statusCode.Equals("DRAFT", StringComparison.OrdinalIgnoreCase) &&
            !statusCode.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
        {
            return new(false, "Only draft or pending prototype trips can be edited.");
        }

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TITLE", "PURPOSE", "TRIP_CATEGORY_ID", "BUDGET_JUSTIFICATION", "COST_CENTER_ID"
        };

        var oldValues = new JsonObject();
        var newValues = new JsonObject();

        foreach (var change in changes)
        {
            if (!allowed.Contains(change.Key))
            {
                continue;
            }

            oldValues[change.Key] = trip[change.Key]?.DeepClone();
            trip[change.Key] = change.Value?.DeepClone();
            newValues[change.Key] = change.Value?.DeepClone();
        }

        if (newValues.Count == 0)
        {
            return new(false, "No supported fields were supplied.");
        }

        trip["UPDATED_AT"] = Iso(DateTimeOffset.Now);
        AddAudit("TRIP", tripId, "UPDATE", oldValues, newValues, actorTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }

    public async Task<MockRecordMutationResult> DeleteDraftTripAsync(
        long tripId,
        long actorTravellerId,
        bool isAdministrator)
    {
        await InitializeAsync();

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        if (trip is null)
        {
            return new(false, "Trip not found.");
        }

        if (!LookupCode(Number(trip, "STATUS_ID")).Equals("DRAFT", StringComparison.OrdinalIgnoreCase) ||
            NullableText(trip, "SUBMITTED_AT") is not null)
        {
            return new(false, "Only an unsubmitted draft may be deleted.");
        }

        if (!isAdministrator && Number(trip, "APPLICANT_ID") != actorTravellerId)
        {
            return new(false, "Only the draft owner or an administrator may delete this record.");
        }

        var relatedExpenseIds = Rows("TRIP_EXPENSE").Select(AsObject)
            .Where(row => Number(row, "TRIP_ID") == tripId)
            .Select(row => Number(row, "TRIP_EXPENSE_ID"))
            .ToHashSet();
        var relatedCommentIds = Rows("TRIP_COMMENT").Select(AsObject)
            .Where(row => Number(row, "TRIP_ID") == tripId)
            .Select(row => Number(row, "COMMENT_ID"))
            .ToHashSet();
        var relatedProcessIds = Rows("TRIP_PROCESS").Select(AsObject)
            .Where(row => Number(row, "TRIP_ID") == tripId)
            .Select(row => Number(row, "TRIP_PROCESS_ID"))
            .ToHashSet();

        RemoveWhere("ATTACHMENT", row =>
            NullableNumber(row, "TRIP_ID") == tripId ||
            (NullableNumber(row, "TRIP_EXPENSE_ID") is { } expenseId && relatedExpenseIds.Contains(expenseId)) ||
            (NullableNumber(row, "COMMENT_ID") is { } commentId && relatedCommentIds.Contains(commentId)));
        RemoveWhere("APPROVAL", row => relatedProcessIds.Contains(Number(row, "TRIP_PROCESS_ID")));

        foreach (var table in new[]
                 {
                     "TRIP_DESTINATION", "FLIGHT", "ARRANGEMENT", "TRIP_EXPENSE",
                     "TRIP_COMMENT", "TRIP_PROCESS", "TRIP_ACTION"
                 })
        {
            RemoveWhere(table, row => Number(row, "TRIP_ID") == tripId);
        }

        RemoveWhere("TRIP", row => Number(row, "TRIP_ID") == tripId);

        AddAudit("TRIP", tripId, "DELETE_DRAFT", trip, null, actorTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }

    public async Task<MockRecordMutationResult> SubmitTripAsync(long tripId, long actorTravellerId)
    {
        await InitializeAsync();

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        if (trip is null)
        {
            return new(false, "Trip not found.");
        }

        var draftStatusId = LookupId("TRIP_STATUS", "DRAFT");
        var pendingStatusId = LookupId("TRIP_STATUS", "PENDING");

        if (Number(trip, "STATUS_ID") != draftStatusId)
        {
            return new(false, "Only a draft can be submitted.");
        }

        var now = DateTimeOffset.Now;
        trip["TRIP_NO"] = $"BTS-{now:yyyy}-{tripId:000000}";
        trip["STATUS_ID"] = pendingStatusId;
        trip["SUBMITTED_AT"] = Iso(now);
        trip["UPDATED_AT"] = Iso(now);

        MoveProcess(tripId, 1, "COMPLETED", now);
        MoveProcess(tripId, 2, "ACTIVE", now);

        AddTripAction(tripId, "SUBMIT", actorTravellerId, draftStatusId, pendingStatusId, null);
        AddAudit("TRIP", tripId, "SUBMIT", new JsonObject { ["STATUS_ID"] = draftStatusId },
            new JsonObject { ["STATUS_ID"] = pendingStatusId }, actorTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }
}
