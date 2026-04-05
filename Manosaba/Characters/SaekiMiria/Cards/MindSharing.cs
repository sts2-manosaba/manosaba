using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class MindSharing : PathCustomCardModel
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        //protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathLoopPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        
        public MindSharing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<PowerModel> powers = new List<PowerModel>();

            List<Creature> teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer)
            .ToList();

            foreach (Creature teammate in teammates)
            {
                foreach(PowerModel item in teammate.Powers)
                {
                    AddPowerToList(powers, item);
                }
            }
            foreach (Creature teammate in teammates)
            {
                foreach (PowerModel item in powers)
                {
                    Console.WriteLine($"Processing power {item.GetType().Name} with amount {item.Amount} for teammate {teammate.Name}");
                    PowerModel? powerById = teammate.GetPowerById(item.Id);
                    if (ShouldIgnoreThisPower(item)) continue;
                    if (powerById != null && !powerById.IsInstanced)
                    {
                        DoHackyThingsForSpecificPowers(powerById);
                        if (item.Amount > powerById.Amount)
                        {
                            await PowerCmd.ModifyAmount(powerById, item.Amount - powerById.Amount, base.Owner.Creature, this);
                        }
                        
                    }
                    else
                    {
                        PowerModel power = (PowerModel)item.ClonePreservingMutability();
                        DoHackyThingsForSpecificPowers(power);
                        await PowerCmd.Apply(power, teammate, item.Amount, base.Owner.Creature, this);
                    }
                }
            }

        }

        private static void AddPowerToList(List<PowerModel> powers, PowerModel power)
        {
            var existing = powers.FirstOrDefault(p => p.GetType() == power.GetType());
            if (existing == null)
            {
                powers.Add(power);
            }
            else
            {
                // if amount is different, take the higher one
                if (existing.Amount != power.Amount)
                {
                    // PowerModel.Amount is read-only, so create a new instance with the higher amount
                    int maxAmount = Math.Max(existing.Amount, power.Amount);
                    // Remove the old instance and add a clone with the higher amount
                    powers.Remove(existing);
                    PowerModel newPower = (PowerModel)power.ClonePreservingMutability();
                    newPower.SetAmount(maxAmount);
                    powers.Add(newPower);
                }
            }
        }

        private static bool ShouldIgnoreThisPower(PowerModel power)
        {
            HashSet<Type> IgnoredPowers = new()
            {
                typeof(MajokaPower),
                typeof(VotePower),
                typeof(CoveredPower),
                typeof(InterceptPower),
                typeof(DeathLoopPower)
            };
            return power != null && IgnoredPowers.Contains(power.GetType());
        }

        private static void DoHackyThingsForSpecificPowers(PowerModel power)
        {
            if (power is ITemporaryPower temporaryPower)
            {
                temporaryPower.IgnoreNextInstance();
            }
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
