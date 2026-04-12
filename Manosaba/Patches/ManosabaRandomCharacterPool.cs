using System.Linq;
using System.Reflection;
using Manosaba;
using Manosaba.Config;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace Manosaba.Patches;

/// <summary>
/// 「隨機」抽籤時篩選 Mod 自機。以執行時型別所屬 <see cref="Assembly"/> 是否為本 Mod DLL 判定，
/// 比依賴命名空間字串更可靠（避免 ModelDb / 產生型別與預期 namespace 不一致時池為空）。
/// </summary>
internal static class ManosabaRandomCharacterPool
{
    private static readonly Lazy<FieldInfo?> LobbyPlayerCharacterField = new(
        () => typeof(LobbyPlayer).GetField(
            "character",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

    private static readonly Lazy<FieldInfo?> LobbyPlayerIdField = new(
        () => typeof(LobbyPlayer).GetField(
            "id",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

    private static Assembly ModAssembly => typeof(Entry).Assembly;

    /// <summary>參考組件中的 <see cref="LobbyPlayer"/> 未必公開 <c>character</c> 欄位，改以反射讀取。</summary>
    internal static CharacterModel? ReadLobbyPlayerCharacter(LobbyPlayer? player)
    {
        if (player is null)
        {
            return null;
        }

        return LobbyPlayerCharacterField.Value?.GetValue(player) as CharacterModel;
    }

    internal static ulong? ReadLobbyPlayerId(LobbyPlayer? player)
    {
        if (player is null)
        {
            return null;
        }

        object? v = LobbyPlayerIdField.Value?.GetValue(player);
        return v is ulong u ? u : null;
    }

    internal static bool IsManosabaPlayableCharacter(CharacterModel c) =>
        c is not RandomCharacter && ReferenceEquals(c.GetType().Assembly, ModAssembly);

    internal static CharacterModel[] BuildManosabaOnlyPool()
    {
        return ModelDb.AllCharacters.Where(IsManosabaPlayableCharacter).ToArray();
    }

    /// <summary>
    /// 目前欄位仍為「隨機」占位、且即將換成的角色為非 Mod 自機時，改從 Mod 池抽籤（出發時
    /// <see cref="MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.StartRunLobby.BeginRunLocally"/> 會直接呼叫 <c>ChangeCharacter</c> 而不走 <c>SetLocalCharacter</c>）。
    /// </summary>
    /// <remarks>
    /// 單人與多人皆用同一套映射：以「原版／UI 傳入的角色」在 <see cref="ModelDb.AllCharacters"/> 列舉中的索引，
    /// 對排序後的 Manosaba 池取模（與種子隨機在多人間一致，單機也不再額外使用 <c>Rng.Chaotic</c>）。
    /// 多人時各玩家仍須在設定選同一項（僅 Manosaba / 全角色）；若一端 ManosabaOnly、一端 All，結算仍會不一致。
    /// </remarks>
    internal static void TryReplaceVanillaPickWhenRandomSlot(ref CharacterModel incoming, CharacterModel? slotCurrent)
    {
        if (ManosabaConfig.RandomCharacterPool != RandomCharacterPoolMode.ManosabaCharactersOnly)
        {
            return;
        }

        if (slotCurrent is not RandomCharacter)
        {
            return;
        }

        if (incoming is RandomCharacter)
        {
            return;
        }

        if (IsManosabaPlayableCharacter(incoming))
        {
            return;
        }

        CharacterModel[] pool = BuildManosabaOnlyPool()
            .OrderBy(c => c.Id.Entry, StringComparer.Ordinal)
            .ToArray();
        if (pool.Length == 0)
        {
            Log.Warn(
                "[Manosaba] RandomCharacterPool=ManosabaOnly but pool empty; vanilla random pick kept.");
            return;
        }

        incoming = MapVanillaRandomPickToManosabaPool(incoming, pool);
    }

    /// <summary>
    /// 以與 <c>BeginRunLocally</c> 相同的 <see cref="ModelDb.AllCharacters"/> 列舉順序，將原版抽中的角色換成 Mod 池內對應項。
    /// </summary>
    private static CharacterModel MapVanillaRandomPickToManosabaPool(CharacterModel vanillaPick, CharacterModel[] poolSorted)
    {
        CharacterModel[] all = ModelDb.AllCharacters.ToArray();
        int idx = -1;
        for (int j = 0; j < all.Length; j++)
        {
            if (all[j].Id.Equals(vanillaPick.Id))
            {
                idx = j;
                break;
            }
        }

        if (idx < 0)
        {
            int h = StringComparer.Ordinal.GetHashCode(vanillaPick.Id.Entry ?? string.Empty);
            idx = (h & int.MaxValue) % poolSorted.Length;
        }

        return poolSorted[idx % poolSorted.Length];
    }
}
