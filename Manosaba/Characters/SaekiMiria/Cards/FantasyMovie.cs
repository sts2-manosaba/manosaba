using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class FantasyMovie : MovieBase
{
    protected override int CardsToDraw => 3;

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
