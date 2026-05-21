using Manosaba.Characters.ShitoAlisa.Visuals;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.ShitoAlisa.Powers;

/// <summary>Red Bind：效果與暈眩相同（施加時立即使目標暈眩）。</summary>
public sealed class RedBindPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun)];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.IsAlive)
            await CreatureCmd.Stun(Owner);

        if (!TestMode.IsOn)
            RedBindVisuals.Sync(Owner, true);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != Owner.Side)
            return Task.CompletedTask;

        // Clear ring VFX as soon as this side's turn ends (enemy turn end for debuffed enemies).
        if (!TestMode.IsOn)
            RedBindVisuals.Sync(Owner, false);

        RemoveInternal();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (!TestMode.IsOn)
            RedBindVisuals.Sync(oldOwner, false);
        return Task.CompletedTask;
    }
}

