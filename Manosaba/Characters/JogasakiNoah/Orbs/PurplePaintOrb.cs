using BaseLib.Utils;
using Godot;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.JogasakiNoa.Orbs;

[Pool(typeof(JogasakiNoahOrbPool))]
public sealed class PurplePaintOrb : ManosabaOrbModel
{
    public override Color DarkenedColor => new("6B4A8D");

    public override decimal PassiveVal => ModifyOrbValue(3m);

    public override decimal EvokeVal => ModifyOrbValue(5m);

    public override Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        return Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target != null)
        {
            throw new InvalidOperationException("Purple paint orb cannot target creatures.");
        }

        List<Creature> candidates = CombatState.HittableEnemies
            .Where(e => e.IsHittable)
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        Creature chosen = Owner.RunState.Rng.CombatTargets.NextItem(candidates);
        Trigger();
        PlayPassiveSfx();
        await PowerCmd.Apply<DoomPower>(chosen, PassiveVal, Owner.Creature, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        List<Creature> enemies = CombatState.HittableEnemies
            .Where(e => e.IsHittable)
            .ToList();
        if (enemies.Count == 0)
        {
            return [];
        }

        await PowerCmd.Apply<DoomPower>(enemies, EvokeVal, Owner.Creature, null);
        return enemies;
    }
}
