using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Models;
using manosaba.Extensions;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class BorrowYourBodyDrainPower : ManosabaTemporaryStrengthPower
{
    private const string SharedIconFile = "borrow_your_body_power.png";

    public override AbstractModel OriginModel => ModelDb.Card<BorrowYourBody>();
    protected override bool IsPositive => false;

    public override string CustomPackedIconPath => SharedIconFile.PowerImagePath();

    public override string CustomBigIconPath => SharedIconFile.PowerImagePath();

    public override string CustomBigBetaIconPath => SharedIconFile.PowerImagePath();
}
