using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class NoahPuppet : PathCustomCardModel
    {
        public override bool GainsBlock => true;
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

        public NoahPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<NoahPuppetCollectionPower>(Owner.Creature, 1m, Owner.Creature, this);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            if (IsUpgraded)
            {
                CardModel? chosen = (await CardSelectCmd.FromHand(
                    prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                    context: choiceContext,
                    player: Owner,
                    filter: null,
                    source: this)).FirstOrDefault();
                if (chosen != null)
                    await CardCmd.Exhaust(choiceContext, chosen);
                return;
            }

            CardPile hand = PileType.Hand.GetPile(Owner);
            CardModel? random = Owner.RunState.Rng.CombatCardSelection.NextItem(hand.Cards);
            if (random != null)
                await CardCmd.Exhaust(choiceContext, random);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(2m);
        }
    }
}
