using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Manosaba.Extensions
{
    public abstract class PathCustomCardModel : CustomCardModel
    {
        public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardsImagePath();

        public PathCustomCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }
    }
}
