using Manosaba.Characters.Common.Commands;
using Manosaba.Extensions;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.TonoHanna.Powers;

/// <summary>
/// Single visible buff: stack count = number of distinct puppet companions with a collection charge (Amount &gt; 0).
/// Icons use <c>puppet_collection_power.png</c>; per-character collection powers stay hidden and only drive the ring + this tally.
/// </summary>
public sealed class PuppetCollectionSummaryPower : PathCustomPowerModel
{
    private const string SharedIconFile = "puppet_collection_power.png";
    private const string WinVfxScenePath = "res://Manosaba/scenes/tono_hanna/vfx/puppet_collection.tscn";
    private const string WinBgmEventPath = "event:/Manosaba/audio/bgm/world_vanquisher.mp3";
    private const int CollectionWinThreshold = 13;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => SharedIconFile.PowerImagePath();

    public override string CustomBigIconPath => SharedIconFile.PowerImagePath();

    public override string CustomBigBetaIconPath => SharedIconFile.PowerImagePath();

    public static async Task SyncToRosterAsync(Creature owner)
    {
        if (TestMode.IsOn)
            return;

        int target = CountActiveCollectionKinds(owner);
        PuppetCollectionSummaryPower? summary = owner.GetPower<PuppetCollectionSummaryPower>();
        int current = summary?.Amount ?? 0;
        if (target == current)
            return;

        if (target == 0)
        {
            if (summary != null)
                await PowerCmd.Remove(summary);
            return;
        }

        await PowerCmd.Apply<PuppetCollectionSummaryPower>(owner, target - (decimal)current, owner, null, silent: true);
    }

    private static int CountActiveCollectionKinds(Creature owner)
    {
        int n = 0;
        foreach (PowerModel p in owner.Powers)
        {
            if (p is PuppetCollectionPowerBase && p.Amount > 0)
                n++;
        }

        return n;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);
        if (power is not PuppetCollectionSummaryPower || power.Owner == null)
            return;
        if (CountActiveCollectionKinds(power.Owner) < CollectionWinThreshold)
            return;
        SfxCmd.Play(WinBgmEventPath, 0.8f);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer)
            return;
        if (CountActiveCollectionKinds(Owner) < CollectionWinThreshold)
            return;
        CombatState? combatState = Owner.CombatState;
        if (combatState == null)
            return;

        await ManosabaVfxCmd.PlaySceneAtCombatCenterAndWait(
            WinVfxScenePath,
            fitCoverViewport: true,
            spriteNodeNames: ["StillA", "StillB"]);
        await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(combatState);
    }
}
