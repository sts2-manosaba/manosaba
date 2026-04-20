using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class SmallPaper : MovieBase
{

    protected override async Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        CardModel generated = CombatState.RunState.Rng.CombatCardGeneration.NextInt(10) == 0
            ? CombatState.CreateCard<TheWayOut>(Owner)
            : CombatState.CreateCard<Junk>(Owner);

        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, addedByPlayer: true);
        CardCmd.PreviewCardPileAdd(result, 1.2f, CardPreviewStyle.HorizontalLayout);
    }
}
