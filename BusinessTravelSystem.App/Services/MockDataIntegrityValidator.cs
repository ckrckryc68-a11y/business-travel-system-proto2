using System.Globalization;
using System.Text.Json.Nodes;

namespace BusinessTravelSystem.App.Services;

internal static class MockDataIntegrityValidator
{
    private static readonly string[] RequiredTables =
    {
        "APP_SETTING", "LOOKUP", "DEPARTMENT", "REGION", "ROLE", "TRAVELLER",
        "TRAVELLER_ROLE", "TRAVELLER_SESSION", "TRIP", "DESTINATION",
        "TRIP_DESTINATION", "FLIGHT", "ARRANGEMENT", "TRIP_EXPENSE", "TRIP_COMMENT",
        "ATTACHMENT", "WORKFLOW", "WORKFLOW_ACTION", "WORKFLOW_STEP", "TRIP_PROCESS",
        "APPROVAL", "TRIP_ACTION", "AUDIT_LOG"
    };

    public static void Validate(JsonObject database)
    {
        var errors = new List<string>();

        foreach (var table in RequiredTables)
        {
            if (database[table] is not JsonArray)
            {
                errors.Add($"Missing required mock table: {table}.");
            }
        }

        if (errors.Count > 0)
        {
            Throw(errors);
        }

        var departments = Ids(database, "DEPARTMENT", "DEPARTMENT_ID");
        var lookups = Ids(database, "LOOKUP", "LOOKUP_ID");
        var roles = Ids(database, "ROLE", "ROLE_ID");
        var travellers = Ids(database, "TRAVELLER", "TRAVELLER_ID");
        var trips = Ids(database, "TRIP", "TRIP_ID");
        var destinations = Ids(database, "DESTINATION", "DESTINATION_ID");
        var regions = Ids(database, "REGION", "REGION_ID");
        var actions = Ids(database, "WORKFLOW_ACTION", "ACTION_ID");
        var workflows = Ids(database, "WORKFLOW", "WORKFLOW_ID");
        var workflowSteps = Ids(database, "WORKFLOW_STEP", "WORKFLOW_STEP_ID");
        var processes = Ids(database, "TRIP_PROCESS", "TRIP_PROCESS_ID");
        var expenses = Ids(database, "TRIP_EXPENSE", "TRIP_EXPENSE_ID");
        var comments = Ids(database, "TRIP_COMMENT", "COMMENT_ID");

        RequireCount(database, "TRAVELLER", 20, 30, errors);
        RequireCount(database, "DEPARTMENT", 5, 10, errors);
        RequireCount(database, "TRIP", 30, 50, errors);
        RequireCount(database, "TRIP_EXPENSE", 80, 120, errors);
        RequireCount(database, "APPROVAL", 20, 40, errors);

        CheckUnique(database, "LOOKUP", errors, "LOOKUP_TYPE", "LOOKUP_CODE");
        CheckUnique(database, "DEPARTMENT", errors, "DEPARTMENT_CODE");
        CheckUnique(database, "ROLE", errors, "ROLE_CODE");
        CheckUnique(database, "TRAVELLER", errors, "EMPLOYEE_NO");
        CheckUnique(database, "TRAVELLER", errors, "EMAIL");
        CheckUnique(database, "TRAVELLER_ROLE", errors, "TRAVELLER_ID", "ROLE_ID");
        CheckUnique(database, "WORKFLOW", errors, "WORKFLOW_CODE", "VERSION_NO");
        CheckUnique(database, "WORKFLOW_STEP", errors, "WORKFLOW_ID", "SEQUENCE_NO");
        CheckUnique(database, "WORKFLOW_STEP", errors, "WORKFLOW_ID", "STEP_CODE");
        CheckUnique(database, "APPROVAL", errors, "TRIP_PROCESS_ID", "APPROVER_TRAVELLER_ID");

        var tripNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var trip in Rows(database, "TRIP"))
        {
            var tripNo = NullableText(trip, "TRIP_NO");
            if (!string.IsNullOrWhiteSpace(tripNo) && !tripNumbers.Add(tripNo))
            {
                errors.Add($"Duplicate TRIP.TRIP_NO: {tripNo}.");
            }

            RequireFk(trip, "APPLICANT_ID", travellers, "TRIP", errors);
            RequireFk(trip, "TRAVELLER_ID", travellers, "TRIP", errors);
            RequireFk(trip, "TRIP_CATEGORY_ID", lookups, "TRIP", errors);
            RequireFk(trip, "COST_CENTER_ID", lookups, "TRIP", errors);
            RequireFk(trip, "STATUS_ID", lookups, "TRIP", errors);
        }

