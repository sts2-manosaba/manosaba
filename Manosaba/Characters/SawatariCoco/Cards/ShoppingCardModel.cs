using BaseLib.Utils;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.SawatariCoco.Cards;

public abstract class ShoppingCardModel : PathCustomCardModel
{
    public const int GoldCost = 5;

    protected ShoppingCardModel(int energyCost, CardType type, TargetType targetType)
        : base(energyCost, type, CardRarity.Token, targetType, shouldShowInCardLibrary: true)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Shopping];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];

    protected static IEnumerable<DynamicVar> WithGoldCost(params DynamicVar[] vars)
    {
        yield return new GoldVar(GoldCost);
        foreach (DynamicVar variable in vars)
        {
            yield return variable;
        }
    }

    protected override bool IsPlayable => base.IsPlayable && Owner.Gold >= GoldCost;

    protected Task PayGoldCostAsync()
        => PlayerCmd.LoseGold(GoldCost, Owner, GoldLossType.Spent);
}
