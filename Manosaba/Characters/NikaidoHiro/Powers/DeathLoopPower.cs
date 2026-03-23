using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class DeathLoopPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

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
            decimal healPercent = Math.Min(100, creature.GetPowerAmount<MajokaPower>());
            await PowerCmd.Remove<DeathLoopPower>(creature);
            decimal amount = Math.Max(1m, (decimal)creature.MaxHp * (healPercent / 100m));
            await CreatureCmd.Heal(creature, amount);
        }
    }
}
