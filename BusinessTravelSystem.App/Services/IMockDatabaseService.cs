using System.Text.Json.Nodes;
using BusinessTravelSystem.App.Models;

namespace BusinessTravelSystem.App.Services;

public interface IMockDatabaseService
{
    event Action? Changed;

    bool IsInitialized { get; }

    Task InitializeAsync();
    Task ResetAsync();
    Task<JsonObject> GetSnapshotAsync();
    Task<MockCredentialRecord?> FindCredentialAsync(string employeeNoOrEmail);
    Task<MockCredentialRecord?> GetDefaultAdministratorAsync();
    Task<IReadOnlyList<MockTripSearchRecord>> GetTripSearchRecordsAsync(long currentTravellerId, bool isAdministrator);
    Task<MockRecordMutationResult> CreateTripAsync(MockTripDraft draft, long actorTravellerId);
    Task<MockRecordMutationResult> UpdateTripAsync(long tripId, JsonObject changes, long actorTravellerId);
    Task<MockRecordMutationResult> DeleteDraftTripAsync(long tripId, long actorTravellerId, bool isAdministrator);
    Task<MockRecordMutationResult> SubmitTripAsync(long tripId, long actorTravellerId);
    Task<MockRecordMutationResult> ApproveTripAsync(long tripId, long approverTravellerId);
    Task<MockRecordMutationResult> RejectTripAsync(long tripId, long approverTravellerId, string remarks);
    Task<MockRecordMutationResult> CancelTripAsync(long tripId, long actorTravellerId, string reason);
}
