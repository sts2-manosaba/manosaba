using Manosaba.Characters.JogasakiNoa.Orbs;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public class RedPaintOrbPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Orb<RedPaintOrb>();
    protected override bool IsPositive => true;
}

