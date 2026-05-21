using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class SkyIsland : PathCustomCardModel
{
    /// <summary>
    /// Same issue as <see cref="NanokaPuppet"/>: vanilla <see cref="CalculatedVar.Calculate"/> gates on <c>card.CombatState</c>, so hand previews showed 0 layers.
    /// </summary>
    private sealed class SkyIslandPowerCalculatedVar : CalculatedVar
    {
        public SkyIslandPowerCalculatedVar()
            : base("SkyIslandPower")
        {
            WithMultiplier(SkyIsland.GetMajokaFactor);
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            decimal mult = GetMajokaFactor(card, target);
            PreviewValue = card.DynamicVars.CalculationBase.BaseValue + card.DynamicVars.CalculationExtra.BaseValue * mult;
        }
    }

    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        HoverTipFactory.FromPower<SkyIslandPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(5m),
        new SkyIslandPowerCalculatedVar()
    ];

    public SkyIsland() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (CombatState == null)
        {
            return;
        }

        decimal layersDecimal = DynamicVars.CalculationBase.BaseValue
            + DynamicVars.CalculationExtra.BaseValue * GetMajokaFactor(this, null);
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

            await CommonActions.Apply<SkyIslandPower>(choiceContext, teammate, this, grant);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    /// <summary>Majoka factor: min(stacks / 100, 1); pairs with <see cref="CalculationExtraVar"/> for up to 5 layers.</summary>
    private static decimal GetMajokaFactor(CardModel card, Creature? _)
    {
        if (!CombatManager.Instance.IsInProgress || card.Owner?.Creature == null)
        {
            return 0m;
        }

        return Math.Min(card.Owner.Creature.GetPowerAmount<MajokaPower>() / 100m, 1m);
    }
}
