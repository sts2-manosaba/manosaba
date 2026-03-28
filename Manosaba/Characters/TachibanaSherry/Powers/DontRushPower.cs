using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TachibanaSherry.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Powers
{
    public class DontRushPower : ManosabaTemporaryStrengthPower
    {
        public override AbstractModel OriginModel => ModelDb.Card<DontRush>();
        protected override bool IsPositive => false;
    }
}
