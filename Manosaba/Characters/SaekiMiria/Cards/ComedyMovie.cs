using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class ComedyMovie : MovieBase
{
    protected override IEnumerable<IHoverTip> MovieHoverTips => [base.EnergyHoverTip];
    protected override IEnumerable<DynamicVar> MovieVars => [new EnergyVar(1)];

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
}
