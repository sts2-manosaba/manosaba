using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class SkyIsland : PathCustomCardModel
{
    private const int EnergyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        HoverTipFactory.FromPower<SkyIslandPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(36m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(5m),
        new CalculatedVar("SkyIslandPower").WithMultiplier(GetMajokaFactor)
    ];

    public SkyIsland() : base(EnergyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (CombatState == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        decimal layersDecimal = ((CalculatedVar)DynamicVars["SkyIslandPower"]).Calculate(null);
        int grant = Math.Max(0, (int)layersDecimal);
        if (grant <= 0)
        {
            return;
        }

        foreach (Creature teammate in CombatState.GetTeammatesOf(Owner.Creature))
        {
            if (!teammate.IsAlive || !teammate.CanReceivePowers)
            {
                continue;
            }

            await PowerCmd.Apply<SkyIslandPower>(teammate, grant, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }

    /// <summary>Majoka factor: min(stacks / 100, 1); pairs with <see cref="CalculationExtraVar"/> for up to 5 layers.</summary>
    private static decimal GetMajokaFactor(CardModel card, Creature? _)
        => Math.Min(card.Owner.Creature.GetPowerAmount<MajokaPower>() / 100m, 1m);
}
