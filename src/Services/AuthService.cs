namespace RecettesIndex.Services;

/// <summary>
/// Service for managing user authentication operations.
/// </summary>
public class AuthService(ISupabaseAuthWrapper authWrapper)
{
    private readonly ISupabaseAuthWrapper _authWrapper = authWrapper ?? throw new ArgumentNullException(nameof(authWrapper));
    private bool _isInitialized;

    /// <summary>
    /// Event raised when authentication state may have changed.
    /// </summary>
    public event Action? AuthStateChanged;

    /// <summary>
    /// Initializes the auth client and attempts to restore any persisted session.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _authWrapper.InitializeAsync();
        _isInitialized = true;
        NotifyAuthStateChanged();
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
        var isAuthenticated = session != null && session.User != null;
        if (isAuthenticated)
        {
            NotifyAuthStateChanged();
        }

        return isAuthenticated;
    }

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SignOutAsync()
    {
        await _authWrapper.SignOut();
        NotifyAuthStateChanged();
    }

    /// <summary>
    /// Triggers a refresh for subscribers based on current auth state.
    /// </summary>
    public void RefreshAuthState()
    {
        NotifyAuthStateChanged();
    }

    /// <summary>
    /// Gets a value indicating whether a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => _authWrapper.CurrentUser != null;

    /// <summary>
    /// Gets the email address of the currently authenticated user, or null if not authenticated.
    /// </summary>
    public string? UserEmail => _authWrapper.CurrentUser?.Email;

    private void NotifyAuthStateChanged()
    {
        AuthStateChanged?.Invoke();
    }
}
