using System.Text.Json.Nodes;

namespace BusinessTravelSystem.App.Models;

public sealed record MockLoginResult(bool Succeeded, string? ErrorMessage = null);

public sealed record MockCredentialRecord(
    long TravellerId,
    string EmployeeNo,
    string Email,
    string FullName,
    string? Nickname,
    string StoredMockPassword,
    string[] RoleCodes,
    string[] RoleNames);

public sealed record MockSignedInUser(
    long TravellerId,
    string EmployeeNo,
    string Email,
    string FullName,
    string? Nickname,
    string[] RoleCodes,
    string[] RoleNames,
    bool RememberMe,
    DateTimeOffset SignedInAt)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? FullName : Nickname;
    public bool IsAdministrator => RoleCodes.Contains("ADMIN", StringComparer.OrdinalIgnoreCase);
    public string PrimaryRoleName => RoleNames.FirstOrDefault() ?? "User";
}

public sealed record MockTripSearchRecord(
    long TripId,
    string TripNumber,
    string Traveler,
    string Destination,
    string Category,
    string Status,
    string Summary,
    string Title,
    string Purpose,
    string BudgetAllocation,
    string CostCenter,
    string ContractRequired,
    string Description,
    string Applicant,
    string Department,
    string DateFiled,
    string TravelDates,
    string Venue,
    decimal EstimatedCost,
    string CostBreakdown,
    string Reviewer,
    string LastUpdated,
    bool NeedsMyApproval);

public sealed record MockTripDraft(
    string Title,
    string Purpose,
    long TravellerId,
    long TripCategoryId,
    long CostCenterId,
    long DestinationId,
    DateTimeOffset ArrivalAt,
    DateTimeOffset DepartureAt,
    string? BudgetJustification = null);

public sealed record MockRecordMutationResult(bool Succeeded, string? ErrorMessage = null, long? RecordId = null);
