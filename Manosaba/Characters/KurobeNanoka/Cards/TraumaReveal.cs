using HoshoMagoCharacter = manosaba.Characters.HoshoMago.HoshoMago;
using KurobeNanokaCharacter = manosaba.Characters.KurobeNanoka.KurobeNanoka;
using NikaidoHiroCharacter = manosaba.Characters.NikaidoHiro.NikaidoHiro;
using TonoHannaCharacter = manosaba.Characters.TonoHanna.TonoHanna;
using HasumiLeiaCharacter = manosaba.Characters.HasumiLeia.HasumiLeia;
using HikamiMeruruCharacter = manosaba.Characters.HikamiMeruru.HikamiMeruru;
using JogasakiNoahCharacter = manosaba.Characters.JogasakiNoah.JogasakiNoah;
using SaekiMiriaCharacter = manosaba.Characters.SaekiMiria.SaekiMiria;
using Manosaba.Characters.HasumiLeia.Cards;
using Manosaba.Characters.HikamiMeruru.Cards;
using Manosaba.Characters.HoshoMago.Cards;
using Manosaba.Characters.JogasakiNoah.Cards;
using Manosaba.Characters.NikaidoHiro.Cards;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.TonoHanna.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Extensions;
using manosaba.Characters.KurobeNanoka;
using BaseLib.Utils;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class TraumaReveal : PathCustomCardModel
{
    private static readonly Dictionary<string, Type> TraumaTypesByCharacterId = new()
    {
        { KurobeNanokaCharacter.CharacterId, typeof(TraumaKurobeNanoka) },
        { NikaidoHiroCharacter.CharacterId, typeof(TraumaNikaidoHiro) },
        { SaekiMiriaCharacter.CharacterId, typeof(TraumaSaekiMiria) },
        { HikamiMeruruCharacter.CharacterId, typeof(TraumaHikamiMeruru) },
        { JogasakiNoahCharacter.CharacterId, typeof(TraumaJogasakiNoah) },
        { HasumiLeiaCharacter.CharacterId, typeof(TraumaHasumiLeia) },
        { TonoHannaCharacter.CharacterId, typeof(TraumaTonoHanna) },
        { HoshoMagoCharacter.CharacterId, typeof(TraumaHoshoMago) },
        // { SakurabaEma.CharacterId, typeof(TraumaSakurabaEma) },
        // { NatsumeAnan.CharacterId, typeof(TraumaNatsumeAnan) },
        // { ShitoArisa.CharacterId, typeof(TraumaShitoArisa) },
        // { SawatariCoco.CharacterId, typeof(TraumaSawatariCoco) },
    };

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.AllAllies;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<TraumaKurobeNanoka>()];

    public TraumaReveal()
        : base(energyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        IEnumerable<Creature> players = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer);

        foreach (Creature creature in players)
        {
            Player? player = creature.Player;
            if (player == null)
            {
                continue;
            }

            Type? traumaType = GetTraumaType(player);
            if (traumaType == null)
            {
                continue;
            }

            CardModel? traumaCard = FindTraumaCard(player, traumaType);
            if (traumaCard == null)
            {
                continue;
            }

            await CardCmd.AutoPlay(choiceContext, traumaCard, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }


    private static Type? GetTraumaType(Player player)
    {
        string characterId = player.Character.Id.ToString().RemovePrefix().ToLowerInvariant();
        return TraumaTypesByCharacterId.GetValueOrDefault(characterId);
    }

    private static CardModel? FindTraumaCard(Player player, Type traumaType)
    {
        foreach (PileType pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            CardModel? traumaCard = pileType.GetPile(player).Cards.FirstOrDefault(card => card.GetType() == traumaType);
            if (traumaCard != null)
            {
                return traumaCard;
            }
        }

        return null;
    }
}
