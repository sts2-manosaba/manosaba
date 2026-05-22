using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Init))]
public static class Patch_NCharacterSelectButton_Init_RandomUnlock
{
    private static readonly AccessTools.FieldRef<NCharacterSelectButton, bool> IsLockedRef =
        AccessTools.FieldRefAccess<NCharacterSelectButton, bool>("_isLocked");

    private static readonly AccessTools.FieldRef<NCharacterSelectButton, TextureRect> IconRef =
        AccessTools.FieldRefAccess<NCharacterSelectButton, TextureRect>("_icon");

    private static readonly AccessTools.FieldRef<NCharacterSelectButton, TextureRect> LockRef =
        AccessTools.FieldRefAccess<NCharacterSelectButton, TextureRect>("_lock");

    [HarmonyPostfix]
    private static void Postfix(NCharacterSelectButton __instance, CharacterModel character)
    {
        if (character is not RandomCharacter)
        {
            return;
        }

        UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
        bool locked = !ManosabaCharacterSelectRandomGate.AreAllPlayableCharactersUnlocked(unlockState);
        IsLockedRef(__instance) = locked;

        TextureRect icon = IconRef(__instance);
        TextureRect lockIcon = LockRef(__instance);
        if (locked)
        {
            icon.Texture = character.CharacterSelectLockedIcon;
            lockIcon.Visible = true;
        }
        else
        {
            icon.Texture = character.CharacterSelectIcon;
            lockIcon.Visible = false;
        }
    }
}
