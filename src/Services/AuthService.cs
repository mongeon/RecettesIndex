namespace RecettesIndex.Services
{
    /// <summary>
    /// Service for managing user authentication operations.
    /// </summary>
    public class AuthService
    {
        private readonly ISupabaseAuthWrapper _authWrapper;
        
        public AuthService(ISupabaseAuthWrapper authWrapper)
        {
            _authWrapper = authWrapper;
        }

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>True if authentication was successful; otherwise, false.</returns>
        public async Task<bool> SignInAsync(string email, string password)
        {
            var session = await _authWrapper.SignIn(email, password);
            return session != null && session.User != null;
        }

        /// <summary>
        /// Signs out the currently authenticated user.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SignOutAsync()
        {
            await _authWrapper.SignOut();
        }

        /// <summary>
        /// Gets a value indicating whether a user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated => _authWrapper.CurrentUser != null;
        
        /// <summary>
        /// Gets the email address of the currently authenticated user, or null if not authenticated.
        /// </summary>
        public string? UserEmail => _authWrapper.CurrentUser?.Email;
    }
}
