using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(StatusCardPool))]
public sealed class Junk : PathCustomCardModel
{
    private new const int EnergyCost = -1;
    private const CardType CardTypeValue = CardType.Status;
    private new const CardRarity Rarity = CardRarity.Status;
    private const TargetType TargetTypeValue = TargetType.None;
    private new const bool ShouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public Junk()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }
}
