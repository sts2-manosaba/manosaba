using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.Common.Powers
{
    public class VotePower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;
    }
}
