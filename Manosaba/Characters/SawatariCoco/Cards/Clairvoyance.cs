using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class Clairvoyance : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal MajokaPerStack = 20m;
    private const decimal MaxStacks = 5m;

    private const string StacksVarName = nameof(ClairvoyanceEffectPower);

    private sealed class ClairvoyanceStacksVar : CalculatedVar
    {
        public ClairvoyanceStacksVar()
            : base(StacksVarName)
        {
            WithMultiplier(GetStacks);
        }

        private static decimal GetStacks(CardModel card, Creature? _)
        {
            decimal majoka = card.Owner?.Creature?.GetPowerAmount<MajokaPower>() ?? 0m;
            return Math.Min(Math.Floor(majoka / MajokaPerStack), MaxStacks);
        }

        public new decimal Calculate(Creature? target)
        {
            if (_owner is not CardModel card)
            {
                return 0m;
            }

            return GetBaseVar().BaseValue + GetExtraVar().BaseValue * GetStacks(card, target);
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            PreviewValue = Calculate(target);
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        HoverTipFactory.FromPower<ClairvoyanceEffectPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new ClairvoyanceStacksVar(),
    ];

    public Clairvoyance() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (CombatState is not { } combatState || DynamicVars[StacksVarName] is not ClairvoyanceStacksVar stacksVar)
        {
            return;
        }

        int stacks = (int)stacksVar.Calculate(null);
        if (stacks <= 0)
        {
            return;
        }

        foreach (Player player in combatState.Players)
        {
            if (player.Creature is not { IsAlive: true } creature)
            {
                continue;
            }

            await CommonActions.Apply<ClairvoyanceEffectPower>(choiceContext, creature, this, stacks);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
