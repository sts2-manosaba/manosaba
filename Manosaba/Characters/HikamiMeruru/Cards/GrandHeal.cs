using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class GrandHeal : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RegenPower>(), HoverTipFactory.FromPower<InhibitionPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal, CardKeyword.Exhaust];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(20), new PowerVar<RegenPower>(10), new PowerVar<InhibitionPower>(5)];
        public GrandHeal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                await CreatureCmd.Heal(item, base.DynamicVars.Heal.BaseValue);
                if (DynamicVars["RegenPower"].BaseValue * (Owner.Creature.GetPowerAmount<MajokaPower>() / 100m) >= 1)
                    await PowerCmd.Apply<RegenPower>(item, DynamicVars["RegenPower"].BaseValue * (Owner.Creature.GetPowerAmount<MajokaPower>() / 100m), Owner.Creature, this);
                if (DynamicVars["InhibitionPower"].BaseValue * (Owner.Creature.GetPowerAmount<MajokaPower>() / 100m) >= 1)
                    await PowerCmd.Apply<InhibitionPower>(item, DynamicVars["InhibitionPower"].BaseValue * (Owner.Creature.GetPowerAmount<MajokaPower>() / 100m), Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
