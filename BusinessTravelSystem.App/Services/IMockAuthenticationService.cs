using BusinessTravelSystem.App.Models;

namespace BusinessTravelSystem.App.Services;

public interface IMockAuthenticationService
{
    event Action? Changed;

    bool IsAuthenticated { get; }
    MockSignedInUser? CurrentUser { get; }

    Task InitializeAsync();
    Task<MockLoginResult> SignInAsync(string employeeNoOrEmail, string password, bool rememberMe);
    Task SignOutAsync();
    Task ResetMockDatabaseAsync();
    bool CanAccessModule(string moduleKey);
    bool Can(string permission);
}
