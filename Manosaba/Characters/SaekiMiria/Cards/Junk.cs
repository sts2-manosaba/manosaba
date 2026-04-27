using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(StatusCardPool))]
public sealed class Junk : PathCustomCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public Junk()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
