using BaseLib.Utils;
using manosaba.Characters.NatsumeAnan.Powers;
using Manosaba.Characters.Common.Cards;
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

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<KokoroPower>()];

    public Kokoro() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await PowerCmd.Apply<KokoroPower>(Owner.Creature, DynamicVars["KokoroPower"].BaseValue, Owner.Creature, this);

        List<CardModel> suicides = [];
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            suicides.Add(CombatState.CreateCard<Suicide>(Owner));
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
        DynamicVars.Cards.UpgradeValueBy(3m);
    }
}
