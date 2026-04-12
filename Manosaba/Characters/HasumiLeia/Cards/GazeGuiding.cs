using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class GazeGuiding : PathCustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Power;
    private const CardRarity Rarity = CardRarity.Ancient;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromPower<MajokaPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GazeGuidingPower>(1m)];

    public GazeGuiding()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<GazeGuidingPower>(Owner.Creature, DynamicVars["GazeGuidingPower"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        base.AddKeyword(CardKeyword.Innate);
    }
}

