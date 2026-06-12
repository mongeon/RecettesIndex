using Supabase.Gotrue;

namespace RecettesIndex.Services;

public interface ISupabaseAuthWrapper
{
    public Task InitializeAsync();
    public Task<Session?> SignIn(string email, string password);
    public Task SignOut();
    public Task SendResetPasswordEmail(string email);
    public User? CurrentUser { get; }

    /// <summary>
    /// Raised when the underlying Supabase auth client changes state
    /// (sign-in, sign-out — including sign-out caused by a failed token refresh).
    /// </summary>
    public event Action<Supabase.Gotrue.Constants.AuthState>? AuthStateChanged;
}
