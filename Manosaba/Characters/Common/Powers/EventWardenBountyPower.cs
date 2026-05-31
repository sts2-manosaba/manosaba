using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.Common.Powers;

public sealed class EventWardenBountyPower : PathCustomPowerModel
{
    public const int GoldReward = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        _ = choiceContext;
        _ = deathAnimLength;
        if (creature != Owner || wasRemovalPrevented || creature.CombatState == null)
        {
            return;
        }

        foreach (Player player in creature.CombatState.Players)
        {
            await PlayerCmd.GainGold(GoldReward, player);
        }
    }
}
