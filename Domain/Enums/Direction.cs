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

/*
@GetEffectiveKey(Direction.Left)⬅
  itr
  render1: UserIcon
Left ⬅
  !itr
  render2: Name Icon
@systemKeybindings[Direction.Left]
  !itr
  omni[Left].SystemKey
private static readonly Direction[] AllDirections = { Direction.Left, Direction.Down, Direction.Up, Direction.Right };
  kill
  omni.Check
formModel.Left
  !itr
  omni[Left].UserKey
{ Direction.Left, ("⬅", "Left") },
  !itr
  omni.new
{ Direction.Left, "h" },
  !itr
  omni.new
public string Left { get; set; } = "";
  kill
  omni[Left].UserKey
case Direction.Left: Left = value; break;
  kill
var keys = new[] { model.Left, model.Down, model.Up, model.Right }
  ??
userKeybindings[direction] = formModel.GetKey(direction) ?? "";
  kill
  Direction.Left => Left,
*/

// internal static class Domain02
// {
//   public static readonly Dictionary<Direction, Domain01> Qqq = EnumHelper.Www<Domain01>([
//       new { SystemDef = "h", Icon = "⬅" },
//       new { SystemDef = "j", Icon = "⬇" },
//       new { SystemDef = "k", Icon = "⬆" },
//       new { SystemDef = "l", Icon = "➡" },
//   ]);
//   public static void Eee()
//   {
//     var v1 = string.Join("\n", Qqq.Values.Select(q => $"{q.UserDef}{q.Icon}"));
//     var t1 = Qqq[Direction.Left];
//     var v2 = $"{t1.Name} {t1.Icon}";
//     var v3 = t1.SystemDef;
//
//
//   }
// }
//
// internal static class EnumHelper
// {
//     /// <summary>
//     /// Returns a HashSet of all values for the specified enum type.
//     /// </summary>
//     /// <typeparam name="T">The enum type</typeparam>
//     /// <param name="includeNone">Whether to include the zero value (None). Defaults to false.</param>
//     /// <returns>HashSet of enum values</returns>
//     public static HashSet<T> GetValues<T>(bool includeNone = false) where T : struct, Enum
//     {
//         return Enum.GetValues<T>().Skip(includeNone ? 0 : 1).ToHashSet();
//     }
//
//     /// <summary>
//     /// Returns a HashSet of all string names for the specified enum type.
//     /// </summary>
//     /// <typeparam name="T">The enum type</typeparam>
//     /// <param name="includeNone">Whether to include the zero value name (None). Defaults to false.</param>
//     /// <returns>HashSet of enum string literals</returns>
//     public static HashSet<string> GetNames<T>(bool includeNone = false) where T : struct, Enum
//     {
//         return Enum.GetNames<T>()
//             .Skip(includeNone ? 0 : 1)
//             .ToHashSet();
//     }
// }
