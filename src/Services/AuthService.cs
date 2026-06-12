using Microsoft.Extensions.Logging;
using Supabase.Gotrue.Exceptions;

namespace RecettesIndex.Services;

/// <summary>
/// Outcome of a sign-in attempt, allowing the UI to show a targeted message.
/// </summary>
public enum SignInOutcome
{
    Success,
    InvalidCredentials,
    EmailNotConfirmed,
    NetworkError,
    TooManyRequests,
    UnknownError
}

/// <summary>
/// Service for managing user authentication operations.
/// </summary>
public class AuthService : IDisposable
{
    private readonly ISupabaseAuthWrapper _authWrapper;
    private readonly ILogger<AuthService>? _logger;
    private bool _isInitialized;
    private bool _wasAuthenticated;
    private bool _signOutRequested;

    public AuthService(ISupabaseAuthWrapper authWrapper, ILogger<AuthService>? logger = null)
    {
        _authWrapper = authWrapper ?? throw new ArgumentNullException(nameof(authWrapper));
        _logger = logger;
        _authWrapper.AuthStateChanged += OnWrapperAuthStateChanged;
    }

    /// <summary>
    /// Event raised when authentication state may have changed.
    /// </summary>
    public event Action? AuthStateChanged;

    /// <summary>
    /// Event raised when the session ends without the user asking for it
    /// (e.g. the refresh token expired or was rejected).
    /// </summary>
    public event Action? SessionExpired;

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
        _wasAuthenticated = IsAuthenticated;
        NotifyAuthStateChanged();
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The outcome of the attempt, so the UI can show a targeted message.</returns>
    public async Task<SignInOutcome> SignInAsync(string email, string password)
    {
        try
        {
            var session = await _authWrapper.SignIn(email, password);
            if (session?.User == null)
            {
                return SignInOutcome.InvalidCredentials;
            }

            _wasAuthenticated = true;
            NotifyAuthStateChanged();
            return SignInOutcome.Success;
        }
        catch (GotrueException ex)
        {
            _logger?.LogWarning(ex, "Sign-in failed: {Reason}", ex.Reason);
            return ex.Reason switch
            {
                FailureHint.Reason.UserBadLogin or
                FailureHint.Reason.UserBadPassword or
                FailureHint.Reason.UserBadEmailAddress or
                FailureHint.Reason.UserBadMultiple => SignInOutcome.InvalidCredentials,
                FailureHint.Reason.UserEmailNotConfirmed => SignInOutcome.EmailNotConfirmed,
                FailureHint.Reason.UserTooManyRequests => SignInOutcome.TooManyRequests,
                FailureHint.Reason.Offline => SignInOutcome.NetworkError,
                _ => SignInOutcome.UnknownError
            };
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogWarning(ex, "Network error during sign-in");
            return SignInOutcome.NetworkError;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during sign-in");
            return SignInOutcome.UnknownError;
        }
    }

    /// <summary>
    /// Sends a password reset email via Supabase.
    /// </summary>
    /// <param name="email">The email address of the account to reset.</param>
    /// <returns>True if the request was accepted; false on error.</returns>
    public async Task<bool> SendPasswordResetAsync(string email)
    {
        try
        {
            await _authWrapper.SendResetPasswordEmail(email);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to send password reset email");
            return false;
        }
    }

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SignOutAsync()
    {
        _signOutRequested = true;
        try
        {
            await _authWrapper.SignOut();
        }
        finally
        {
            _signOutRequested = false;
            _wasAuthenticated = false;
        }
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

    private void OnWrapperAuthStateChanged(Supabase.Gotrue.Constants.AuthState state)
    {
        switch (state)
        {
            case Supabase.Gotrue.Constants.AuthState.SignedIn:
                _wasAuthenticated = true;
                NotifyAuthStateChanged();
                break;

            case Supabase.Gotrue.Constants.AuthState.SignedOut:
                var sessionLost = _wasAuthenticated && !_signOutRequested;
                _wasAuthenticated = false;
                NotifyAuthStateChanged();
                if (sessionLost)
                {
                    _logger?.LogInformation("Session ended without user sign-out (token expired or rejected)");
                    SessionExpired?.Invoke();
                }
                break;
        }
    }

    private void NotifyAuthStateChanged()
    {
        AuthStateChanged?.Invoke();
    }

    public void Dispose()
    {
        _authWrapper.AuthStateChanged -= OnWrapperAuthStateChanged;
    }
}