        foreach (var row in Rows(database, "APP_SETTING"))
        {
            RequireFk(row, "UPDATED_BY_TRAVELLER_ID", travellers, "APP_SETTING", errors);
        }

        foreach (var row in Rows(database, "REGION"))
        {
            RequireFk(row, "CITY_ID", lookups, "REGION", errors);
            RequireFk(row, "PROVINCE_ID", lookups, "REGION", errors);
            RequireFk(row, "COUNTRY_ID", lookups, "REGION", errors);
        }

        foreach (var row in Rows(database, "ROLE"))
        {
            RequireFk(row, "MODULE_ID", lookups, "ROLE", errors);
        }

        foreach (var row in Rows(database, "TRAVELLER"))
        {
            RequireFk(row, "DEPARTMENT_ID", departments, "TRAVELLER", errors);
        }

        foreach (var row in Rows(database, "TRAVELLER_ROLE"))
        {
            RequireFk(row, "TRAVELLER_ID", travellers, "TRAVELLER_ROLE", errors);
            RequireFk(row, "ROLE_ID", roles, "TRAVELLER_ROLE", errors);
            RequireFk(row, "ASSIGNED_BY_TRAVELLER_ID", travellers, "TRAVELLER_ROLE", errors);
            RequireNullableFk(row, "REVOKED_BY_TRAVELLER_ID", travellers, "TRAVELLER_ROLE", errors);
        }

        foreach (var row in Rows(database, "TRAVELLER_SESSION"))
        {
            RequireFk(row, "TRAVELLER_ID", travellers, "TRAVELLER_SESSION", errors);
        }

        foreach (var row in Rows(database, "DESTINATION"))
        {
            RequireFk(row, "REGION_ID", regions, "DESTINATION", errors);
        }

        foreach (var row in Rows(database, "TRIP_DESTINATION"))
        {
            RequireFk(row, "TRIP_ID", trips, "TRIP_DESTINATION", errors);
            RequireFk(row, "DESTINATION_ID", destinations, "TRIP_DESTINATION", errors);
            CheckChronology(row, "ARRIVAL_AT", "DEPARTURE_AT", "TRIP_DESTINATION", allowEqual: false, errors);
        }

        foreach (var row in Rows(database, "FLIGHT"))
        {
            RequireFk(row, "TRIP_ID", trips, "FLIGHT", errors);
            RequireFk(row, "DEPARTURE_DESTINATION_ID", destinations, "FLIGHT", errors);
            RequireFk(row, "ARRIVAL_DESTINATION_ID", destinations, "FLIGHT", errors);
            RequireFk(row, "FLIGHT_TYPE_ID", lookups, "FLIGHT", errors);
            CheckChronology(row, "DEPARTURE_AT", "ARRIVAL_AT", "FLIGHT", allowEqual: false, errors);
        }

        foreach (var row in Rows(database, "ARRANGEMENT"))
        {
            RequireFk(row, "TRIP_ID", trips, "ARRANGEMENT", errors);
            RequireFk(row, "DESTINATION_ID", destinations, "ARRANGEMENT", errors);
            RequireFk(row, "ARRANGEMENT_TYPE_ID", lookups, "ARRANGEMENT", errors);
            RequireFk(row, "ARRANGEMENT_STATUS_ID", lookups, "ARRANGEMENT", errors);
        }

