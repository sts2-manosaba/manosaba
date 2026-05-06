using BaseLib.Utils;
using manosaba.Characters.NatsumeAnan.Powers;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Kokoro : NatsumeKotodamaCardModel
{
    private const int BaseSuicideCards = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<KokoroPower>(1m),
        new CardsVar(BaseSuicideCards),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromPower<KokoroPower>(),
        HoverTipFactory.FromCard<KokoroSuicide>(),
    ];

    public Kokoro() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Unique];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await PowerCmd.Apply<KokoroPower>(Owner.Creature, DynamicVars["KokoroPower"].BaseValue, Owner.Creature, this);

        MegaCrit.Sts2.Core.Combat.CombatState? combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        List<CardModel> suicides = [];
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            CardModel suicide = combatState.CreateCard<KokoroSuicide>(Owner);
            CardCmd.ApplyKeyword(suicide, CardKeyword.Exhaust);
            suicides.Add(suicide);
        }

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            suicides,
            PileType.Draw,
            addedByPlayer: true,
            CardPilePosition.Random);
        CardCmd.PreviewCardPileAdd(results);

        _ = choiceContext;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
