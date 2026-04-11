using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class TraumaSharing : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [];
        public TraumaSharing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            List<PowerModel> originalDebuffs = (from p in cardPlay.Target.Powers
                                                where p.TypeForCurrentAmount == PowerType.Debuff
                                                select (PowerModel)p.ClonePreservingMutability()).ToList();
            foreach (Creature enemy in base.CombatState.HittableEnemies)
            {
                if (enemy == cardPlay.Target)
                {
                    continue;
                }

                foreach (PowerModel item in originalDebuffs)
                {
                    PowerModel powerById = enemy.GetPowerById(item.Id);
                    if (powerById != null && !powerById.IsInstanced)
                    {
                        DoHackyThingsForSpecificPowers(powerById);
                        await PowerCmd.ModifyAmount(powerById, item.Amount, base.Owner.Creature, this);
                    }
                    else
                    {
                        PowerModel power = (PowerModel)item.ClonePreservingMutability();
                        DoHackyThingsForSpecificPowers(power);
                        await PowerCmd.Apply(power, enemy, item.Amount, base.Owner.Creature, this);
                    }
                }
            }

        }


        private static void DoHackyThingsForSpecificPowers(PowerModel power)
        {
            if (power is ITemporaryPower temporaryPower )
            {
                temporaryPower.IgnoreNextInstance();
            }
            else if(power is ManosabaTemporaryStrengthPower temporaryStrengthPower)
            {
                temporaryStrengthPower.IgnoreNextInstance();
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
