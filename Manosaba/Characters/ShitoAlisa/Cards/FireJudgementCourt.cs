using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public sealed class FireJudgementCourt : ShitoAlisaCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0,
            new PowerVar<BurnPower>(10m),
            new CalculationBaseVar(0m),
            new CalculationExtraVar(3m),
            new CalculatedVar("BurnTriggerCount").WithMultiplier(GetMajokaFactor));

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<MajokaPower>()];

    public FireJudgementCourt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState? state = CombatState;
        if (state == null)
            return;

        List<Creature> opponents = state.GetOpponentsOf(Owner.Creature).ToList();
        decimal burnAmount = DynamicVars["BurnPower"].BaseValue;
        foreach (Creature enemy in opponents)
        {
            if (!enemy.IsAlive || !enemy.IsHittable)
                continue;
            await PowerCmd.Apply<BurnPower>(enemy, burnAmount, Owner.Creature, this);
        }

        foreach (Creature enemy in opponents)
        {
            if (!enemy.IsAlive)
                continue;

            BurnPower? burn = enemy.GetPower<BurnPower>();
            if (burn == null)
                continue;

            int triggers = (int)Math.Clamp(Math.Floor(((CalculatedVar)DynamicVars["BurnTriggerCount"]).Calculate(null)), 0m, 3m);
            for (int i = 0; i < triggers; i++)
                await burn.AfterSideTurnStart(enemy.Side, state);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }

    private static decimal GetMajokaFactor(CardModel card, Creature? _) =>
        Math.Min(card.Owner.Creature.GetPowerAmount<MajokaPower>() / 100m, 1m);

}

