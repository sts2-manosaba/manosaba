using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Powers;

public class QuickWitTemporaryDexterityPower : ManosabaTemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<QuickWit>();
}
