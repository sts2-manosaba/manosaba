using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public class MovieViewing : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private static readonly IReadOnlyList<Func<Player, CombatState, MovieBase>> MovieFactories =
    [
        static (player, combatState) => combatState.CreateCard<HorrorMovie>(player),
        static (player, combatState) => combatState.CreateCard<ComedyMovie>(player),
        static (player, combatState) => combatState.CreateCard<FantasyMovie>(player),
        static (player, combatState) => combatState.CreateCard<ActionMovie>(player),
        static (player, combatState) => combatState.CreateCard<RomanticMovie>(player),
        static (player, combatState) => combatState.CreateCard<SpyMovie>(player),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<HorrorMovie>(),
        HoverTipFactory.FromCard<ComedyMovie>(),
        HoverTipFactory.FromCard<FantasyMovie>(),
        HoverTipFactory.FromCard<ActionMovie>(),
        HoverTipFactory.FromCard<RomanticMovie>(),
        HoverTipFactory.FromCard<SpyMovie>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public MovieViewing()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null)
            return;

        int count = DynamicVars.Cards.IntValue;
        if (count <= 0)
            return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        List<MovieBase> movies = new(count);
        for (int i = 0; i < count; i++)
        {
            Func<Player, CombatState, MovieBase>? factory = rng.NextItem(MovieFactories);
            if (factory == null)
            {
                continue;
            }

            movies.Add(factory(Owner, CombatState));
        }

        if (movies.Count == 0)
            return;

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            movies,
            PileType.Hand,
            addedByPlayer: true,
            CardPilePosition.Random);
        CardCmd.PreviewCardPileAdd(results);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
