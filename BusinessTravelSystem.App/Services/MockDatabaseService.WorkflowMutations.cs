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
    public async Task<MockRecordMutationResult> ApproveTripAsync(long tripId, long approverTravellerId)
    {
        await InitializeAsync();

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        var activeProcess = FindActiveProcess(tripId);
        if (trip is null || activeProcess is null)
        {
            return new(false, "No active approval or workflow step was found.");
        }

        var now = DateTimeOffset.Now;
        var sequence = Number(activeProcess, "SEQUENCE_NO_SNAPSHOT");
        var processId = Number(activeProcess, "TRIP_PROCESS_ID");

        var duplicate = Rows("APPROVAL").Select(AsObject).Any(row =>
            Number(row, "TRIP_PROCESS_ID") == processId &&
            Number(row, "APPROVER_TRAVELLER_ID") == approverTravellerId);
        if (duplicate)
        {
            return new(false, "This approver has already acted on the current step.");
        }

        Rows("APPROVAL").Add(new JsonObject
        {
            ["APPROVAL_ID"] = NextId("APPROVAL", "APPROVAL_ID"),
            ["TRIP_PROCESS_ID"] = processId,
            ["APPROVER_TRAVELLER_ID"] = approverTravellerId,
            ["ACTION_ID"] = ActionId("APPROVE"),
            ["REMARKS"] = "Approved in the local prototype.",
            ["DECIDED_AT"] = Iso(now)
        });

        activeProcess["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", "COMPLETED");
        activeProcess["COMPLETED_AT"] = Iso(now);

        var nextProcess = FindProcess(tripId, sequence + 1);
        if (nextProcess is not null)
        {
            nextProcess["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", "ACTIVE");
            nextProcess["STARTED_AT"] ??= Iso(now);
        }

        var fromStatusId = Number(trip, "STATUS_ID");
        var toStatusId = fromStatusId;

        if (sequence >= 10 && LookupCode(fromStatusId).Equals("PENDING", StringComparison.OrdinalIgnoreCase))
        {
            toStatusId = LookupId("TRIP_STATUS", "APPROVED");
        }
        else if (sequence == 11)
        {
            toStatusId = LookupId("TRIP_STATUS", "UPCOMING");
        }
        else if (sequence == 13)
        {
            toStatusId = LookupId("TRIP_STATUS", "COMPLETED");
        }

        trip["STATUS_ID"] = toStatusId;
        trip["UPDATED_AT"] = Iso(now);

        AddTripAction(tripId, "APPROVE", approverTravellerId, fromStatusId, toStatusId,
            $"Completed workflow step {sequence}.");
        AddAudit("TRIP_PROCESS", processId, "APPROVE", null,
            new JsonObject { ["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", "COMPLETED") },
            approverTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }

    public async Task<MockRecordMutationResult> RejectTripAsync(
        long tripId,
        long approverTravellerId,
        string remarks)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(remarks))
        {
            return new(false, "A rejection reason is required.");
        }

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        var activeProcess = FindActiveProcess(tripId);
        if (trip is null || activeProcess is null)
        {
            return new(false, "No active approval or workflow step was found.");
        }

        var now = DateTimeOffset.Now;
        var processId = Number(activeProcess, "TRIP_PROCESS_ID");
        var fromStatusId = Number(trip, "STATUS_ID");
        var rejectedStatusId = LookupId("TRIP_STATUS", "REJECTED");

        Rows("APPROVAL").Add(new JsonObject
        {
            ["APPROVAL_ID"] = NextId("APPROVAL", "APPROVAL_ID"),
            ["TRIP_PROCESS_ID"] = processId,
            ["APPROVER_TRAVELLER_ID"] = approverTravellerId,
            ["ACTION_ID"] = ActionId("REJECT"),
            ["REMARKS"] = remarks.Trim(),
            ["DECIDED_AT"] = Iso(now)
        });

        activeProcess["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", "REJECTED");
        activeProcess["COMPLETED_AT"] = Iso(now);
        trip["STATUS_ID"] = rejectedStatusId;
        trip["UPDATED_AT"] = Iso(now);

        AddTripAction(tripId, "REJECT", approverTravellerId, fromStatusId, rejectedStatusId, remarks.Trim());
        AddAudit("TRIP_PROCESS", processId, "REJECT", null,
            new JsonObject { ["REMARKS"] = remarks.Trim() }, approverTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }

    public async Task<MockRecordMutationResult> CancelTripAsync(
        long tripId,
        long actorTravellerId,
        string reason)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(reason))
        {
            return new(false, "A cancellation reason is required.");
        }

        var trip = FindById("TRIP", "TRIP_ID", tripId);
        if (trip is null)
        {
            return new(false, "Trip not found.");
        }

        var currentCode = LookupCode(Number(trip, "STATUS_ID"));
        if (currentCode is "COMPLETED" or "CANCELLED")
        {
            return new(false, "The selected trip can no longer be cancelled.");
        }

        var now = DateTimeOffset.Now;
        var fromStatusId = Number(trip, "STATUS_ID");
        var cancelledStatusId = LookupId("TRIP_STATUS", "CANCELLED");
        var activeProcess = FindActiveProcess(tripId);

        if (activeProcess is not null)
        {
            activeProcess["STEP_STATUS_ID"] = LookupId("PROCESS_STATUS", "CANCELLED");
            activeProcess["COMPLETED_AT"] = Iso(now);
        }

        trip["STATUS_ID"] = cancelledStatusId;
        trip["UPDATED_AT"] = Iso(now);

        AddTripAction(tripId, "CANCEL", actorTravellerId, fromStatusId, cancelledStatusId, reason.Trim());
        AddAudit("TRIP", tripId, "CANCEL", new JsonObject { ["STATUS_ID"] = fromStatusId },
            new JsonObject { ["STATUS_ID"] = cancelledStatusId, ["REASON"] = reason.Trim() },
            actorTravellerId);
        await PersistAndNotifyAsync();

        return new(true, RecordId: tripId);
    }
}
