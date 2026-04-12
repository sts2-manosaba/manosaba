using System;
using System.Reflection;
using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class Aspiration : PathCustomCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    private const int energyCost = 2;
    private const CardType cardTypeValue = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetTypeValue = TargetType.AllAllies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    public Aspiration()
        : base(energyCost, cardTypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        decimal blockRemaining = GetCreatureBlock(Owner.Creature);
        if (blockRemaining <= 0m)
            return;

        IEnumerable<Creature> teammates = CombatState
            .GetTeammatesOf(Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != Owner.Creature);

        foreach (Creature teammate in teammates)
        {
            await CreatureCmd.GainBlock(teammate, new BlockVar(blockRemaining, ValueProp.Move), cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    private static decimal GetCreatureBlock(Creature creature)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = creature.GetType();

        foreach (string name in new[] { "Block", "CurrentBlock", "BlockAmount" })
        {
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null)
            {
                object? value = prop.GetValue(creature);
                return value == null ? 0m : Convert.ToDecimal(value);
            }

            FieldInfo? field = type.GetField(name, flags);
            if (field != null)
            {
                object? value = field.GetValue(creature);
                return value == null ? 0m : Convert.ToDecimal(value);
            }
        }

        return 0m;
    }
}

