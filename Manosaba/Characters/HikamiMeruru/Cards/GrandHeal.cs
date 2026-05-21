using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class GrandHeal : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InhibitionPower>(), HoverTipFactory.FromPower<MajokaPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new HealVar(20),
            new PowerVar<RegenPower>(10),
            new CalculationBaseVar(1m),
            new CalculationExtraVar(9m),
            new CalculatedVar("InhibitionPower").WithMultiplier(GetMajokaFactor)
        ];
        public GrandHeal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null || base.Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(ownerCreature)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                await CreatureCmd.Heal(item, base.DynamicVars.Heal.BaseValue);
                decimal inhibition = ((CalculatedVar)DynamicVars["InhibitionPower"]).Calculate(null);
                if (inhibition >= 1m)
                    await CommonActions.Apply<InhibitionPower>(choiceContext, item, this, inhibition);

                if (item.Player != null)
                {
                    List<CardModel> list = GetStatuses(item.Player).ToList();
                    foreach (var card in list)
                    {
                        await CardCmd.Exhaust(choiceContext, card);
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }

        private static decimal GetMajokaFactor(CardModel card, Creature? _)
            => Math.Min(card.Owner?.Creature?.GetPowerAmount<MajokaPower>() / 100m ?? 0m, 1m);

        private static IEnumerable<CardModel> GetStatuses(Player owner)
        {
            return owner.PlayerCombatState?.AllCards.Where((CardModel c) => c.Type == CardType.Status && c.Pile?.Type != PileType.Exhaust) ?? [];
        }
    }
}
