using System.Text.Json;
using BusinessTravelSystem.App.Models;
using Microsoft.JSInterop;

namespace BusinessTravelSystem.App.Services;

public sealed class AuthSessionService : IMockAuthenticationService
{
    private const string SessionStorageKey = "bts.prototype.currentUser.v1";
    private const string MockPasswordMarkerPrefix = "INSECURE_MOCK_PLAINTEXT:";

    private readonly IMockDatabaseService _database;
    private readonly IJSRuntime _jsRuntime;
    private MockSignedInUser? _currentUser;

    public AuthSessionService(IMockDatabaseService database, IJSRuntime jsRuntime)
    {
        _database = database;
        _jsRuntime = jsRuntime;
    }

    public event Action? Changed;

    public bool IsAuthenticated => _currentUser is not null;
    public MockSignedInUser? CurrentUser => _currentUser;
    public string? UserName => _currentUser?.FullName;
    public bool RememberMe => _currentUser?.RememberMe ?? false;
    public DateTimeOffset? SignedInAt => _currentUser?.SignedInAt;
    public bool IsAdministrator => _currentUser?.IsAdministrator ?? false;
    public string PrimaryRoleName => _currentUser?.PrimaryRoleName ?? "User";

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        var stored = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", SessionStorageKey);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            try
            {
                var restored = JsonSerializer.Deserialize<MockSignedInUser>(stored);
                if (restored is not null)
                {
                    var currentCredential = await _database.FindCredentialAsync(restored.EmployeeNo);
                    if (currentCredential is not null)
                    {
                        _currentUser = restored with
                        {
                            FullName = currentCredential.FullName,
                            Nickname = currentCredential.Nickname,
                            Email = currentCredential.Email,
                            RoleCodes = currentCredential.RoleCodes,
                            RoleNames = currentCredential.RoleNames
                        };
                        Changed?.Invoke();
                        return;
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore corrupted local prototype state and restore the default administrator below.
            }
        }

        await RestoreDefaultAdministratorAsync();
    }

    public async Task<MockLoginResult> SignInAsync(
        string employeeNoOrEmail,
        string password,
        bool rememberMe)
    {
        var credential = await _database.FindCredentialAsync(employeeNoOrEmail);
        if (credential is null)
        {
            return new(false, "No active mock account matches that employee number or email address.");
        }

        // INSECURE MOCK ONLY: the prototype intentionally compares the local password directly.
        // Never copy this mechanism or the seed marker into production authentication.
        var expectedPassword = credential.StoredMockPassword.StartsWith(
            MockPasswordMarkerPrefix,
            StringComparison.Ordinal)
            ? credential.StoredMockPassword[MockPasswordMarkerPrefix.Length..]
            : credential.StoredMockPassword;

        if (!string.Equals(password, expectedPassword, StringComparison.Ordinal))
        {
            return new(false, "The mock password is incorrect.");
        }

        await SetCurrentUserAsync(credential, rememberMe);
        return new(true);
    }

    public async Task SignOutAsync()
    {
        _currentUser = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionStorageKey);
        Changed?.Invoke();
    }

    public void SignOut()
    {
        _currentUser = null;
        _ = _jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionStorageKey);
        Changed?.Invoke();
    }

    public async Task ResetMockDatabaseAsync()
    {
        await _database.ResetAsync();
        await RestoreDefaultAdministratorAsync();
    }

    public bool CanAccessModule(string moduleKey)
    {
        if (IsAdministrator)
        {
            return true;
        }

        return moduleKey.Trim().ToLowerInvariant() switch
        {
            "find" => HasAnyRole("COMMON", "SUPERIOR", "GA", "HR", "ACCOUNTING",
                "VICE_PRESIDENT", "CEO", "HRD", "CLINIC", "READ_ONLY"),
            "apply" => HasAnyRole("COMMON"),
            "approval" => HasAnyRole("SUPERIOR", "HR", "ACCOUNTING", "VICE_PRESIDENT", "CEO"),
            "reports" => HasAnyRole("HRD", "ACCOUNTING"),
            _ => false
        };
    }

    public bool Can(string permission)
    {
        if (IsAdministrator)
        {
            return true;
        }

        return permission.Trim().ToLowerInvariant() switch
        {
            "trips.view" => IsAuthenticated,
            "trips.create" => HasAnyRole("COMMON"),
            "trips.edit" => HasAnyRole("COMMON", "SUPERIOR"),
            "trips.delete" => HasAnyRole("COMMON"),
            "trips.submit" => HasAnyRole("COMMON"),
            "trips.approve" => HasAnyRole("SUPERIOR", "HR", "ACCOUNTING", "VICE_PRESIDENT", "CEO"),
            "trips.reject" => HasAnyRole("SUPERIOR", "HR", "ACCOUNTING", "VICE_PRESIDENT", "CEO"),
            "trips.cancel" => HasAnyRole("COMMON", "GA"),
            "arrangements.manage" => HasAnyRole("GA", "CLINIC"),
            "expenses.manage" => HasAnyRole("HR", "ACCOUNTING"),
            "reports.manage" => HasAnyRole("HRD"),
            _ => false
        };
    }

    private async Task RestoreDefaultAdministratorAsync()
    {
        var administrator = await _database.GetDefaultAdministratorAsync()
                            ?? throw new InvalidOperationException(
                                "The required mock administrator account is missing from the seed data.");

        await SetCurrentUserAsync(administrator, rememberMe: true);
    }

    private async Task SetCurrentUserAsync(MockCredentialRecord credential, bool rememberMe)
    {
        _currentUser = new MockSignedInUser(
            TravellerId: credential.TravellerId,
            EmployeeNo: credential.EmployeeNo,
            Email: credential.Email,
            FullName: credential.FullName,
            Nickname: credential.Nickname,
            RoleCodes: credential.RoleCodes,
            RoleNames: credential.RoleNames,
            RememberMe: rememberMe,
            SignedInAt: DateTimeOffset.Now);

        var serialized = JsonSerializer.Serialize(_currentUser);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SessionStorageKey, serialized);
        Changed?.Invoke();
    }

    private bool HasAnyRole(params string[] roleCodes)
    {
        if (_currentUser is null)
        {
            return false;
        }

        return roleCodes.Any(required =>
            _currentUser.RoleCodes.Contains(required, StringComparer.OrdinalIgnoreCase));
    }
}
