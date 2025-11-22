using MudBlazor;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for RatingColorService
/// </summary>
public class RatingColorServiceTests
{
    #region GetRatingColorHex Tests

    [Fact]
    public void GetRatingColorHex_FiveStars_ReturnsGreen()
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(5);

        // Assert
        Assert.Equal("#4CAF50", color);
    }

    [Fact]
    public void GetRatingColorHex_FourStars_ReturnsBlue()
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(4);

        // Assert
        Assert.Equal("#2196F3", color);
    }

    [Fact]
    public void GetRatingColorHex_ThreeStars_ReturnsOrange()
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(3);

        // Assert
        Assert.Equal("#FF9800", color);
    }

    [Fact]
    public void GetRatingColorHex_TwoStars_ReturnsGrey()
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(2);

        // Assert
        Assert.Equal("#9E9E9E", color);
    }

    [Fact]
    public void GetRatingColorHex_OneStar_ReturnsRed()
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(1);

        // Assert
        Assert.Equal("#F44336", color);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(999)]
    public void GetRatingColorHex_InvalidRatings_ReturnsDarkGrey(int rating)
    {
        // Act
        var color = RatingColorService.GetRatingColorHex(rating);

        // Assert
        Assert.Equal("#757575", color);
    }

    #endregion

    #region GetRatingMudColor Tests

    [Fact]
    public void GetRatingMudColor_FiveStars_ReturnsSuccess()
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(5);

        // Assert
        Assert.Equal(Color.Success, color);
    }

    [Fact]
    public void GetRatingMudColor_FourStars_ReturnsInfo()
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(4);

        // Assert
        Assert.Equal(Color.Info, color);
    }

    [Fact]
    public void GetRatingMudColor_ThreeStars_ReturnsWarning()
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(3);

        // Assert
        Assert.Equal(Color.Warning, color);
    }

    [Fact]
    public void GetRatingMudColor_TwoStars_ReturnsDefault()
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(2);

        // Assert
        Assert.Equal(Color.Default, color);
    }

    [Fact]
    public void GetRatingMudColor_OneStar_ReturnsError()
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(1);

        // Assert
        Assert.Equal(Color.Error, color);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void GetRatingMudColor_InvalidRatings_ReturnsDark(int rating)
    {
        // Act
        var color = RatingColorService.GetRatingMudColor(rating);

        // Assert
        Assert.Equal(Color.Dark, color);
    }

    #endregion

    #region GetRowBackgroundStyle Tests

    [Fact]
    public void GetRowBackgroundStyle_FiveStars_ReturnsGreenWithOpacity()
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(5);

        // Assert
        Assert.Equal("background-color: #4CAF500D;", style);
    }

    [Fact]
    public void GetRowBackgroundStyle_FourStars_ReturnsBlueWithOpacity()
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(4);

        // Assert
        Assert.Equal("background-color: #2196F30D;", style);
    }

    [Fact]
    public void GetRowBackgroundStyle_ThreeStars_ReturnsOrangeWithOpacity()
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(3);

        // Assert
        Assert.Equal("background-color: #FF98000D;", style);
    }

    [Fact]
    public void GetRowBackgroundStyle_TwoStars_ReturnsGreyWithOpacity()
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(2);

        // Assert
        Assert.Equal("background-color: #9E9E9E0D;", style);
    }

    [Fact]
    public void GetRowBackgroundStyle_OneStar_ReturnsRedWithOpacity()
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(1);

        // Assert
        Assert.Equal("background-color: #F443360D;", style);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void GetRowBackgroundStyle_InvalidRatings_ReturnsDarkGreyWithOpacity(int rating)
    {
        // Act
        var style = RatingColorService.GetRowBackgroundStyle(rating);

        // Assert
        Assert.StartsWith("background-color: #757575", style);
        Assert.EndsWith("0D;", style);
    }

    [Fact]
    public void GetRowBackgroundStyle_AlwaysEndsWithSemicolon()
    {
        // Arrange
        var ratings = new[] { 1, 2, 3, 4, 5, 0, -1, 6 };

        // Act & Assert
        foreach (var rating in ratings)
        {
            var style = RatingColorService.GetRowBackgroundStyle(rating);
            Assert.EndsWith(";", style);
        }
    }

    #endregion

    #region GetRatingColorBarStyle Tests

    [Fact]
    public void GetRatingColorBarStyle_FiveStars_ReturnsGreen()
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(5);

        // Assert
        Assert.Equal("background-color: #4CAF50;", style);
    }

    [Fact]
    public void GetRatingColorBarStyle_FourStars_ReturnsBlue()
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(4);

        // Assert
        Assert.Equal("background-color: #2196F3;", style);
    }

    [Fact]
    public void GetRatingColorBarStyle_ThreeStars_ReturnsOrange()
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(3);

        // Assert
        Assert.Equal("background-color: #FF9800;", style);
    }

    [Fact]
    public void GetRatingColorBarStyle_TwoStars_ReturnsGrey()
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(2);

        // Assert
        Assert.Equal("background-color: #9E9E9E;", style);
    }

    [Fact]
    public void GetRatingColorBarStyle_OneStar_ReturnsRed()
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(1);

        // Assert
        Assert.Equal("background-color: #F44336;", style);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void GetRatingColorBarStyle_InvalidRatings_ReturnsDarkGrey(int rating)
    {
        // Act
        var style = RatingColorService.GetRatingColorBarStyle(rating);

        // Assert
        Assert.Equal("background-color: #757575;", style);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AllMethods_ConsistentColorMapping_ForSameRating()
    {
        // Arrange
        var rating = 5;

        // Act
        var hexColor = RatingColorService.GetRatingColorHex(rating);
        var mudColor = RatingColorService.GetRatingMudColor(rating);
        var rowStyle = RatingColorService.GetRowBackgroundStyle(rating);
        var barStyle = RatingColorService.GetRatingColorBarStyle(rating);

        // Assert
        Assert.Equal("#4CAF50", hexColor);
        Assert.Equal(Color.Success, mudColor);
        Assert.Contains("#4CAF50", rowStyle);
        Assert.Contains("#4CAF50", barStyle);
    }

    [Theory]
    [InlineData(1, "#F44336", Color.Error)]
    [InlineData(2, "#9E9E9E", Color.Default)]
    [InlineData(3, "#FF9800", Color.Warning)]
    [InlineData(4, "#2196F3", Color.Info)]
    [InlineData(5, "#4CAF50", Color.Success)]
    public void ColorMapping_IsConsistent_AcrossAllMethods(int rating, string expectedHex, Color expectedMudColor)
    {
        // Act
        var hexColor = RatingColorService.GetRatingColorHex(rating);
        var mudColor = RatingColorService.GetRatingMudColor(rating);

        // Assert
        Assert.Equal(expectedHex, hexColor);
        Assert.Equal(expectedMudColor, mudColor);
    }

    #endregion
}
