namespace FrontendBlazor.Services;

public class UserService
{
    public bool IsAuthenticated { get; private set; } = false;
    public string GivenName { get; private set; } = "Alice";

    public event Action? OnChange;

    public void SignIn()
    {
        IsAuthenticated = true;
        GivenName = "Alice";
        OnChange?.Invoke();
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        GivenName = string.Empty;
        OnChange?.Invoke();
    }
}

