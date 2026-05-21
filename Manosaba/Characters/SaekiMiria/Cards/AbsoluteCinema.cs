using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.SaekiMiria.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class AbsoluteCinema : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<AbsoluteCinemaPower>(),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<AbsoluteCinemaPower>(2m)];

    public AbsoluteCinema()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => CommonActions.Apply<AbsoluteCinemaPower>(
            choiceContext,
            Owner.Creature,
            this,
            DynamicVars["AbsoluteCinemaPower"].BaseValue);

    protected override void OnUpgrade()
    {
        DynamicVars["AbsoluteCinemaPower"].UpgradeValueBy(1m);
    }
}
