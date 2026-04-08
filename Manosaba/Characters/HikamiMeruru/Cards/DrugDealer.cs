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

        private const int EnergyCost = 2;
        private const CardType CardTypeValue = CardType.Skill;
        private const CardRarity Rarity = CardRarity.Uncommon;
        private const TargetType TargetTypeValue = TargetType.Self;
        private const bool ShouldShowInCardLibrary = true;
        private const int CatalystChanceOnUpgrade = 50;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded ? [HoverTipFactory.FromPotion<Catalyst>()] : [];

        private readonly List<PotionModel> _potionPool =
        [
            ModelDb.Potion<LesserBlockPotion>(),
            ModelDb.Potion<LesserPainKillerPotion>(),
            ModelDb.Potion<LesserStrengthPotion>(),
        ];

        public DrugDealer() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
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
