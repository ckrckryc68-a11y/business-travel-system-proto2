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
    private async Task<JsonObject> LoadSeedAsync()
    {
        var merged = new JsonObject();

        foreach (var file in SeedFiles)
        {
            var envelopeJson = await _httpClient.GetStringAsync(file);
            var envelope = JsonNode.Parse(envelopeJson)?.AsObject()
                           ?? throw new InvalidOperationException($"Mock seed file '{file}' is invalid.");
            var compressed = Convert.FromBase64String(Text(envelope, "data"));
            await using var source = new MemoryStream(compressed);
            await using var gzip = new GZipStream(source, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var shard = JsonNode.Parse(json)?.AsObject()
                        ?? throw new InvalidOperationException($"Mock seed payload '{file}' is invalid.");

            foreach (var table in shard)
            {
                merged[table.Key] = table.Value?.DeepClone();
            }
        }

        return merged;
    }

    private static JsonObject? TryParseDatabase(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(json)?.AsObject();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private MockCredentialRecord BuildCredential(JsonObject traveller)
    {
        var travellerId = Number(traveller, "TRAVELLER_ID");
        var activeAssignments = Rows("TRAVELLER_ROLE")
            .Select(AsObject)
            .Where(row =>
                Number(row, "TRAVELLER_ID") == travellerId &&
                row["REVOKED_AT"] is null)
            .ToArray();

        var roles = Rows("ROLE")
            .Select(AsObject)
            .Where(IsActive)
            .Where(role => activeAssignments.Any(assignment =>
                Number(assignment, "ROLE_ID") == Number(role, "ROLE_ID")))
            .ToArray();

        return new MockCredentialRecord(
            TravellerId: travellerId,
            EmployeeNo: Text(traveller, "EMPLOYEE_NO"),
            Email: Text(traveller, "EMAIL"),
            FullName: Text(traveller, "FULL_NAME"),
            Nickname: NullableText(traveller, "NICKNAME"),
            StoredMockPassword: Text(traveller, "PIN_HASH"),
            RoleCodes: roles.Select(role => Text(role, "ROLE_CODE")).ToArray(),
            RoleNames: roles.Select(role => Text(role, "ROLE_NAME")).ToArray());
    }

    private JsonObject GetDatabase() =>
        _database ?? throw new InvalidOperationException("Mock database has not been initialized.");

    private JsonArray Rows(string tableName) =>
        GetDatabase()[tableName]?.AsArray()
        ?? throw new InvalidOperationException($"Mock table '{tableName}' is missing.");

    private JsonObject? FindById(string tableName, string primaryKey, long id) =>
        Rows(tableName).Select(AsObject).FirstOrDefault(row => Number(row, primaryKey) == id);

    private JsonObject? FindActiveProcess(long tripId)
    {
        var activeStatusId = LookupId("PROCESS_STATUS", "ACTIVE");
        return Rows("TRIP_PROCESS").Select(AsObject)
            .Where(row => Number(row, "TRIP_ID") == tripId && Number(row, "STEP_STATUS_ID") == activeStatusId)
            .OrderBy(row => Number(row, "SEQUENCE_NO_SNAPSHOT"))
            .FirstOrDefault();
    }

    private JsonObject? FindProcess(long tripId, long sequence) =>
        Rows("TRIP_PROCESS").Select(AsObject).FirstOrDefault(row =>
            Number(row, "TRIP_ID") == tripId &&
            Number(row, "SEQUENCE_NO_SNAPSHOT") == sequence);

    private void MoveProcess(long tripId, long sequence, string processStatusCode, DateTimeOffset changedAt)
    {
        var process = FindProcess(tripId, sequence);
        if (process is null)
        {
            return;
        }

        process["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", processStatusCode);

        if (processStatusCode.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            process["STARTED_AT"] ??= Iso(changedAt);
            process["COMPLETED_AT"] = null;
        }
        else
        {
            process["STARTED_AT"] ??= Iso(changedAt);
            process["COMPLETED_AT"] = Iso(changedAt);
        }
    }

    private long FindApproverForRole(long roleId)
    {
        var roleAssignments = Rows("TRAVELLER_ROLE")
            .Select(AsObject)
            .Where(row => Number(row, "ROLE_ID") == roleId && row["REVOKED_AT"] is null)
            .ToArray();

        foreach (var assignment in roleAssignments)
        {
            var traveller = FindById("TRAVELLER", "TRAVELLER_ID", Number(assignment, "TRAVELLER_ID"));
            if (traveller is not null && IsActive(traveller))
            {
                return Number(traveller, "TRAVELLER_ID");
            }
        }

        return 1;
    }

    private long LookupId(string lookupType, string lookupCode) =>
        Number(Rows("LOOKUP").Select(AsObject).First(row =>
            EqualsText(Text(row, "LOOKUP_TYPE"), lookupType) &&
            EqualsText(Text(row, "LOOKUP_CODE"), lookupCode)), "LOOKUP_ID");

    private string LookupCode(long lookupId)
    {
        var lookup = FindById("LOOKUP", "LOOKUP_ID", lookupId);
        return lookup is null ? string.Empty : Text(lookup, "LOOKUP_CODE");
    }

    private long ActionId(string actionCode) =>
        Number(Rows("WORKFLOW_ACTION").Select(AsObject).First(row =>
            EqualsText(Text(row, "ACTION_CODE"), actionCode)), "ACTION_ID");

    private long NextId(string tableName, string primaryKey)
    {
        var rows = Rows(tableName).Select(AsObject).ToArray();
        return rows.Length == 0 ? 1 : rows.Max(row => Number(row, primaryKey)) + 1;
    }

    private void AddTripAction(
        long tripId,
        string actionCode,
        long actorTravellerId,
        long fromStatusId,
        long toStatusId,
        string? remarks)
    {
        Rows("TRIP_ACTION").Add(new JsonObject
        {
            ["TRIP_ACTION_ID"] = NextId("TRIP_ACTION", "TRIP_ACTION_ID"),
            ["TRIP_ID"] = tripId,
            ["ACTION_ID"] = ActionId(actionCode),
            ["ACTION_BY_TRAVELLER_ID"] = actorTravellerId,
            ["ACTION_AT"] = Iso(DateTimeOffset.Now),
            ["REMARKS"] = remarks,
            ["FROM_STATUS_ID"] = fromStatusId,
            ["TO_STATUS_ID"] = toStatusId
        });
    }

    private void AddAudit(
        string entityType,
        long entityId,
        string actionCode,
        JsonNode? oldValues,
        JsonNode? newValues,
        long actorTravellerId)
    {
        Rows("AUDIT_LOG").Add(new JsonObject
        {
            ["AUDIT_LOG_ID"] = NextId("AUDIT_LOG", "AUDIT_LOG_ID"),
            ["ENTITY_TYPE"] = entityType,
            ["ENTITY_ID"] = entityId,
            ["ACTION_CODE"] = actionCode,
            ["OLD_VALUES"] = oldValues?.ToJsonString(),
            ["NEW_VALUES"] = newValues?.ToJsonString(),
            ["PERFORMED_BY_TRAVELLER_ID"] = actorTravellerId,
            ["PERFORMED_AT"] = Iso(DateTimeOffset.Now),
            ["CLIENT_IP"] = "127.0.0.1"
        });
    }

    private void RemoveWhere(string tableName, Func<JsonObject, bool> predicate)
    {
        var rows = Rows(tableName);
        for (var index = rows.Count - 1; index >= 0; index--)
        {
            if (rows[index] is JsonObject row && predicate(row))
            {
                rows.RemoveAt(index);
            }
        }
    }

    private async Task PersistAndNotifyAsync()
    {
        await PersistAsync();
        Changed?.Invoke();
    }

    private Task PersistAsync() =>
        _jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            StorageKey,
            GetDatabase().ToJsonString(new JsonSerializerOptions { WriteIndented = false })).AsTask();

    private static JsonObject AsObject(JsonNode? node) =>
        node?.AsObject() ?? throw new InvalidOperationException("Expected a JSON object row.");

    private static string Text(JsonObject row, string propertyName) =>
        row[propertyName]?.GetValue<string>() ?? string.Empty;

    private static string? NullableText(JsonObject row, string propertyName) =>
        row[propertyName] is null ? null : row[propertyName]!.GetValue<string>();

    private static long Number(JsonObject row, string propertyName) =>
        row[propertyName]?.GetValue<long>() ?? 0;

    private static long? NullableNumber(JsonObject row, string propertyName) =>
        row[propertyName] is null ? null : row[propertyName]!.GetValue<long>();

    private static decimal Decimal(JsonObject row, string propertyName) =>
        row[propertyName]?.GetValue<decimal>() ?? 0m;

    private static DateTimeOffset? Date(JsonObject row, string propertyName)
    {
        var value = NullableText(row, propertyName);
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }

    private static bool IsActive(JsonObject row) =>
        Text(row, "IS_ACTIVE").Equals("Y", StringComparison.OrdinalIgnoreCase);

    private static bool EqualsText(string left, string right) =>
        left.Equals(right, StringComparison.OrdinalIgnoreCase);

    private static string Iso(DateTimeOffset value) =>
        value.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatDate(DateTimeOffset? value) =>
        value?.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) ?? "Not set";

    private static string FormatDateRange(DateTimeOffset? start, DateTimeOffset? end)
    {
        if (start is null)
        {
            return "Dates not set";
        }

        if (end is null || start.Value.Date == end.Value.Date)
        {
            return start.Value.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
        }

        return $"{start.Value:dd MMM}–{end.Value:dd MMM yyyy}";
    }
}
