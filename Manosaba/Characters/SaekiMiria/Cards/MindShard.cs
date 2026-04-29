using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using manosaba.Characters.HikamiMeruru;
using manosaba.Characters.HoshoMago;
using manosaba.Characters.JogasakiNoah;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.NatsumeAnan;
using manosaba.Characters.NikaidoHiro;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.SaekiMiria.Helper;
using manosaba.Characters.ShitoAlisa;
using manosaba.Characters.TachibanaSherry;
using manosaba.Characters.TonoHanna;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class MindShard : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const int cardsOffered = 3;

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

    public MindShard()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<CardPoolModel> pools = characterCardPools.ToList();
        if (pools.Count > 1)
        {
            pools.Remove(Owner.Character.CardPool);
        }

        IEnumerable<CardModel> cards = pools
            .SelectMany(cardPool => cardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(card => card.Type == CardType.Attack)
            .Where(card => !MiriaConstants.IsIgnoredCard(card));

        List<CardModel> options = CardFactory
            .GetDistinctForCombat(Owner, cards, cardsOffered, Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        if (IsUpgraded)
        {
            foreach (CardModel item in options)
            {
                CardCmd.Upgrade(item);
            }
        }

        if (options.Count == 0)
        {
            return;
        }

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: true);
        if (selected == null)
        {
            return;
        }

        selected.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, addedByPlayer: true);
    }
}
