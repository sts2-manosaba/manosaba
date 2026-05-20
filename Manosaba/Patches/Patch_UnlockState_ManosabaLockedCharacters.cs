using HarmonyLib;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using System.Collections;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(UnlockState), nameof(UnlockState.Characters), MethodType.Getter)]
public static class Patch_UnlockState_ManosabaLockedCharacters
{
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable __result)
    {
        if (__result is not IEnumerable<CharacterModel> characters)
        {
            return;
        }

        __result = characters.Where(c => !ManosabaLockedCharacterIds.IsLocked(c)).ToList();
    }
}
