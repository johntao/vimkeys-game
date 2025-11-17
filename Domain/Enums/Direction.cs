namespace p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Enum representing the four possible movement directions
/// Corresponds to VIM keys: k=Up, j=Down, h=Left, l=Right
/// </summary>
public enum Direction
{
    None,
    Left,
    Down,
    Up,
    Right
}

public class Domain01
{
  public required Direction Key { get; init; }
  public required string SystemDef { get; init; }
  public required string Name { get; init; }
  public required string Icon { get; init; }
  public string? UserDef { get; set; } = null;
  public string CurrentDef => string.IsNullOrEmpty(UserDef) ? SystemDef : UserDef;
}

internal static class EnumHelper
{
    /// <summary>
    /// Returns a HashSet of all values for the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="includeNone">Whether to include the zero value (None). Defaults to false.</param>
    /// <returns>HashSet of enum values</returns>
    private static IEnumerable<T> GetValues<T>(bool includeNone = false) where T : struct, Enum
    {
        return Enum.GetValues<T>().Skip(includeNone ? 0 : 1);
    }

    /// <summary>
    /// Returns a HashSet of all string names for the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="includeNone">Whether to include the zero value name (None). Defaults to false.</param>
    /// <returns>HashSet of enum string literals</returns>
    public static HashSet<string> GetNames<T>(bool includeNone = false) where T : struct, Enum
    {
        return Enum.GetNames<T>()
            .Skip(includeNone ? 0 : 1)
            .ToHashSet();
    }
    public static Dictionary<TEnum, UDomain> Www<TEnum, UDomain>(IEnumerable<UDomain> arr)
    {
      return GetValues<TEnum>().Zip(arr, (key, patch) =>
      {
          var newEntry = new UDomain(patch)
          {
            Key = key,
            Name = key.ToString()
          };
          return newEntry;
      }).ToDictionary(q => q.Key, q => q);
    }
}
