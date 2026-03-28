using System;
using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.TachibanaSherry.Relics
{
    [Pool(typeof(TachibanaSherryRelicPool))]
    public sealed class FairyHeart : PathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature != player.Creature)
                return;

            decimal majoka = base.Owner.Creature.GetPowerAmount<MajokaPower>();
            if (majoka > 0)
                await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, -majoka, player.Creature, null);
        }

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != base.Owner.Creature)
                return 0m;

            decimal strength = base.Owner.Creature.GetPowerAmount<StrengthPower>();
            if (strength <= 0m)
                return 0m;

            return strength*-1m;
        }
    }
}
