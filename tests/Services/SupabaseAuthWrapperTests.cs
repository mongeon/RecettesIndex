using NSubstitute;
using RecettesIndex.Services;
using Supabase.Gotrue;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Tests for SupabaseAuthWrapper to ensure proper authentication wrapper functionality.
/// </summary>
public class SupabaseAuthWrapperTests
{
    [Fact]
    public void Constructor_WithValidClient_DoesNotThrow()
    {
        // Arrange
        var client = new Supabase.Client("https://test.supabase.co", "test-key");

        // Act
        var wrapper = new SupabaseAuthWrapper(client);

        // Assert
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void CurrentUser_WhenNoUserSignedIn_ReturnsNull()
    {
        // Arrange
        var client = new Supabase.Client("https://test.supabase.co", "test-key");
        var wrapper = new SupabaseAuthWrapper(client);

        // Act
        var currentUser = wrapper.CurrentUser;

        // Assert
        Assert.Null(currentUser);
    }
}
