using BaseLib.Extensions;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class RomanticMovie : MovieBase
{
    protected override IEnumerable<IHoverTip> MovieHoverTips => [HoverTipFactory.FromPower<HealingPower>()];
    protected override IEnumerable<DynamicVar> MovieVars => [new PowerVar<HealingPower>(1m)];

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<HealingPower>(Owner.Creature, DynamicVars["HealingPower"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        DynamicVars["HealingPower"].UpgradeValueBy(1m);
    }
}
