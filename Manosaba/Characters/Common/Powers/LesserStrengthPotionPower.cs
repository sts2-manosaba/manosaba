using Manosaba.Characters.HikamiMeruru.Potions;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public class LesserStrengthPotionPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Potion<LesserStrengthPotion>();

    protected override bool IsPositive => true;
}

