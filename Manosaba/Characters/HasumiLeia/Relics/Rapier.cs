using BaseLib.Extensions;
using BaseLib.Utils;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.HasumiLeia.Relics
{

    [Pool(typeof(HasumiLeiaRelicPool))]
    public sealed class Rapier : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected decimal basePercentage = 0.6m;
        private bool _reflectedSinceLastTurnStart;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.ForEnergy(this)];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (ReflectionDamageGuard.IsActive)
                return;

            if (target == base.Owner.Creature && result.BlockedDamage > 0 && props.IsPoweredAttack_() && dealer != null)
            {
                _reflectedSinceLastTurnStart = true;
                decimal damage = decimal.Ceiling(result.BlockedDamage * basePercentage);
                int extraHits = target.GetPowerAmount<RapierMasteryPower>();
                int hits = 1 + Math.Max(0, extraHits);

                await ReflectionDamageGuard.Run(async () =>
                {
                    for (int i = 0; i < hits; i++)
                    {
                        if (!dealer.IsAlive)
                            return;
                        if (cardSource is Objection)
                            return;
                        if (dealer.IsPlayer && cardSource == null)
                            return;

                        await CreatureCmd.Damage(choiceContext, dealer, damage, ValueProp.Move, base.Owner.Creature, null);
                    }
                });
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature != player.Creature)
                return;

            if (!_reflectedSinceLastTurnStart)
                return;

            _reflectedSinceLastTurnStart = false;

            decimal vigor = base.Owner.Creature.GetPowerAmount<VigorPower>();
            if (vigor > 0m)
                await PowerCmd.Apply<VigorPower>(base.Owner.Creature, -vigor, base.Owner.Creature, null);
        }


        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            
            int level = RelicLevel;
            basePercentage = 0.6m + (level - 1) * 0.1m; // Increases by 10% each level, starting at 60% at level 1, 100% at level 5
        }
    }
}
