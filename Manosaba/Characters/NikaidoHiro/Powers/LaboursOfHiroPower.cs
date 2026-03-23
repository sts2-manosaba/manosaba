using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class LaboursOfHiroPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner.Player)
            {
                await PowerCmd.Apply<MajokaPower>(Owner.Player.Creature, 20, Owner.Player.Creature, null);
            }
        }
    }
}
