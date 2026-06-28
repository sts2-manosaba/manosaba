using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class NiceOutfit : ShoppingCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithGoldCost(
        new PowerVar<StrengthPower>(1m),
        new PowerVar<DexterityPower>(1m));

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public NiceOutfit() : base(energyCost, type, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await PayGoldCostAsync();
        await CommonActions.Apply<StrengthPower>(choiceContext, Owner.Creature, this, DynamicVars["StrengthPower"].BaseValue);
        await CommonActions.Apply<DexterityPower>(choiceContext, Owner.Creature, this, DynamicVars["DexterityPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DexterityPower"].UpgradeValueBy(1m);
    }
}
