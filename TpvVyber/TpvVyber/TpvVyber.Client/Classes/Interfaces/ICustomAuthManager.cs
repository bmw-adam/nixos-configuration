namespace TpvVyber.Client.Classes.Interfaces;

public interface ICustomAuthManager
{
    Task<string?> GetAccessTokenAsync();
    Task SignInAsync();
    Task SignOutAsync();
    bool IsAuthenticated { get; }
}
