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

public interface IHasKey<T> where T : struct, Enum
{
  T Key { get; init; }
  string Name { get; init; }
  IHasKey<T> WithKeyAndName(T key, string name);
}
public record Domain01 : IHasKey<Direction>
{
  public Direction Key { get; init; }
  public string Name { get; init; } = string.Empty;
  public required string SystemDef { get; init; }
  public required string Icon { get; init; }
  public string? UserDef { get; set; } = null;
  public string CurrentDef => string.IsNullOrEmpty(UserDef) ? SystemDef : UserDef;
  public IHasKey<Direction> WithKeyAndName(Direction key, string name) => this with { Key = key, Name = name };
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
    /// <summary>
    /// Creates a dictionary mapping enum values to domain objects.
    /// Merges partial domain objects with enum metadata (Key and Name).
    /// </summary>
    /// <typeparam name="TEnum">The enum type to use as dictionary keys</typeparam>
    /// <typeparam name="UDomain">The domain type to use as dictionary values (must be Domain01)</typeparam>
    /// <param name="partialObjects">Partial domain objects with SystemDef, Icon, etc.</param>
    /// <returns>Dictionary mapping enum values to fully initialized domain objects</returns>
    public static Dictionary<TEnum, UDomain> Www<TEnum, UDomain>(IEnumerable<UDomain> partialObjects)
        where TEnum : struct, Enum
        where UDomain : IHasKey<TEnum>
    {
      var enumValues = GetValues<TEnum>();
      return enumValues.Zip(partialObjects, (enumValue, partial) =>
      {
          // Use 'with' expression to create a new record instance with merged properties
          return (UDomain)partial.WithKeyAndName(enumValue, enumValue.ToString());
      }).ToDictionary(q => q.Key, q => q);
    }
}
