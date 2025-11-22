using MudBlazor;

namespace RecettesIndex.Services;

/// <summary>
/// Provides consistent color coding for recipe ratings throughout the application.
/// </summary>
/// <remarks>
/// This service centralizes all rating-to-color mapping logic to ensure consistency
/// across all UI components (tables, cards, prints, charts, etc.).
/// </remarks>
public static class RatingColorService
{
    /// <summary>
    /// Gets the hex color code for a given recipe rating.
    /// </summary>
    /// <param name="rating">The rating value (1-5 stars, or 0 for unrated).</param>
    /// <returns>A hex color code string (e.g., "#4CAF50").</returns>
    /// <remarks>
    /// Color mapping:
    /// - 5 stars: Green (#4CAF50) - Excellent
    /// - 4 stars: Blue (#2196F3) - Very Good
    /// - 3 stars: Orange (#FF9800) - Good
    /// - 2 stars: Grey (#9E9E9E) - Fair
    /// - 1 star: Red (#F44336) - Poor
    /// - Unrated: Dark Grey (#757575)
    /// </remarks>
    public static string GetRatingColorHex(int rating) => rating switch
    {
        5 => "#4CAF50", // Green - Success
        4 => "#2196F3", // Blue - Info
        3 => "#FF9800", // Orange - Warning
        2 => "#9E9E9E", // Grey - Default
        1 => "#F44336", // Red - Error
        _ => "#757575"  // Dark Grey - Unrated
    };
    
    /// <summary>
    /// Gets the MudBlazor Color enum value for a given recipe rating.
    /// </summary>
    /// <param name="rating">The rating value (1-5 stars, or 0 for unrated).</param>
    /// <returns>A MudBlazor Color enum value.</returns>
    /// <remarks>
    /// Used for MudBlazor components like MudChip, MudProgressLinear, etc.
    /// </remarks>
    public static Color GetRatingMudColor(int rating) => rating switch
    {
        5 => Color.Success,
        4 => Color.Info,
        3 => Color.Warning,
        2 => Color.Default,
        1 => Color.Error,
        _ => Color.Dark
    };
    
    /// <summary>
    /// Gets a CSS background color style with 5% opacity for table rows.
    /// </summary>
    /// <param name="rating">The rating value (1-5 stars, or 0 for unrated).</param>
    /// <returns>A CSS style string for background-color with opacity (e.g., "background-color: #4CAF500D;").</returns>
    /// <remarks>
    /// The "0D" suffix represents 5% opacity in hex (0.05 * 255 = 13 = 0x0D).
    /// This provides subtle color coding without overwhelming the UI.
    /// </remarks>
    public static string GetRowBackgroundStyle(int rating) => 
        $"background-color: {GetRatingColorHex(rating)}0D;";
    
    /// <summary>
    /// Gets a CSS background-color property value for solid color usage.
    /// </summary>
    /// <param name="rating">The rating value (1-5 stars, or 0 for unrated).</param>
    /// <returns>A CSS background-color value (e.g., "background-color: #4CAF50;").</returns>
    /// <remarks>
    /// Used for elements that need full color intensity (print views, color bars, etc.).
    /// </remarks>
    public static string GetRatingColorBarStyle(int rating) => 
        $"background-color: {GetRatingColorHex(rating)};";
}
