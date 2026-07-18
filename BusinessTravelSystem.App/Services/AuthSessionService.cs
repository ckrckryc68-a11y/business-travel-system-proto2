namespace BusinessTravelSystem.App.Services;

public sealed class AuthSessionService
{
    public bool IsAuthenticated { get; private set; }
    public string? UserName { get; private set; }
    public bool RememberMe { get; private set; }
    public DateTimeOffset? SignedInAt { get; private set; }

    public event Action? Changed;

    public void SignIn(string userName, bool rememberMe)
    {
        IsAuthenticated = true;
        UserName = userName.Trim();
        RememberMe = rememberMe;
        SignedInAt = DateTimeOffset.Now;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        UserName = null;
        RememberMe = false;
        SignedInAt = null;
        Changed?.Invoke();
    }
}
