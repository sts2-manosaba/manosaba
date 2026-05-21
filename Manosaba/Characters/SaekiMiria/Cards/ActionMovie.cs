using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class ActionMovie : MovieBase
{
    protected override IEnumerable<IHoverTip> MovieHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    protected override IEnumerable<DynamicVar> MovieVars => [new PowerVar<StrengthPower>(1m)];

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CommonActions.Apply<StrengthPower>(choiceContext, Owner.Creature, this, DynamicVars.Strength.BaseValue);

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
    }
}
