using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class LucidDream : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public LucidDream()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (Owner?.Creature is not { } ownerCreature || Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        CardPile discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-LUCID_DREAM.selectionScreenPrompt"), 1)
        {
            PretendCardsCanBePlayed = true,
        };

        CardModel? selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            discardPile,
            Owner,
            prefs)).FirstOrDefault();

        if (selected == null)
        {
            return;
        }

        Creature? target = GetAutoTarget(selected, Owner, ownerCreature, combatState);
        await CardCmd.AutoPlay(choiceContext, selected, target, AutoPlayType.Default, skipXCapture: true);
    }

    private static Creature? GetAutoTarget(CardModel card, Player ownerPlayer, Creature ownerCreature, ICombatState combatState)
    {
        Rng combatTargets = ownerPlayer.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => PickRandomAlly(combatTargets, combatState, ownerCreature),
            TargetType.AnyPlayer => ownerCreature,
            _ => null,
        };
    }

    private static Creature? PickRandomAlly(Rng combatTargets, ICombatState combatState, Creature ownerCreature)
    {
        List<Creature> allies = combatState.Allies
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != ownerCreature)
            .ToList();

        return allies.Count == 0 ? null : combatTargets.NextItem(allies);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
