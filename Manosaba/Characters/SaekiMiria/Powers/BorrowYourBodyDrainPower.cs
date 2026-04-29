using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class BorrowYourBodyDrainPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<BorrowYourBody>();
    protected override bool IsPositive => false;
}
