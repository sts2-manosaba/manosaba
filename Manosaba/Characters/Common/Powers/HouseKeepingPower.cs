using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.Common.Powers;

public class HouseKeepingPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<HouseKeeping>();

    protected override bool IsPositive => true;
}

