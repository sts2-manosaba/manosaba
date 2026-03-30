using BaseLib.Utils;
using Godot;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoa.Orbs;

[Pool(typeof(JogasakiNoahOrbPool))]
public sealed class RedPaintOrb : ManosabaOrbModel
{
    public override Color DarkenedColor => new("8A3333");

    public override decimal PassiveVal => ModifyOrbValue(1m);

    public override decimal EvokeVal => ModifyOrbValue(8m);

    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, PassiveVal, Owner.Creature, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        IReadOnlyList<Creature> enemies = CombatState.HittableEnemies;
        if (enemies.Count == 0)
        {
            return Array.Empty<Creature>();
        }

        PlayEvokeSfx();
        await CreatureCmd.Damage(playerChoiceContext, enemies, EvokeVal, ValueProp.Unpowered, Owner.Creature);
        return enemies.ToList();
    }
}
