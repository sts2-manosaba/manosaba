using HarmonyLib;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using System.Collections;

namespace Manosaba.Patches;

/// <summary>
/// Manosaba locked placeholders stay in <see cref="ModelDb.AllCharacters"/> but must not count as
/// unlocked, random-pool eligible, or otherwise playable until removed from
/// <see cref="ManosabaLockedCharacterIds"/>.
/// </summary>
public static class Patch_UnlockState_ManosabaLockedCharacters
{
    internal static bool CountsAsPlayableCharacter(CharacterModel character) =>
        !ManosabaLockedCharacterIds.IsLocked(character);

    internal static IEnumerable<CharacterModel> FilterLockedCharacters(IEnumerable<CharacterModel> characters) =>
        characters.Where(CountsAsPlayableCharacter);

    [HarmonyPatch(typeof(UnlockState), nameof(UnlockState.Characters), MethodType.Getter)]
    public static class UnlockStateCharactersPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref IEnumerable __result)
        {
            if (__result is not IEnumerable<CharacterModel> characters)
            {
                return;
            }

            __result = FilterLockedCharacters(characters).ToList();
        }
    }
}
