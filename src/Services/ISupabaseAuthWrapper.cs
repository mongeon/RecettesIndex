using Supabase.Gotrue;

namespace RecettesIndex.Services
{
    public interface ISupabaseAuthWrapper
    {
        Task<Session?> SignIn(string email, string password);
        Task SignOut();
        User? CurrentUser { get; }
    }
}
