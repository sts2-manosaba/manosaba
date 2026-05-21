using System.Linq;

using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using Manosaba.Characters.TachibanaSherry.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class WhatDoYouMean : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag>();
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<WeakPower>(),
            HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.Static(StaticHoverTip.Block),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

        public WhatDoYouMean() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target is not { } target || Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            await CommonActions.Apply<WeakPower>(choiceContext, target, this, DynamicVars["WeakPower"].BaseValue);
            if (ownerCreature.GetPowerAmount<SherryDetectiveRewardPower>() > 0m)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, ownerCreature, 2m, ownerCreature, this);
            }

            CardModel? cardModel = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                context: choiceContext,
                player: Owner,
                filter: null,
                source: this)).FirstOrDefault();
            if (cardModel != null)
            {
                await CardCmd.Exhaust(choiceContext, cardModel);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["WeakPower"].UpgradeValueBy(1m);
        }
    }
}
