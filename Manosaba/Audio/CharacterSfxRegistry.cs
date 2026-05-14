using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Audio;

public static class CharacterSfxRegistry
{
    /// <summary>
    /// Matches <see cref="BaseLib.Extensions.TypePrefix.GetPrefix"/> for types in the <c>manosaba.*</c> namespace
    /// after <c>ToLowerInvariant</c> (e.g. run <c>Character.Id.Entry</c> <c>MANOSABA-TACHIBANA_SHERRY</c> → <c>manosaba-tachibana_sherry</c>).
    /// </summary>
    private const string ManosabaPrefixedIdLower = "manosaba-";

    private static readonly Dictionary<string, CharacterSfxBase> Registry = new();

    public static void Register(string characterId, CharacterSfxBase sfx)
    {
        Registry[characterId.ToLowerInvariant()] = sfx;
    }

    /// <summary>
    /// Maps in-run <c>Character.Id.Entry</c> (BaseLib-prefixed) to keys used in <see cref="Register"/>.
    /// </summary>
    public static string NormalizeRegistryLookupKey(string characterIdEntry)
    {
        string lower = characterIdEntry.ToLowerInvariant();
        if (lower.StartsWith(ManosabaPrefixedIdLower, StringComparison.Ordinal))
            return lower[ManosabaPrefixedIdLower.Length..];
        return lower;
    }

    public static bool TryGetForCharacterId(string? characterIdEntry, out CharacterSfxBase? sfx)
    {
        sfx = null;
        if (string.IsNullOrEmpty(characterIdEntry))
            return false;

        string key = NormalizeRegistryLookupKey(characterIdEntry);
        if (Registry.TryGetValue(key, out sfx))
            return true;

        // Fallback: allow callers that pass the prefixed form as registry key
        return Registry.TryGetValue(characterIdEntry.ToLowerInvariant(), out sfx);
    }

    public static CharacterSfxBase? TryGetForPlayer(Player? player)
    {
        string? id = player?.Character?.Id.Entry;
        return TryGetForCharacterId(id, out var sfx) ? sfx : null;
    }

    /// <summary>
    /// Returns custom SFX for UI / combat overlay hooks (<c>Patch_CharacterSfx_Interceptor</c>).
    /// Prefers the <b>local</b> player in multiplayer (<see cref="LocalContext.GetMe"/>); falls back to the first
    /// registered Manosaba character in the run when <c>NetId</c> is unset (e.g. some single-player / test paths).
    /// </summary>
    public static CharacterSfxBase? GetCurrentPlayerSfx()
    {
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return null;

        Player? local = LocalContext.GetMe(runState);
        if (local != null)
        {
            CharacterSfxBase? localSfx = TryGetForPlayer(local);
            if (localSfx != null)
                return localSfx;
        }

        foreach (var player in runState.Players)
        {
            string? entry = player.Character?.Id.Entry;
            if (entry != null && TryGetForCharacterId(entry, out var sfx))
                return sfx;
        }

        return null;
    }
}
