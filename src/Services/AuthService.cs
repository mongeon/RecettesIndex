namespace RecettesIndex.Services
{
    public class AuthService
    {
        private readonly ISupabaseAuthWrapper _authWrapper;
        
        public AuthService(ISupabaseAuthWrapper authWrapper)
        {
            _authWrapper = authWrapper;
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            var session = await _authWrapper.SignIn(email, password);
            return session != null && session.User != null;
        }

        public async Task SignOutAsync()
        {
            await _authWrapper.SignOut();
        }

        public bool IsAuthenticated => _authWrapper.CurrentUser != null;
        public string? UserEmail => _authWrapper.CurrentUser?.Email;
    }
}
