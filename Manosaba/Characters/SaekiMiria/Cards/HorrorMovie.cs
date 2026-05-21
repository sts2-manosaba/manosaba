using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class HorrorMovie : MovieBase
{
    protected override IEnumerable<IHoverTip> MovieHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
    protected override IEnumerable<DynamicVar> MovieVars => [new PowerVar<MajokaPower>(10m)];

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CommonActions.Apply<MajokaPower>(choiceContext, Owner.Creature, this, DynamicVars["MajokaPower"].BaseValue);

    protected override void OnUpgrade()
    {
        DynamicVars["MajokaPower"].UpgradeValueBy(5m);
    }
}
