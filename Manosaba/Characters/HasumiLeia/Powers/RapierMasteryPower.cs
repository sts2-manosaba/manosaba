using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class RapierMasteryPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
}

