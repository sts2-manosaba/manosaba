using MegaCrit.Sts2.Core.Entities.Cards;

namespace Manosaba.Characters.KurobeNanoka.Cards;

// Back-compat for earlier Nanoka gun cards; prefer inheriting GunBase for new gun cards.
public abstract class GunShotBase : GunBase
{
    protected GunShotBase(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}

