using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using manosaba.Characters.SaekiMiria;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public abstract class MovieBase : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Token;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected virtual int CardsToDraw => 1;
    protected virtual IEnumerable<IHoverTip> MovieHoverTips => [];
    protected virtual IEnumerable<DynamicVar> MovieVars => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => MovieHoverTips;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(CardsToDraw), ..MovieVars];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected MovieBase()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, CardsToDraw, Owner);
        await OnMovieEffect(choiceContext, cardPlay);
    }

    protected abstract Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
