using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using manosaba.Characters.HikamiMeruru;
using manosaba.Characters.HoshoMago;
using manosaba.Characters.JogasakiNoah;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.NatsumeAnan;
using manosaba.Characters.NikaidoHiro;
using manosaba.Characters.SaekiMiria;
using manosaba.Characters.ShitoAlisa;
using manosaba.Characters.TachibanaSherry;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class MindOverload : PathCustomCardModel
{
    private const int requiredMajoka = 100;
    private const int cardsToPlay = 10;

    private const int energyCost = 5;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private static readonly IReadOnlyList<CardPoolModel> characterCardPools =
    [
        ModelDb.CardPool<HasumiLeiaCardPool>(),
        ModelDb.CardPool<HikamiMeruruCardPool>(),
        ModelDb.CardPool<HoshoMagoCardPool>(),
        ModelDb.CardPool<JogasakiNoahCardPool>(),
        ModelDb.CardPool<KurobeNanokaCardPool>(),
        ModelDb.CardPool<NatsumeAnanCardPool>(),
        ModelDb.CardPool<NikaidoHiroCardPool>(),
        ModelDb.CardPool<SaekiMiriaCardPool>(),
        ModelDb.CardPool<ShitoAlisaCardPool>(),
        ModelDb.CardPool<TachibanaSherryCardPool>(),
        ModelDb.CardPool<TonoHannaCardPool>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(cardsToPlay),
        new DynamicVar("MajokaRequired", requiredMajoka),
    ];

    public MindOverload()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (Owner?.Creature is not { } ownerCreature || Owner.Creature.Player is not { } ownerPlayer || Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        List<CardModel> pool = characterCardPools
            .SelectMany(cardPool => cardPool.GetUnlockedCards(ownerPlayer.UnlockState, ownerPlayer.RunState.CardMultiplayerConstraint))
            .Where(card => card.Type == CardType.Power)
            .Where(card => card.Rarity != CardRarity.Token)
            .Where(card => !card.CanonicalKeywords.Contains(ManosabaKeywords.Mahou))
            .Where(card => !MiriaConstants.IsIgnoredCard(card))
            .GroupBy(card => card.Id)
            .Select(group => group.First())
            .ToList();

        if (pool.Count == 0)
        {
            return;
        }

        Rng rng = ownerPlayer.RunState.Rng.CombatCardGeneration;
        List<CardModel> generatedCards = CardHelperService
            .GetAvailableCards(ownerPlayer, pool, Math.Min(cardsToPlay, pool.Count), rng)
            .ToList();

        if (generatedCards.Count == 0)
        {
            return;
        }

        bool first = true;
        foreach (CardModel generated in generatedCards)
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }

            Creature? target = GetAutoTarget(generated, ownerPlayer, ownerCreature, combatState);
            await CardCmd.AutoPlay(choiceContext, generated, target, AutoPlayType.Default, skipXCapture: true, first);
            first = false;
        }
    }

    private static Creature? GetAutoTarget(CardModel card, Player ownerPlayer, Creature ownerCreature, CombatState combatState)
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

    private static Creature? PickRandomAlly(Rng combatTargets, CombatState combatState, Creature ownerCreature)
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
