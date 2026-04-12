using BaseLib.Extensions;
using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class JudgeJogasakiNoah : PathCustomCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    private const int energyCost = 3;
    private const CardType cardTypeValue = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetTypeValue = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public JudgeJogasakiNoah() : base(energyCost, cardTypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = SelectOtherPlayerWithNikaidoPriority();
        if (targetPlayer == null || Owner.Creature.CombatState == null)
            return;

        List<CardModel> badDrawCards =
        [
            Owner.Creature.CombatState.CreateCard<BadDrawA>(targetPlayer),
            Owner.Creature.CombatState.CreateCard<BadDrawB>(targetPlayer),
            Owner.Creature.CombatState.CreateCard<BadDrawC>(targetPlayer)
        ];

        foreach (CardModel badDraw in badDrawCards)
        {
            await CardPileCmd.AddGeneratedCardToCombat(badDraw, PileType.Hand, addedByPlayer: true);
        }
    }

    private Player? SelectOtherPlayerWithNikaidoPriority()
    {
        List<Player> candidates = Owner.RunState.Players
            .Where(p => p != Owner)
            .Where(p => PileType.Hand.GetPile(p).Cards.Count <= 7)
            .ToList();

        if (candidates.Count == 0)
            return null;

        Player? nikaido = candidates.FirstOrDefault(IsNikaidoHiro);
        if (nikaido != null)
            return nikaido;

        return Owner.RunState.Rng.CombatTargets.NextItem(candidates);
    }

    private static bool IsNikaidoHiro(Player player)
    {
        string id = player.Character.Id.ToString().RemovePrefix().ToLowerInvariant();
        return id == "nikaido_hiro";
    }
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
