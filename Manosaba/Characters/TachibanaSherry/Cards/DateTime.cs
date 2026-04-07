using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class DateTime : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<MajokaPower>(),
            HoverTipFactory.FromPower<DateTimeNoMajokaPower>(),
            HoverTipFactory.FromPower<StrengthPower>()
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(5m)];

        public DateTime() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.Owner.Creature.GetPowerAmount<MajokaPower>() == 0)
            {
                decimal healAmount = base.DynamicVars.Heal.BaseValue + base.Owner.Creature.GetPowerAmount<StrengthPower>();
                if (healAmount > 0)
                    await CreatureCmd.Heal(base.Owner.Creature, healAmount);
            }

            await PowerCmd.Apply<DateTimeNoMajokaPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
