using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Extensions;

public static class LockboxOptionExtensions
{
    public static Box ConvertToBox(this LockboxOption box)
    {
        return box switch
        {
            LockboxOption.Copper => Box.Copper,
            LockboxOption.Steel => Box.Steel,
            LockboxOption.Silver => Box.Silver,
            LockboxOption.Platinum => Box.Platinum,
            LockboxOption.Gold => Box.Gold,
            LockboxOption.Titanium => Box.Titanium,
            LockboxOption.Iron => Box.Iron,
            LockboxOption.Mirrored => Box.Mirrored,
            LockboxOption.Slime => Box.Slime,
            _ => throw new InvalidOperationException($"Box '{box}' is not a valid lockbox type.")
        };
    }
}
