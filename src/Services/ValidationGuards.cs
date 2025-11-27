namespace RecettesIndex.Services;

public static class ValidationGuards
{
    public static string? RequireNotNull<T>(T value, string name)
        => value is null ? $"{name} cannot be null" : null;

    public static string? RequireNonEmpty(string? value, string name)
        => string.IsNullOrWhiteSpace(value) ? $"{name} is required" : null;

    public static string? RequirePositive(int value, string name)
        => value <= 0 ? $"Invalid {name}" : null;

    public static string? RequireInRange(int value, int min, int max, string name)
        => (value < min || value > max) ? $"{name} must be between {min} and {max}" : null;
}
