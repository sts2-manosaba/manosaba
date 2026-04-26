using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using HannaCharacter = manosaba.Characters.TonoHanna.TonoHanna;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Characters.TonoHanna.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class LifelongFriend : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<LifelongFriendPower>(1)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<LifelongFriendPower>(),
            HoverTipFactory.FromCard<Boulders>(),
            HoverTipFactory.FromCard<RefuseMajo>(),
        ];

        public LifelongFriend() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            decimal stack = DynamicVars["LifelongFriendPower"].BaseValue;
            LifelongFriendPower? selfPower = await PowerCmd.Apply<LifelongFriendPower>(base.Owner.Creature, stack, base.Owner.Creature, this);
            LifelongFriendPower? allyPower = await PowerCmd.Apply<LifelongFriendPower>(cardPlay.Target, stack, base.Owner.Creature, this);
            selfPower?.SetPartner(cardPlay.Target);
            allyPower?.SetPartner(base.Owner.Creature);

            if (cardPlay.Target.Player?.Character is HannaCharacter)
            {
                Player hannaAlly = cardPlay.Target.Player!;
                CombatState combatState = hannaAlly.Creature.CombatState
                    ?? throw new InvalidOperationException("LifelongFriend Refuse placement requires active combat.");
                CardModel refuse = combatState.CreateCard(ModelDb.Card<RefuseMajo>(), hannaAlly);
                CardPileAddResult refuseResult = await CardPileCmd.Add(refuse, PileType.Draw, CardPilePosition.Top, this);
                if (LocalContext.IsMe(hannaAlly.Creature))
                {
                    CardCmd.PreviewCardPileAdd(refuseResult);
                }
            }
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
