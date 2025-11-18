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
public enum AllConfig
{
    None,
    Left,
    Down,
    Up,
    Right,
    LeftX,
    DownX,
    UpX,
    RightX,
}

public interface IHasKey<T> where T : struct, Enum
{
  T Key { get; init; }
  string Name { get; init; }
  IHasKey<T> WithKeyAndName(T key, string name);
}
public abstract record KeyConfig : IHasKey<AllConfig>
{
  public AllConfig Key { get; init; }
  public string Name { get; init; } = string.Empty;
  public required string SystemDef { get; init; }
  public required string Icon { get; init; }
  public string? UserDef { get; set; } = null;
  public string CurrentDef => string.IsNullOrEmpty(UserDef) ? SystemDef : UserDef;
  public IHasKey<AllConfig> WithKeyAndName(AllConfig key, string name) => this with { Key = key, Name = name };
}

/// <summary>
/// Configuration for basic movement keys (hjkl)
/// Links to a Direction for single-step movement
/// </summary>
public record BasicMovement(int directionIndex) : KeyConfig
{
  public Direction Direction { get; init; } = (Direction)directionIndex;
}

/// <summary>
/// Configuration for multiplier movement keys (e.g., 4x movement)
/// Inherits Direction from BasicMovement and adds a configurable multiplier
/// </summary>
public record BasicXTimes(int directionIndex) : BasicMovement(directionIndex)
{
  /// <summary>
  /// User-defined multiplier value. If null, uses default of 4.
  /// </summary>
  public int? UserDefinedMultiplier { get; set; } = null;

  /// <summary>
  /// Gets the effective multiplier value (user-defined or default 4)
  /// </summary>
  public int CurrentMultiplier => UserDefinedMultiplier ?? 4;
}

/// <summary>
/// Extension methods for enum types
/// </summary>
public static class EnumTypeExtensions
{
    /// <summary>
    /// Initializes a dictionary mapping enum values to domain objects.
    /// This is an idiomatic extension method that allows calling typeof(YourEnum).InitializeDict(...)
    /// </summary>
    /// <typeparam name="TEnum">The enum type to use as dictionary keys</typeparam>
    /// <typeparam name="TDomain">The domain type implementing IHasKey&lt;TEnum&gt;</typeparam>
    /// <param name="enumType">The enum Type (not used, only for extension syntax)</param>
    /// <param name="partialObjects">Partial domain objects with required properties</param>
    /// <returns>Dictionary mapping enum values to fully initialized domain objects</returns>
    public static Dictionary<TEnum, TDomain> InitializeDict<TEnum, TDomain>(this TEnum _, IEnumerable<TDomain> partialObjects)
        where TEnum : struct, Enum
        where TDomain : IHasKey<TEnum>
    {
        return Enum.GetValues<TEnum>().Skip(1).Zip(partialObjects, (enumValue, partial) =>
        {
            return (TDomain)partial.WithKeyAndName(enumValue, enumValue.ToString());
        }).ToDictionary(q => q.Key, q => q);
    }
}
