using Manosaba.Audio;

namespace manosaba.Characters.SawatariCoco.Helpers;

/// <summary>
/// Character SFX hook for <see cref="SawatariCoco"/>; register via <c>CharacterSfxRegistry</c> in Entry.
/// </summary>
public sealed class SawatariCocoSfx : CharacterSfxBase
{
    public static readonly SawatariCocoSfx Instance = new();
}
