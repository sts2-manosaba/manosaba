using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public class DissolvePower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Dissolve>();
    protected override bool IsPositive => false;
}
