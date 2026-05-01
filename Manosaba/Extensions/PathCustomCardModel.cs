using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Extensions
{
    public abstract class PathCustomCardModel : CustomCardModel
    {
        /// <summary>
        /// Vanilla <c>Hook.ShouldAddToDeck</c> calls every deck card in the run as a listener; uniqueness must use
        /// <paramref name="card"/>.Owner only, never the listener's <c>base.Owner</c> (would block other players in multiplayer).
        /// </summary>
        public override bool ShouldAddToDeck(CardModel card)
        {
            if (!card.CanonicalKeywords.Contains(ManosabaKeywords.Unique))
                return true;

            Player? owner = card.Owner;
            if (owner?.Deck?.Cards == null)
                return true;

            return !owner.Deck.Cards.Any(c => c.Id == card.Id && !ReferenceEquals(c, card));
        }
        public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardsImagePath();

        public PathCustomCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        /// <inheritdoc />
        /// <remarks>Uses the card type's declared <c>[Pool(typeof(...))]</c> so Common cards keep CommonCardPool visuals even when <see cref="CardModel.Pool"/> resolves to a character pool.</remarks>
        public override CardPoolModel VisualCardPool =>
            ManosabaDeclaredVisualCardPoolResolver.TryResolveDeclaredVisualPool(GetType()) ?? base.VisualCardPool;
    }
}
