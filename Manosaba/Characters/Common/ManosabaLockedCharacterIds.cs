using Manosaba.Audio;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common;

/// <summary>
/// Character select placeholders that appear locked until removed from this set.
/// </summary>
public static class ManosabaLockedCharacterIds
{
    public const string SakurabaEma = "sakuraba_ema";
    public const string SawatariCoco = "sawatari_coco";
    public const string TsukishiroYuki = "tsukishiro_yuki";

    private static readonly HashSet<string> LockedIds =
    [
        SakurabaEma,
        SawatariCoco,
        TsukishiroYuki,
    ];

    /// <summary>
    /// <see cref="CharacterModel.Id.Entry"/> uses BaseLib form e.g. <c>MANOSABA-SAKURABA_EMA</c>;
    /// registry keys use snake_case <c>sakuraba_ema</c>.
    /// </summary>
    public static string NormalizeCharacterIdEntry(string? characterIdEntry)
    {
        if (string.IsNullOrEmpty(characterIdEntry))
            return string.Empty;

        return CharacterSfxRegistry.NormalizeRegistryLookupKey(characterIdEntry);
    }

    public static bool IsLocked(CharacterModel character) =>
        LockedIds.Contains(NormalizeCharacterIdEntry(character.Id.Entry));
}
