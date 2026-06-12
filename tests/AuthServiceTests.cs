using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RecettesIndex.Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Xunit;

namespace RecettesIndex.Tests;

public class AuthServiceTests
{
    private readonly ISupabaseAuthWrapper _mockAuthWrapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockAuthWrapper = Substitute.For<ISupabaseAuthWrapper>();
        _authService = new AuthService(_mockAuthWrapper);
    }

    [Fact]
    public async Task SignInAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var mockSession = new Session();
        var mockUser = new User { Email = email };
        mockSession.User = mockUser;
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(mockSession));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.Success, result);
        await _mockAuthWrapper.Received(1).SignIn(email, password);
    }

    [Fact]
    public async Task SignInAsync_WithInvalidCredentials_ReturnsInvalidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(null));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.InvalidCredentials, result);
        await _mockAuthWrapper.Received(1).SignIn(email, password);
    }

    [Fact]
    public async Task SignInAsync_WithNullSession_ReturnsInvalidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(null));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.InvalidCredentials, result);
    }

    [Fact]
    public async Task SignInAsync_WithSessionButNullUser_ReturnsInvalidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var mockSession = new Session();  // Session with null User
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(mockSession));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.InvalidCredentials, result);
    }

    [Fact]
    public async Task SignInAsync_ThrowsUnexpectedException_ReturnsUnknownError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        _mockAuthWrapper.SignIn(email, password).Throws(new Exception("Boom"));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.UnknownError, result);
    }

    [Fact]
    public async Task SignInAsync_ThrowsHttpRequestException_ReturnsNetworkError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        _mockAuthWrapper.SignIn(email, password).Throws(new HttpRequestException("offline"));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.NetworkError, result);
    }

    [Fact]
    public async Task SendPasswordResetAsync_Success_ReturnsTrue()
    {
        // Arrange
        _mockAuthWrapper.SendResetPasswordEmail("test@example.com").Returns(Task.CompletedTask);

        // Act
        var result = await _authService.SendPasswordResetAsync("test@example.com");

        // Assert
        Assert.True(result);
        await _mockAuthWrapper.Received(1).SendResetPasswordEmail("test@example.com");
    }

    [Fact]
    public async Task SendPasswordResetAsync_Failure_ReturnsFalse()
    {
        // Arrange
        _mockAuthWrapper.SendResetPasswordEmail("test@example.com").Throws(new Exception("boom"));

        // Act
        var result = await _authService.SendPasswordResetAsync("test@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WrapperSignedOut_AfterAuthentication_RaisesSessionExpired()
    {
        // Arrange - simulate a signed-in session, then a sign-out NOT requested by the user
        var email = "test@example.com";
        var mockSession = new Session { User = new User { Email = email } };
        _mockAuthWrapper.SignIn(email, "pw").Returns(Task.FromResult<Session?>(mockSession));
        await _authService.SignInAsync(email, "pw");

        var expired = false;
        _authService.SessionExpired += () => expired = true;

        // Act
        _mockAuthWrapper.AuthStateChanged += Raise.Event<Action<Supabase.Gotrue.Constants.AuthState>>(
            Supabase.Gotrue.Constants.AuthState.SignedOut);

        // Assert
        Assert.True(expired);
    }

    [Fact]
    public async Task WrapperSignedOut_DuringUserSignOut_DoesNotRaiseSessionExpired()
    {
        // Arrange
        var email = "test@example.com";
        var mockSession = new Session { User = new User { Email = email } };
        _mockAuthWrapper.SignIn(email, "pw").Returns(Task.FromResult<Session?>(mockSession));
        await _authService.SignInAsync(email, "pw");

        var expired = false;
        _authService.SessionExpired += () => expired = true;
        _mockAuthWrapper.When(w => w.SignOut()).Do(_ =>
            _mockAuthWrapper.AuthStateChanged += Raise.Event<Action<Supabase.Gotrue.Constants.AuthState>>(
                Supabase.Gotrue.Constants.AuthState.SignedOut));

        // Act
        await _authService.SignOutAsync();

        // Assert
        Assert.False(expired);
    }

    [Fact]
    public async Task SignOutAsync_CallsSupabaseSignOut()
    {
        // Act
        await _authService.SignOutAsync();

        // Assert
        await _mockAuthWrapper.Received(1).SignOut();
    }

    [Fact]
    public async Task SignInAsync_WithValidCredentials_RaisesAuthStateChanged()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var mockSession = new Session { User = new User { Email = email } };
        var raised = false;
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(mockSession));
        _authService.AuthStateChanged += () => raised = true;

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.Success, result);
        Assert.True(raised);
    }

    [Fact]
    public async Task SignOutAsync_RaisesAuthStateChanged()
    {
        // Arrange
        var raised = false;
        _authService.AuthStateChanged += () => raised = true;

        // Act
        await _authService.SignOutAsync();

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void RefreshAuthState_RaisesAuthStateChanged()
    {
        // Arrange
        var raised = false;
        _authService.AuthStateChanged += () => raised = true;

        // Act
        _authService.RefreshAuthState();

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public async Task InitializeAsync_CallsWrapperInitializeAndRaisesAuthStateChanged()
    {
        // Arrange
        var raised = false;
        _authService.AuthStateChanged += () => raised = true;

        // Act
        await _authService.InitializeAsync();

        // Assert
        await _mockAuthWrapper.Received(1).InitializeAsync();
        Assert.True(raised);
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_InitializesOnlyOnce()
    {
        // Act
        await _authService.InitializeAsync();
        await _authService.InitializeAsync();

        // Assert
        await _mockAuthWrapper.Received(1).InitializeAsync();
    }

    [Fact]
    public void IsAuthenticated_WithCurrentUser_ReturnsTrue()
    {
        // Arrange
        var mockUser = new User { Email = "test@example.com" };
        _mockAuthWrapper.CurrentUser.Returns(mockUser);

        // Act
        var result = _authService.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WithoutCurrentUser_ReturnsFalse()
    {
        // Arrange
        _mockAuthWrapper.CurrentUser.Returns((User?)null);

        // Act
        var result = _authService.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UserEmail_WithCurrentUser_ReturnsEmail()
    {
        // Arrange
        var expectedEmail = "test@example.com";
        var mockUser = new User { Email = expectedEmail };
        _mockAuthWrapper.CurrentUser.Returns(mockUser);

        // Act
        var result = _authService.UserEmail;

        // Assert
        Assert.Equal(expectedEmail, result);
    }

    [Fact]
    public void UserEmail_WithoutCurrentUser_ReturnsNull()
    {
        // Arrange
        _mockAuthWrapper.CurrentUser.Returns((User?)null);

        // Act
        var result = _authService.UserEmail;

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("test@example.com", "")]
    [InlineData("", "")]
    public async Task SignInAsync_WithEmptyCredentials_StillCallsSupabase(string email, string password)
    {
        // Arrange
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(null));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.Equal(SignInOutcome.InvalidCredentials, result);
        await _mockAuthWrapper.Received(1).SignIn(email, password);
    }

    [Fact]
    public async Task SignOutAsync_WhenExceptionThrown_ShouldPropagate()
    {
        // Arrange
        var expectedException = new Exception("Network error");
        _mockAuthWrapper.SignOut().Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _authService.SignOutAsync());
        Assert.Equal(expectedException.Message, exception.Message);
    }
}
