using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Extensions
{
    public abstract class PathCustomCardModel : CustomCardModel
    {
        public override bool ShouldAddToDeck(CardModel card)
        {
            if (!card.CanonicalKeywords.Contains(ManosabaKeywords.Unique))
                return true;

            if (base.Owner?.Deck?.Cards == null)
                return true;

            if (card.Id == this.Id && base.Owner.Deck.Cards.Any(c => c.Id == this.Id))
                return false;

            return true;
        }
        public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardsImagePath();

        public PathCustomCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }
    }
}
