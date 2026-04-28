using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class RefuseMajo : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override bool IsPlayable => base.IsPlayable && Owner.Creature.GetPowerAmount<MajokaPower>() < 100;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        HoverTipFactory.FromCard<Boulders>(),
    ];

    public RefuseMajo() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (CardModel c in hand)
        {
            if (c == cardPlay.Card)
                continue;

            CardPileAddResult? r = await CardCmd.TransformTo<Boulders>(c);
            if (r is { success: true, cardAdded: { } added })
            {
                CardCmd.ApplyKeyword(added, CardKeyword.Exhaust);
                added.EnergyCost.SetThisTurnOrUntilPlayed(0);
            }
        }

        decimal majoka = Owner.Creature.GetPowerAmount<MajokaPower>();
        if (majoka > 0)
            await PowerCmd.Apply<MajokaPower>(Owner.Creature, -majoka, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
