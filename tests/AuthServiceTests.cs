using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using RecettesIndex.Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace RecettesIndex.Tests
{
    public class AuthServiceTests
    {
        private readonly Supabase.Client _mockSupabaseClient;
        private readonly IGotrueClient<User, Session> _mockAuth;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockSupabaseClient = Substitute.For<Supabase.Client>();
            _mockAuth = Substitute.For<IGotrueClient<User, Session>>();
            _mockSupabaseClient.Auth.Returns(_mockAuth);
            _authService = new AuthService(_mockSupabaseClient);
        }

        [Fact]
        public async Task SignInAsync_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var mockSession = Substitute.For<Session>();
            var mockUser = Substitute.For<User>();
            mockSession.User.Returns(mockUser);
            _mockAuth.SignIn(email, password).Returns(mockSession);

            // Act
            var result = await _authService.SignInAsync(email, password);

            // Assert
            Assert.True(result);
            await _mockAuth.Received(1).SignIn(email, password);
        }

        [Fact]
        public async Task SignInAsync_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            _mockAuth.SignIn(email, password).Returns((Session?)null);

            // Act
            var result = await _authService.SignInAsync(email, password);

            // Assert
            Assert.False(result);
            await _mockAuth.Received(1).SignIn(email, password);
        }

        [Fact]
        public async Task SignInAsync_WithNullSession_ReturnsFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            _mockAuth.SignIn(email, password).Returns((Session?)null);

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
            var mockSession = Substitute.For<Session>();
            mockSession.User.Returns((User?)null);
            _mockAuth.SignIn(email, password).Returns(mockSession);

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
            _mockAuth.SignIn(email, password).Throws(new Exception("Network error"));

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
            await _mockAuth.Received(1).SignOut();
        }

        [Fact]
        public void IsAuthenticated_WithCurrentUser_ReturnsTrue()
        {
            // Arrange
            var mockUser = Substitute.For<User>();
            _mockAuth.CurrentUser.Returns(mockUser);

            // Act
            var result = _authService.IsAuthenticated;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAuthenticated_WithoutCurrentUser_ReturnsFalse()
        {
            // Arrange
            _mockAuth.CurrentUser.Returns((User?)null);

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
            var mockUser = Substitute.For<User>();
            mockUser.Email.Returns(expectedEmail);
            _mockAuth.CurrentUser.Returns(mockUser);

            // Act
            var result = _authService.UserEmail;

            // Assert
            Assert.Equal(expectedEmail, result);
        }

        [Fact]
        public void UserEmail_WithoutCurrentUser_ReturnsNull()
        {
            // Arrange
            _mockAuth.CurrentUser.Returns((User?)null);

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
            _mockAuth.SignIn(email, password).Returns((Session?)null);

            // Act
            var result = await _authService.SignInAsync(email, password);

            // Assert
            Assert.False(result);
            await _mockAuth.Received(1).SignIn(email, password);
        }

        [Fact]
        public async Task SignOutAsync_WhenExceptionThrown_ShouldPropagate()
        {
            // Arrange
            var expectedException = new Exception("Network error");
            _mockAuth.SignOut().Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.SignOutAsync());
            Assert.Equal(expectedException.Message, exception.Message);
        }
    }
}
