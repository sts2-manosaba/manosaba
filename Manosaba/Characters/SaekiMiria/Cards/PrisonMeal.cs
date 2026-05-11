using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class PrisonMeal : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HealingPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<MajokaPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<HealingPower>(3m),
        new PowerVar<PoisonPower>(3m),
        new PowerVar<MajokaPower>(5m),
    ];

    public PrisonMeal()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PowerCmd.Apply<HealingPower>(Owner.Creature, DynamicVars["HealingPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PoisonPower>(Owner.Creature, DynamicVars.Poison.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HealingPower"].UpgradeValueBy(2m);
        DynamicVars["PoisonPower"].UpgradeValueBy(2m);
    }

/*    | Regen \ Poison |   1 |   2 |   3 |  4 |   5 |   6 |   7 |   8 |   9 |  10 |
| -------------: | --: | --: | --: | -: | --: | --: | --: | --: | --: | --: |
        |              1 |   0 |  -2 |  -5 | -9 | -14 | -20 | -27 | -35 | -44 | -54 |
        |              2 |  +2 |   0 |  -3 | -7 | -12 | -18 | -25 | -33 | -42 | -52 |
        |              3 |  +3 |  +1 |  -2 | -6 | -11 | -17 | -24 | -32 | -41 | -51 |
        |              4 |  +6 |  +4 |  +1 | -3 |  -8 | -14 | -21 | -29 | -38 | -48 |
        |              5 |  +7 |  +5 |  +2 | -2 |  -7 | -13 | -20 | -28 | -37 | -47 |
        |              6 |  +9 |  +7 |  +4 |  0 |  -5 | -11 | -18 | -26 | -35 | -45 |
        |              7 | +10 |  +8 |  +5 | +1 |  -4 | -10 | -17 | -25 | -34 | -44 |
        |              8 | +14 | +12 |  +9 | +5 |   0 |  -6 | -13 | -21 | -30 | -40 |
        |              9 | +15 | +13 | +10 | +6 |  +1 |  -5 | -12 | -20 | -29 | -39 |
        |             10 | +17 | +15 | +12 | +8 |  +3 |  -3 | -10 | -18 | -27 | -37 |
        */
        }
