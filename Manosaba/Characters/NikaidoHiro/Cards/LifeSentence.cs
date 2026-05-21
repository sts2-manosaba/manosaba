using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.NikaidoHiro.Cards;

[Pool(typeof(NikaidoHiroCardPool))]
public sealed class LifeSentence : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<LifeSentencePower>(2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LifeSentencePower>()];

    public LifeSentence() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.Apply<LifeSentencePower>(choiceContext, Owner.Creature, this, DynamicVars["LifeSentencePower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LifeSentencePower"].UpgradeValueBy(1m);
    }
}
