using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using SherryCharacter = manosaba.Characters.TachibanaSherry.TachibanaSherry;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class FlyingBroom : PathCustomCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SoarPower>(),
        HoverTipFactory.FromCard<PicnicTime>(),
    ];

    public FlyingBroom() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<HannaPuppetPower>(Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<HannaPuppetPower>(cardPlay.Target, 1m, Owner.Creature, this);

        if (cardPlay.Target.Player?.Character is SherryCharacter)
        {
            await GrantFreePicnicThisTurnAsync(Owner);
            await GrantFreePicnicThisTurnAsync(cardPlay.Target.Player!);
        }
    }

    private async Task GrantFreePicnicThisTurnAsync(Player player)
    {
        CombatState combatState = player.Creature.CombatState
            ?? throw new InvalidOperationException("GrantFreePicnicThisTurnAsync requires an active combat.");
        CardModel picnic = combatState.CreateCard(ModelDb.Card<PicnicTime>(), player);
        picnic.EnergyCost.SetThisTurnOrUntilPlayed(0);
        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(picnic, PileType.Hand, true, CardPilePosition.Top);
        if (LocalContext.IsMe(player.Creature))
        {
            CardCmd.PreviewCardPileAdd(result);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
