namespace RecettesIndex.Services;

/// <summary>
/// Contains UI-related constants for the application.
/// </summary>
public static class UIConstants
{
    public const int AnimationDelay = 300; // in milliseconds
    /// <summary>
    /// The number of random recipes to select/display.
    /// </summary>
    public const int RandomRecipeCount = 5;

    /// <summary>
    /// Predefined gradient colors for recipe cards.
    /// </summary>
    public static readonly IReadOnlyList<string> GradientColors = new[]
    {
        "linear-gradient(135deg, #667eea 0%, #764ba2 100%)", // Purple
        "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)", // Pink to Red
        "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)", // Blue
        "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)", // Green to Cyan
        "linear-gradient(135deg, #fa709a 0%, #fee140 100%)", // Pink to Yellow
        "linear-gradient(135deg, #30cfd0 0%, #330867 100%)", // Cyan to Purple
        "linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)", // Light Cyan to Pink
        "linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)", // Coral to Light Pink
        "linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)", // Peach
        "linear-gradient(135deg, #ff6e7f 0%, #bfe9ff 100%)"  // Red to Light Blue
    };
}
