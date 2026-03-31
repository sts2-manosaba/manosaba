using Manosaba.Characters.JogasakiNoa.Orbs;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public class PurplePaintOrbPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Orb<PurplePaintOrb>();
    protected override bool IsPositive => false;
}

