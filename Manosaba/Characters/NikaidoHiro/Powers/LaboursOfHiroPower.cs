using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
