using BaseLib.Abstracts;
using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class LaboursOfHiroPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner.Player)
            {
                await PowerCmd.Apply<MajokaPower>(Owner.Player.Creature, 20, Owner.Player.Creature, null);
                await PowerCmd.Apply<StrengthPower>(Owner.Player.Creature, 1, Owner.Player.Creature, null);
            }
        }

        public override string CustomPackedIconPath => "LaboursOfHiro.png".PowerImagePath();
        public override string CustomBigIconPath => "LaboursOfHiro.png".PowerImagePath();
        public override string CustomBigBetaIconPath => "LaboursOfHiro.png".PowerImagePath();
    }
}
