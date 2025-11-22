using Supabase.Gotrue;

namespace RecettesIndex.Services;

public class SupabaseAuthWrapper(Supabase.Client client) : ISupabaseAuthWrapper
{
    private readonly Supabase.Client _client = client;

    public async Task<Session?> SignIn(string email, string password)
    {
        return await _client.Auth.SignIn(email, password);
    }

    public async Task SignOut()
    {
        await _client.Auth.SignOut();
    }

    public User? CurrentUser => _client.Auth.CurrentUser;
}
