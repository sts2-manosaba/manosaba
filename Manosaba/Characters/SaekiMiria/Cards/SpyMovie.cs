using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

public sealed class SpyMovie : MovieBase
{
    protected override IEnumerable<IHoverTip> MovieHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];
    protected override IEnumerable<DynamicVar> MovieVars => [new PowerVar<DexterityPower>(1m)];

    protected override Task OnMovieEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1m);
    }
}

