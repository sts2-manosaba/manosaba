using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Extensions;

public sealed class RelicLevelingHoverTipPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldPlayVfx => false;
}
