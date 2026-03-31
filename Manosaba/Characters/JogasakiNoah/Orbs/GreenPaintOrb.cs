using BaseLib.Utils;
using Godot;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.JogasakiNoa.Orbs;

[Pool(typeof(JogasakiNoahOrbPool))]
public sealed class GreenPaintOrb : ManosabaOrbModel
{
    private decimal _passiveVal = 3m;

    public override Color DarkenedColor => new("3D8A4B");

    public override decimal PassiveVal => ModifyOrbValue(_passiveVal);

    public override decimal EvokeVal => ModifyOrbValue(5m);

    public override Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        return Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target != null)
        {
            throw new InvalidOperationException("Green paint orb cannot target creatures.");
        }

        decimal healAmount = PassiveVal;
        if (healAmount <= 0m)
        {
            return;
        }

        Trigger();
        PlayPassiveSfx();
        await CreatureCmd.Heal(Owner.Creature, healAmount);
        _passiveVal = Math.Max(0m, _passiveVal - 1m);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        List<Creature> teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer)
            .ToList();

        foreach (Creature teammate in teammates)
        {
            await CreatureCmd.Heal(teammate, EvokeVal);
        }

        return teammates;
    }
}
