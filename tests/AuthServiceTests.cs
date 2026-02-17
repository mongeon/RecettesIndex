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
    public async Task SignInAsync_WithValidCredentials_ReturnsTrue()
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
        Assert.True(result);
        await _mockAuthWrapper.Received(1).SignIn(email, password);
    }

    [Fact]
    public async Task SignInAsync_WithInvalidCredentials_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(null));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.False(result);
        await _mockAuthWrapper.Received(1).SignIn(email, password);
    }

    [Fact]
    public async Task SignInAsync_WithNullSession_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(null));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SignInAsync_WithSessionButNullUser_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var mockSession = new Session();  // Session with null User
        _mockAuthWrapper.SignIn(email, password).Returns(Task.FromResult<Session?>(mockSession));

        // Act
        var result = await _authService.SignInAsync(email, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SignInAsync_ThrowsException_ShouldPropagate()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        _mockAuthWrapper.SignIn(email, password).Throws(new Exception("Network error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            async () => await _authService.SignInAsync(email, password));

        Assert.Equal("Network error", ex.Message);
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
        Assert.True(result);
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
        Assert.False(result);
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
