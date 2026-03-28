using BaseLib.Utils;
using System;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class LifelongFriend : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<DynamicVar> CanonicalVars => [];

        public LifelongFriend() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            LifelongFriendPower? selfPower = await PowerCmd.Apply<LifelongFriendPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
            LifelongFriendPower? allyPower = await PowerCmd.Apply<LifelongFriendPower>(cardPlay.Target, 1m, base.Owner.Creature, this);
            selfPower?.SetPartner(cardPlay.Target);
            allyPower?.SetPartner(base.Owner.Creature);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
