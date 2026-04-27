using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class Reload : PathCustomCardModel
{
    private new const int EnergyCost = 0;
    private new const CardType Type = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    public Reload() : base(EnergyCost, Type, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        int cardsToDraw = handCards.Count;

        if (cardsToDraw == 0)
            return;

        await CardCmd.Discard(choiceContext, handCards);
        await CardPileCmd.Draw(choiceContext, cardsToDraw, Owner);
    }

    protected override void OnUpgrade()
    {
        base.AddKeyword(CardKeyword.Retain);
    }
}
