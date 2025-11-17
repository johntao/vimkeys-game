namespace p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Enum representing the four possible movement directions
/// Corresponds to VIM keys: k=Up, j=Down, h=Left, l=Right
/// </summary>
public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right
}

internal static class EnumHelper
{
    /// <summary>
    /// Returns a HashSet of all values for the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="includeNone">Whether to include the zero value (None). Defaults to false.</param>
    /// <returns>HashSet of enum values</returns>
    public static HashSet<T> GetValues<T>(bool includeNone = false) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        if (includeNone)
        {
            return new HashSet<T>(values);
        }

        // Filter out the zero value (typically "None")
        return values.Where(v => Convert.ToInt32(v) != 0).ToHashSet();
    }

    /// <summary>
    /// Returns a HashSet of all string names for the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="includeNone">Whether to include the zero value name (None). Defaults to false.</param>
    /// <returns>HashSet of enum string literals</returns>
    public static HashSet<string> GetNames<T>(bool includeNone = false) where T : struct, Enum
    {
        var names = Enum.GetNames<T>();

        if (includeNone)
        {
            return new HashSet<string>(names);
        }

        // Filter out the name corresponding to zero value
        var values = Enum.GetValues<T>();
        return names
            .Where((name, index) => Convert.ToInt32(values.GetValue(index)!) != 0)
            .ToHashSet();
    }
}
