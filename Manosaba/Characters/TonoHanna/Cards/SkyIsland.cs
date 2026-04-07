using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class SkyIsland : PathCustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SkyIslandBaseDamage", 8m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Damage").WithMultiplier(GetFinalDamageFromMajoka)
    ];

    protected override bool ShouldGlowGoldInternal => Owner.Creature.GetPowerAmount<MajokaPower>() >= 100;

    public SkyIsland() : base(0, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = ResolveEnergyXValue();
        if (hits <= 0 || CombatState == null)
            return;

        decimal hitDamage = ((CalculatedVar)DynamicVars["Damage"]).Calculate(null);
        await DamageCmd.Attack(hitDamage)
            .WithHitCount(hits)
            .Unpowered()
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SkyIslandBaseDamage"].UpgradeValueBy(2m);
    }

    private static decimal GetFinalDamageFromMajoka(CardModel card, Creature? _)
    {
        decimal baseDamage = card.DynamicVars["SkyIslandBaseDamage"].BaseValue;
        if (baseDamage <= 0m)
            return 0m;

        decimal majoka = card.Owner.Creature.GetPowerAmount<MajokaPower>();
        decimal mahouScale = Math.Min(majoka / 100m, 1m);
        decimal multiplicativeBonus = 1m + majoka / 100m;
        decimal scaledDamage = Math.Floor(baseDamage * mahouScale);
        decimal additiveBonus = Math.Floor(majoka / 25m);
        return Math.Floor((scaledDamage + additiveBonus) * multiplicativeBonus);
    }
}
