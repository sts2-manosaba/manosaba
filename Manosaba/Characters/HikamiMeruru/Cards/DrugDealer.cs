using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class DrugDealer : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        private const int energyCost = 2;
        private const CardType cardTypeValue = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetTypeValue = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        private const int CatalystChanceOnUpgrade = 50;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded ? [HoverTipFactory.FromPotion<Catalyst>()] : [];

        private readonly List<PotionModel> _potionPool =
        [
            ModelDb.Potion<LesserBlockPotion>(),
            ModelDb.Potion<LesserPainKillerPotion>(),
            ModelDb.Potion<LesserFlexPotion>(),
        ];

        public DrugDealer() : base(energyCost, cardTypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<Player> teammates = (from c in CombatState.GetTeammatesOf(Owner.Creature)
                                      where c != null && c.IsAlive && c.IsPlayer && c.Player != null
                                      select c.Player!)
                .Append(Owner)
                .Distinct()
                .ToList();

            foreach (Player teammate in teammates)
            {
                // TODO if arisa then 2x
                PotionModel potionModel = _potionPool[Owner.RunState.Rng.CombatPotionGeneration.NextInt(_potionPool.Count)];
                await PotionCmd.TryToProcure(potionModel.ToMutable(), teammate);
            }

            if (IsUpgraded && Owner.RunState.Rng.CombatPotionGeneration.NextInt(100) < CatalystChanceOnUpgrade)
            {
                await PotionCmd.TryToProcure<Catalyst>(Owner);
            }
        }

        protected override void OnUpgrade()
        {
        }
    }
}
