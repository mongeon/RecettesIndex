using Supabase.Gotrue;

namespace RecettesIndex.Services;

public interface ISupabaseAuthWrapper
{
    public Task InitializeAsync();
    public Task<Session?> SignIn(string email, string password);
    public Task SignOut();
    public User? CurrentUser { get; }
}
