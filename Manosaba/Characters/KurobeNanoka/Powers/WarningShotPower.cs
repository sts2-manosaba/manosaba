using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.KurobeNanoka.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public class WarningShotPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<WarningShot>();
    protected override bool IsPositive => false;
}