        var expensesPerTrip = new Dictionary<long, int>();
        foreach (var row in Rows(database, "TRIP_EXPENSE"))
        {
            var tripId = Number(row, "TRIP_ID");
            RequireFk(row, "TRIP_ID", trips, "TRIP_EXPENSE", errors);
            RequireFk(row, "EXPENSE_TYPE_ID", lookups, "TRIP_EXPENSE", errors);
            RequireFk(row, "EXPENSE_STAGE_ID", lookups, "TRIP_EXPENSE", errors);
            RequireFk(row, "CREATED_BY_TRAVELLER_ID", travellers, "TRIP_EXPENSE", errors);

            var expectedBase = Math.Round(Decimal(row, "ORIGINAL_AMOUNT") * Decimal(row, "EXCHANGE_RATE"), 2);
            if (Decimal(row, "BASE_AMOUNT") != expectedBase)
            {
                errors.Add($"TRIP_EXPENSE {Number(row, "TRIP_EXPENSE_ID")} has an inconsistent base amount.");
            }

            expensesPerTrip[tripId] = expensesPerTrip.GetValueOrDefault(tripId) + 1;
        }

        if (!expensesPerTrip.Values.Any(count => count >= 3))
        {
            errors.Add("No trip contains multiple expense records.");
        }

        foreach (var row in Rows(database, "TRIP_COMMENT"))
        {
            RequireFk(row, "TRIP_ID", trips, "TRIP_COMMENT", errors);
            RequireFk(row, "CREATED_BY_TRAVELLER_ID", travellers, "TRIP_COMMENT", errors);
        }

        foreach (var row in Rows(database, "ATTACHMENT"))
        {
            var parentCount = new[]
            {
                row["TRIP_ID"], row["TRAVELLER_ID"], row["TRIP_EXPENSE_ID"], row["COMMENT_ID"]
            }.Count(value => value is not null);

            if (parentCount != 1)
            {
                errors.Add($"ATTACHMENT {Number(row, "ATTACHMENT_ID")} must have exactly one parent.");
            }

            RequireNullableFk(row, "TRIP_ID", trips, "ATTACHMENT", errors);
            RequireNullableFk(row, "TRAVELLER_ID", travellers, "ATTACHMENT", errors);
            RequireNullableFk(row, "TRIP_EXPENSE_ID", expenses, "ATTACHMENT", errors);
            RequireNullableFk(row, "COMMENT_ID", comments, "ATTACHMENT", errors);
            RequireFk(row, "UPLOADED_BY_TRAVELLER_ID", travellers, "ATTACHMENT", errors);
            RequireNullableFk(row, "DELETED_BY_TRAVELLER_ID", travellers, "ATTACHMENT", errors);
        }

        foreach (var row in Rows(database, "WORKFLOW"))
        {
            RequireNullableFk(row, "TRIP_CATEGORY_ID", lookups, "WORKFLOW", errors);
        }

        foreach (var row in Rows(database, "WORKFLOW_STEP"))
        {
            RequireFk(row, "WORKFLOW_ID", workflows, "WORKFLOW_STEP", errors);
            RequireFk(row, "APPROVER_ROLE_ID", roles, "WORKFLOW_STEP", errors);
            RequireFk(row, "REQUIRED_ACTION_ID", actions, "WORKFLOW_STEP", errors);
        }

        foreach (var row in Rows(database, "TRIP_PROCESS"))
        {
            RequireFk(row, "TRIP_ID", trips, "TRIP_PROCESS", errors);
            RequireFk(row, "WORKFLOW_STEP_ID", workflowSteps, "TRIP_PROCESS", errors);
            RequireFk(row, "STEP_STATUS_ID", lookups, "TRIP_PROCESS", errors);
            RequireNullableFk(row, "ASSIGNED_TO_TRAVELLER_ID", travellers, "TRIP_PROCESS", errors);

            if (row["COMPLETED_AT"] is not null)
            {
                CheckChronology(row, "STARTED_AT", "COMPLETED_AT", "TRIP_PROCESS", allowEqual: true, errors);
            }
        }

        foreach (var row in Rows(database, "APPROVAL"))
        {
            RequireFk(row, "TRIP_PROCESS_ID", processes, "APPROVAL", errors);
            RequireFk(row, "APPROVER_TRAVELLER_ID", travellers, "APPROVAL", errors);
            RequireFk(row, "ACTION_ID", actions, "APPROVAL", errors);
        }

