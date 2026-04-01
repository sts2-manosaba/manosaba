using Manosaba.Characters.Common.Commands;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Powers
{
    public class SpaceOutPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;


        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != CombatSide.Player || Owner?.Player == null)
            {
                return;
            }
            ThrowingPlayerChoiceContext choiceContext = new();
            IReadOnlyList<OrbModel> paintOrbs = JogasakiNoahOrbPool.AllOrbs;
            OrbModel randomOrb = paintOrbs[Owner.CombatState.RunState.Rng.CombatOrbGeneration.NextInt(paintOrbs.Count)];
            await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), Owner.Player);
        }

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side == CombatSide.Player)
            {
                HashSet<Type> paintOrbTypes = JogasakiNoahOrbPool.AllOrbs
                    .Select(o => o.GetType())
                    .ToHashSet();
                int distinctColorCount = Owner.Player.PlayerCombatState.OrbQueue.Orbs
                    .Select(o => o.GetType())
                    .Where(paintOrbTypes.Contains)
                    .Distinct()
                    .Count();
                if (distinctColorCount < 8)
                    return;
                await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(Owner.Player.Creature.CombatState);
            }
        }
    }
}
