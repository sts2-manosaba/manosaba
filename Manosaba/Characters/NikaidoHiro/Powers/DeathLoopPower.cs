using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class DeathLoopPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HealPercent", 0m)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

        public override Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            SyncHealPercent();
            return Task.CompletedTask;
        }

        public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power is MajokaPower && power.Owner == Owner)
                SyncHealPercent();
            return Task.CompletedTask;
        }

        public override bool ShouldDieLate(Creature creature)
        {
            if (creature != base.Owner || base.Amount < 1)
            {
                return true;
            }

            return false;
        }

        public override async Task AfterPreventingDeath(Creature creature)
        {
            Flash();
            decimal healPercent = Math.Min(100m, creature.GetPowerAmount<MajokaPower>());
            await PowerCmd.Remove<DeathLoopPower>(creature);
            decimal amount = Math.Max(1m, (decimal)creature.MaxHp * (healPercent / 100m));
            await CreatureCmd.Heal(creature, amount);
        }

        private void SyncHealPercent()
        {
            if (Owner == null)
                return;
            DynamicVars["HealPercent"].BaseValue = Math.Min(100m, Owner.GetPowerAmount<MajokaPower>());
        }
    }
}
