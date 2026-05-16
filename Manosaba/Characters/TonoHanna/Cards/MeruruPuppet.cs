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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class MeruruPuppet : PathCustomCardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MeruruPuppetCollectionPower>(1)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromCard<AnAnPuppet>(),
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ];

        public MeruruPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MeruruPuppetCollectionPower>(Owner.Creature, DynamicVars["MeruruPuppetCollectionPower"].BaseValue, Owner.Creature, this);

            IReadOnlyList<CardModel> exhaust = PileType.Exhaust.GetPile(Owner).Cards;
            if (exhaust.Count == 0)
                return;

            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
            CardModel? chosen = (await CardSelectCmd.FromSimpleGrid(choiceContext, exhaust, Owner, prefs)).FirstOrDefault();
            if (chosen != null)
            {
                await CardPileCmd.Add(chosen, PileType.Hand);
                if (chosen is AnAnPuppet && Owner.Creature is { } ownerCreature)
                {
                    await CreatureCmd.Heal(ownerCreature, 3m);
                }
            }
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}