        foreach (var row in Rows(database, "TRIP_ACTION"))
        {
            RequireFk(row, "TRIP_ID", trips, "TRIP_ACTION", errors);
            RequireFk(row, "ACTION_ID", actions, "TRIP_ACTION", errors);
            RequireFk(row, "ACTION_BY_TRAVELLER_ID", travellers, "TRIP_ACTION", errors);
            RequireFk(row, "FROM_STATUS_ID", lookups, "TRIP_ACTION", errors);
            RequireFk(row, "TO_STATUS_ID", lookups, "TRIP_ACTION", errors);
        }

        foreach (var row in Rows(database, "AUDIT_LOG"))
        {
            RequireFk(row, "PERFORMED_BY_TRAVELLER_ID", travellers, "AUDIT_LOG", errors);
        }

        var administrator = Rows(database, "TRAVELLER").FirstOrDefault(row =>
            Text(row, "EMPLOYEE_NO").Equals("01023712", StringComparison.OrdinalIgnoreCase));

        if (administrator is null ||
            !Text(administrator, "EMAIL").Equals("marqpaulgonzales22@gmail.com", StringComparison.OrdinalIgnoreCase) ||
            !Text(administrator, "FULL_NAME").Equals("Marq Paul Gonzales", StringComparison.Ordinal))
        {
            errors.Add("The required default mock administrator record is missing or incorrect.");
        }

        if (errors.Count > 0)
        {
            Throw(errors);
        }
    }

    private static IReadOnlyList<JsonObject> Rows(JsonObject database, string tableName) =>
        database[tableName]!.AsArray().Select(node => node!.AsObject()).ToArray();

    private static HashSet<long> Ids(JsonObject database, string table, string key) =>
        Rows(database, table).Select(row => Number(row, key)).ToHashSet();

    private static void RequireCount(
        JsonObject database,
        string table,
        int minimum,
        int maximum,
        List<string> errors)
    {
        var count = database[table]!.AsArray().Count;
        if (count < minimum || count > maximum)
        {
            errors.Add($"{table} contains {count} rows; expected {minimum}–{maximum}.");
        }
    }

    private static void CheckUnique(
        JsonObject database,
        string table,
        List<string> errors,
        params string[] fields)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows(database, table))
        {
            var key = string.Join("|", fields.Select(field => row[field]?.ToJsonString() ?? "null"));
            if (!seen.Add(key))
            {
                errors.Add($"{table} contains a duplicate key for {string.Join(", ", fields)}.");
            }
        }
    }

    private static void RequireFk(
        JsonObject row,
        string field,
        HashSet<long> targetIds,
        string table,
        List<string> errors)
    {
        var value = Number(row, field);
        if (!targetIds.Contains(value))
        {
            errors.Add($"{table}.{field} references missing ID {value}.");
        }
    }

    private static void RequireNullableFk(
        JsonObject row,
        string field,
        HashSet<long> targetIds,
        string table,
        List<string> errors)
    {
        if (row[field] is null)
        {
            return;
        }

        RequireFk(row, field, targetIds, table, errors);
    }

    private static void CheckChronology(
        JsonObject row,
        string startField,
        string endField,
        string table,
        bool allowEqual,
        List<string> errors)
    {
        if (!DateTimeOffset.TryParse(Text(row, startField), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var start) ||
            !DateTimeOffset.TryParse(Text(row, endField), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var end))
        {
            errors.Add($"{table} has an invalid {startField}/{endField} timestamp.");
            return;
        }

        if (allowEqual ? end < start : end <= start)
        {
            errors.Add($"{table} has {endField} before {startField}.");
        }
    }

    private static long Number(JsonObject row, string field) =>
        row[field]?.GetValue<long>() ?? 0;

    private static decimal Decimal(JsonObject row, string field) =>
        row[field]?.GetValue<decimal>() ?? 0m;

    private static string Text(JsonObject row, string field) =>
        row[field]?.GetValue<string>() ?? string.Empty;

    private static string? NullableText(JsonObject row, string field) =>
        row[field] is null ? null : row[field]!.GetValue<string>();

    private static void Throw(IEnumerable<string> errors) =>
        throw new InvalidOperationException(
            "Mock seed validation failed:" + Environment.NewLine +
            string.Join(Environment.NewLine, errors.Select(error => $"- {error}")));
}
