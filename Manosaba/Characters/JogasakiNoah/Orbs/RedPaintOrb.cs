using BaseLib.Utils;
using Godot;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.JogasakiNoa.Orbs;

[Pool(typeof(JogasakiNoahOrbPool))]
public sealed class RedPaintOrb : ManosabaOrbModel
{
    public override Color DarkenedColor => new("8A3333");

    public override decimal PassiveVal => ModifyOrbValue(1m);

    public override decimal EvokeVal => ModifyOrbValue(8m);

    public override async Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        await PowerCmd.Apply<RedPaintOrbPower>(Owner.Creature, PassiveVal, Owner.Creature, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        List<Creature> teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer)
            .ToList();

        foreach (Creature teammate in teammates)
        {
            await PowerCmd.Apply<RedPaintOrbPower>(teammate, PassiveVal, Owner.Creature, null);
        }
        return teammates;
    }
}
