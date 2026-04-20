using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Powers;

public class PortableFletchingStationPower : PathCustomPowerModel
{
    public const string ProcChanceVar = "ProcChance";
    public const int ProcChancePercent = 50;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(ProcChanceVar, ProcChancePercent)];

    public async Task TriggerFlash()
    {
        Flash();
        await Task.CompletedTask;
    }
}
