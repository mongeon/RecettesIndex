namespace RecettesIndex.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _client;
        public AuthService(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            var session = await _client.Auth.SignIn(email, password);
            return session != null && session.User != null;
        }

        public async Task SignOutAsync()
        {
            await _client.Auth.SignOut();
        }

        public bool IsAuthenticated => _client.Auth.CurrentUser != null;
        public string? UserEmail => _client.Auth.CurrentUser?.Email;
    }
}
