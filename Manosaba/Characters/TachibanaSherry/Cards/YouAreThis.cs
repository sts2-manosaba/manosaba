using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using Manosaba.Characters.TachibanaSherry.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class YouAreThis : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag>();
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromPower<CluePower>(),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new PowerVar<StrengthPower>(3m),
            new PowerVar<CluePower>(2m),
        ];

        public YouAreThis() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null || base.Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            await PowerCmd.Apply<StrengthPower>(
                ownerCreature,
                DynamicVars["StrengthPower"].BaseValue,
                ownerCreature,
                this);
            await PowerCmd.Apply<CluePower>(
                ownerCreature,
                DynamicVars["CluePower"].BaseValue,
                ownerCreature,
                this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["StrengthPower"].UpgradeValueBy(2m);
            DynamicVars["CluePower"].UpgradeValueBy(1m);
        }
    }
}
