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
public record KeyConfig : IHasKey<Direction>
{
  public Direction Key { get; init; }
  public string Name { get; init; } = string.Empty;
  public required string SystemDef { get; init; }
  public required string Icon { get; init; }
  public string? UserDef { get; set; } = null;
  public string CurrentDef => string.IsNullOrEmpty(UserDef) ? SystemDef : UserDef;
  public IHasKey<Direction> WithKeyAndName(Direction key, string name) => this with { Key = key, Name = name };
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
    public static Dictionary<TEnum, TDomain> InitializeDict<TEnum, TDomain>(this Type enumType, IEnumerable<TDomain> partialObjects)
        where TEnum : struct, Enum
        where TDomain : IHasKey<TEnum>
    {
        if (!enumType.IsEnum || enumType != typeof(TEnum))
        {
            throw new ArgumentException($"Type parameter must match the enum type. Expected {typeof(TEnum).Name}, got {enumType.Name}");
        }

        return Enum.GetValues<TEnum>().Skip(1).Zip(partialObjects, (enumValue, partial) =>
        {
            return (TDomain)partial.WithKeyAndName(enumValue, enumValue.ToString());
        }).ToDictionary(q => q.Key, q => q);
    }
}
