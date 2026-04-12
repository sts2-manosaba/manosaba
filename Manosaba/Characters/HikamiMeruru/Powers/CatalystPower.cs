using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.Powers
{
    public sealed class CatalystPower : PathCustomPowerModel
    {
        private const string ThresholdVar = "Threshold";
        private bool _resolvingThreshold;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(ThresholdVar, 3m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<Tredecim>()];

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

            int threshold = DynamicVars[ThresholdVar].IntValue;
            if (_resolvingThreshold || power is not CatalystPower || power.Owner != Owner || power.Amount < threshold)
            {
                return;
            }

            _resolvingThreshold = true;
            try
            {
                await PowerCmd.Remove(this);
                if (Owner.Player != null)
                {
                    await PotionCmd.TryToProcure<Tredecim>(Owner.Player);
                }
            }
            finally
            {
                _resolvingThreshold = false;
            }
        }
    }
}
