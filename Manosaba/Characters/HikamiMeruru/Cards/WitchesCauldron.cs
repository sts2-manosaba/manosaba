using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HikamiWitchesCauldronRelic = manosaba.Characters.HikamiMeruru.Relics.WitchesCauldron;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class WitchesCauldron : PathCustomCardModel
    {

        private const int energyCost = 2;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Unique, CardKeyword.Exhaust];

        public WitchesCauldron() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            _ = choiceContext;
            _ = cardPlay;

            Owner.GetRelic<HikamiWitchesCauldronRelic>()?.MaximizeFirepower();
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
