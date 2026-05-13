using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class GoodMorningSherry : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move)];

    public GoodMorningSherry() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target || Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, ownerCreature, 2m, ValueProp.Unpowered, ownerCreature);

        decimal bonus = ownerCreature.GetPowerAmount<SherryDetectiveRewardPower>() > 0 ? 5m : 0m;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
