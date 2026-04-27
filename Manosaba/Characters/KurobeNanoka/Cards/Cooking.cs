using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class Cooking : PathCustomCardModel
{
    public override bool GainsBlock => true;

    private new const int EnergyCost = 1;
    private new const CardType Type = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<AccurateCookie>(),
        HoverTipFactory.FromPower<Manosaba.Characters.KurobeNanoka.Powers.AccuratePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
    ];

    public Cooking() : base(EnergyCost, Type, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        List<CardModel> statusCards = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card.Type == CardType.Status)
            .ToList();

        foreach (CardModel statusCard in statusCards)
        {
            await CardCmd.TransformTo<AccurateCookie>(statusCard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
