using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Manosaba.Characters.KurobeNanoka.Cards;

public abstract class GunBase : PathCustomCardModel
{
    protected GunBase(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected bool TrySpendBulletsOnPlay()
    {
        if (!DynamicVars.TryGetValue("BulletCost", out var bulletCostVar))
        {
            return true;
        }

        MagicalGun? magicalGun = Owner.GetRelic<MagicalGun>();
        if (magicalGun == null)
        {
            return false;
        }

        int bulletCost = bulletCostVar.IntValue;
        if (!magicalGun.ConsumeBullets(bulletCost))
        {
            return false;
        }

        return true;
    }

    protected override bool IsPlayable =>
        Owner.GetRelic<MagicalGun>()?.HasEnoughBullets(DynamicVars["BulletCost"].IntValue) == true;
}
