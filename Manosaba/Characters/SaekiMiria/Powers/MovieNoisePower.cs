using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class MovieNoisePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Amount <= 0m)
            return;

        if (cardPlay.Card.Owner != Owner.Player)
            return;

        if (cardPlay.Card is not MovieBase)
            return;

        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        List<Creature> enemies = combatState
            .GetOpponentsOf(Owner)
            .Where(e => e.IsHittable)
            .ToList();

        if (enemies.Count == 0)
            return;

        Creature target = Owner.Player.RunState.Rng.CombatTargets.NextItem(enemies);
        await CreatureCmd.Damage(context, target, Amount, ValueProp.Unpowered, Owner);
    }
}

