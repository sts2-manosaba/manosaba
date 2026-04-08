using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class FluxPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
    }
}
