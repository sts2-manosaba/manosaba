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
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class MindExpansionPower : PathCustomPowerModel
{
    private const int cardsOfferedPerTurn = 3;

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

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.Player is not { } ownerPlayer || Owner.CombatState == null || Amount <= 0m)
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

        for (int i = 0; i < Amount; i++)
        {
            List<CardModel> options = CardHelperService
                .GetAvailableCards(ownerPlayer, pool, Math.Min(cardsOfferedPerTurn, pool.Count), ownerPlayer.RunState.Rng.CombatCardGeneration)
                .ToList();

            if (options.Count == 0)
            {
                return;
            }

            CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, ownerPlayer);
            if (selected == null)
            {
                continue;
            }

            selected.AddKeyword(ManosabaKeywords.Shared);
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, addedByPlayer: true);
        }
    }
}
