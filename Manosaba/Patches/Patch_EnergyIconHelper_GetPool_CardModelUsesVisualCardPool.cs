using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

/// <summary>
/// Vanilla <see cref="EnergyIconHelper.GetPool"/> uses <c>player.Character.CardPool</c> whenever a <see cref="CardModel"/>
/// has an owner, so description <c>energyPrefix</c> / text energy icons ignore the card's own pool. Outside an active run
/// (e.g. card library), use <see cref="CardModel.VisualCardPool"/> so Manosaba Common matches CommonCardPool. During a run,
/// <b>local player's</b> mutable cards keep vanilla behavior so in-game text energy matches the character you are playing.
/// </summary>
[HarmonyPatch(typeof(EnergyIconHelper))]
public static class Patch_EnergyIconHelper_GetPool_CardModelUsesVisualCardPool
{
    private static readonly MethodInfo Target = AccessTools.Method(typeof(EnergyIconHelper), "GetPool", [typeof(AbstractModel)])
        ?? throw new InvalidOperationException("EnergyIconHelper.GetPool(AbstractModel) not found.");

    [HarmonyTargetMethod]
    private static MethodBase TargetMethod() => Target;

    [HarmonyPrefix]
    private static bool Prefix(AbstractModel model, ref IPoolModel __result)
    {
        if (model is not CardModel card)
            return true;

        if (ShouldUseVanillaCharacterPoolForLocalPlayerCard(card))
            return true;

        __result = card.VisualCardPool;
        return false;
    }

    private static bool ShouldUseVanillaCharacterPoolForLocalPlayerCard(CardModel card)
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        if (!card.IsMutable || card.Owner == null)
            return false;
        return LocalContext.IsMine(card);
    }
}
