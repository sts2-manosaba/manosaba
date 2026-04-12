using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace Manosaba.Patches;

/// <summary>
/// 在「隨機」占位符被換成具體角色時，若設定為僅 Mod 池，則把非 Mod 結果改抽成 Mod 自機。
/// 角色選單點選會走 <see cref="StartRunLobby.SetLocalCharacter"/> → 內部再呼叫 <c>ChangeCharacter</c>。
/// </summary>
/// <remarks>
/// 若玩家在「隨機」高亮下改點原版角色再出發，也會被視為隨機結算而改抽 Mod（與隨機到原版再替換同一條路）。
/// 多人時請所有人將「隨機角色池」設為同一選項（與難度／血量設定相同要求）。
/// </remarks>
[HarmonyPatch(typeof(StartRunLobby), nameof(StartRunLobby.SetLocalCharacter))]
public static class Patch_StartRunLobby_SetLocalCharacter_ManosaRandomFilter
{
    [HarmonyPrefix]
    private static void Prefix(StartRunLobby __instance, ref CharacterModel character)
    {
        ManosabaRandomCharacterPool.TryReplaceVanillaPickWhenRandomSlot(
            ref character,
            ManosabaRandomCharacterPool.ReadLobbyPlayerCharacter(__instance.LocalPlayer));
    }
}
