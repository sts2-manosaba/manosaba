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

/// <summary>SIX SEVEN：隨機獲得力量、疑點、敏捷、覆甲或格檔；升級移除消耗。</summary>
[Pool(typeof(TachibanaSherryCardPool))]
public sealed class SixSeven : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<CluePower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1m),
        new PowerVar<CluePower>(1m),
        new PowerVar<DexterityPower>(1m),
        new PowerVar<PlatingPower>(7m),
        new BlockVar(67m, ValueProp.Move),
    ];

    public SixSeven() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        int roll = Owner.RunState.Rng.CombatCardSelection.NextInt(100);
        if (roll < 67)
        {
            await PowerCmd.Apply<StrengthPower>(ownerCreature, DynamicVars["StrengthPower"].BaseValue, ownerCreature, this);
        }
        else if (roll < 80)
        {
            await PowerCmd.Apply<CluePower>(ownerCreature, DynamicVars["CluePower"].BaseValue, ownerCreature, this);
        }
        else if (roll < 93)
        {
            await PowerCmd.Apply<DexterityPower>(ownerCreature, DynamicVars["DexterityPower"].BaseValue, ownerCreature, this);
        }
        else if (roll < 99)
        {
            await PowerCmd.Apply<PlatingPower>(ownerCreature, DynamicVars["PlatingPower"].BaseValue, ownerCreature, this);
        }
        else
        {
            await CreatureCmd.GainBlock(ownerCreature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
