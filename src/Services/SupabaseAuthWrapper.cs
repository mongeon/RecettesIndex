using Supabase.Gotrue;

namespace RecettesIndex.Services;

public class SupabaseAuthWrapper : ISupabaseAuthWrapper
{
    private readonly Supabase.Client _client;

    public event Action<Supabase.Gotrue.Constants.AuthState>? AuthStateChanged;

    public SupabaseAuthWrapper(Supabase.Client client)
    {
        _client = client;
        _client.Auth.AddStateChangedListener((_, state) => AuthStateChanged?.Invoke(state));
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    public async Task<Session?> SignIn(string email, string password)
    {
        return await _client.Auth.SignIn(email, password);
    }

    public async Task SignOut()
    {
        await _client.Auth.SignOut();
    }

    public async Task SendResetPasswordEmail(string email)
    {
        await _client.Auth.ResetPasswordForEmail(email);
    }

    public User? CurrentUser => _client.Auth.CurrentUser;
}
