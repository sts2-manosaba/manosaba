using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public class HouseKeepingPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<HouseKeeping>();
    protected override bool IsPositive => true;
}

