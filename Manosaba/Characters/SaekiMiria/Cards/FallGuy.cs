using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class FallGuy : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Execution];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)];
        public FallGuy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            List<PowerModel> transferDebuffs = target.Powers
                .Where(power => power.TypeForCurrentAmount == PowerType.Debuff)
                .Where(power => !ShouldIgnoreThisPower(power))
                .Select(power => (PowerModel)power.ClonePreservingMutability())
                .ToList();

            foreach (PowerModel originalPower in target.Powers
                         .Where(power => power.TypeForCurrentAmount == PowerType.Debuff)
                         .Where(power => !ShouldIgnoreThisPower(power))
                         .ToList())
            {
                if (originalPower.IsInstanced)
                {
                    await PowerCmd.Remove(originalPower);
                    continue;
                }

                if (originalPower.Amount > 0m)
                {
                    await PowerCmd.ModifyAmount(originalPower, -originalPower.Amount, ownerCreature, this);
                }
            }

            foreach (PowerModel debuff in transferDebuffs)
            {
                if (debuff.Amount <= 0m)
                {
                    continue;
                }

                PowerModel? existingPower = ownerCreature.GetPowerById(debuff.Id);
                if (existingPower != null && !existingPower.IsInstanced)
                {
                    DoHackyThingsForSpecificPowers(existingPower);
                    await PowerCmd.ModifyAmount(existingPower, debuff.Amount, ownerCreature, this);
                }
                else
                {
                    PowerModel clone = (PowerModel)debuff.ClonePreservingMutability();
                    DoHackyThingsForSpecificPowers(clone);
                    await PowerCmd.Apply(clone, ownerCreature, debuff.Amount, ownerCreature, this);
                }
            }

            await CreatureCmd.GainBlock(ownerCreature, DynamicVars.Block, cardPlay);
        }

        private static bool ShouldIgnoreThisPower(PowerModel power)
        {
            HashSet<Type> ignoredPowers = MiriaConstants.IgnoredPowers;
            return ignoredPowers.Contains(power.GetType());
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
            DynamicVars.Block.UpgradeValueBy(3m);
        }
    }
}
