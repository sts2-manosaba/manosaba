using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Characters.KurobeNanoka.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class Aim : PathCustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<AccuratePower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AccuratePower>(60m),
    ];

    public Aim() : base(EnergyCost, Type, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return PowerCmd.Apply<AccuratePower>(Owner.Creature, DynamicVars["AccuratePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AccuratePower"].UpgradeValueBy(40m);
    }
}
