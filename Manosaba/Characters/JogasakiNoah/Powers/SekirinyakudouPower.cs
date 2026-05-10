using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public sealed class SekirinyakudouPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}
