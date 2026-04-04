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
public sealed class BlackPaintOrb : ManosabaOrbModel
{
    public override Color DarkenedColor => new("2C2C2C");

    public override decimal PassiveVal => ModifyPaintOrbValue(5m);

    public override decimal EvokeVal => ModifyPaintOrbValue(10m);

    public override Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
    {
        return Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        PlayPassiveSfx();
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, PassiveVal, Owner.Creature, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        List<Creature> teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer)
            .ToList();

        foreach (Creature teammate in teammates)
        {
            await PowerCmd.Apply<MajokaPower>(teammate, EvokeVal, Owner.Creature, null);
        }

        return teammates;
    }
}
