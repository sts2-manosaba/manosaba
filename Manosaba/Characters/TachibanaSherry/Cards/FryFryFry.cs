using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class FryFryFry : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<CluePower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(2m)];

    public FryFryFry() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        decimal healAmount = DynamicVars.Heal.BaseValue + ownerCreature.GetPowerAmount<CluePower>();
        if (healAmount > 0m)
        {
            await CreatureCmd.Heal(ownerCreature, healAmount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(3m);
    }
}
