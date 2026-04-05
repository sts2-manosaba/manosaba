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
            // PowerId -> max amount
            Dictionary<string, int> powerMap = new();

            var teammates = CombatState?.GetTeammatesOf(Owner.Creature)
                ?.Where(c => c != null && c.IsAlive && c.IsPlayer)
                .ToList();

            if (teammates == null || teammates.Count == 0)
            {
                Console.WriteLine("No valid teammates found for Mind Sharing.");
                return;
            }

            // ===== Collect powers (DATA ONLY) =====
            foreach (var teammate in teammates)
            {
                if (teammate?.Powers == null) continue;

                foreach (var item in teammate.Powers)
                {
                    if (item == null) continue;
                    if (ShouldIgnoreThisPower(item)) continue;

                    Console.WriteLine($"Collected power {item.GetType().Name} amount {item.Amount} from {teammate.Name}");

                    if (!powerMap.ContainsKey(item.Id.ToString()))
                    {
                        powerMap[item.Id.ToString()] = item.Amount;
                    }
                    else
                    {
                        powerMap[item.Id.ToString()] = Math.Max(powerMap[item.Id.ToString()], item.Amount);
                    }
                }
            }

            // ===== Apply powers =====
            foreach (var teammate in teammates)
            {
                if (teammate == null) continue;

                foreach (var kvp in powerMap)
                {
                    var powerId = ModelId.Deserialize(kvp.Key);
                    int targetAmount = kvp.Value;

                    PowerModel? existing = null;

                    try
                    {
                        existing = teammate.GetPowerById(powerId);
                    }
                    catch
                    {
                        continue;
                    }

                    if (existing != null && !existing.IsInstanced)
                    {
                        Console.WriteLine($"Updating existing power {powerId} on {teammate.Name}");

                        DoHackyThingsForSpecificPowers(existing);

                        if (targetAmount > existing.Amount)
                        {
                            await PowerCmd.ModifyAmount(
                                existing,
                                targetAmount - existing.Amount,
                                Owner.Creature,
                                this
                            );
                        }
                    }
                    else
                    {
                        // Find a source power to clone from (any teammate)
                        var source = teammates
                            .SelectMany(t => t.Powers ?? Enumerable.Empty<PowerModel>())
                            .FirstOrDefault(p => p != null && p.Id == powerId);

                        if (source == null) continue;

                        var clone = source.ClonePreservingMutability() as PowerModel;
                        if (clone == null) continue;

                        Console.WriteLine($"Applying new power {powerId} to {teammate.Name}");

                        DoHackyThingsForSpecificPowers(clone);

                        await PowerCmd.Apply(
                            clone,
                            teammate,
                            targetAmount,
                            Owner.Creature,
                            this
                        );
                    }
                }
            }
        }

        private static void AddPowerToList(Dictionary<string, int> powers, PowerModel power)
        {
            if (power == null) return;

            if (!powers.ContainsKey(power.Id.ToString()))
            {
                powers[power.Id.ToString()] = power.Amount;
            }
            else
            {
                powers[power.Id.ToString()] = Math.Max(powers[power.Id.ToString()], power.Amount);
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
            if (power is ManosabaTemporaryStrengthPower manosabaTemporaryStrengthPower)
            {
                manosabaTemporaryStrengthPower.IgnoreNextInstance();
            }
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
