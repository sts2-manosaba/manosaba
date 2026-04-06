using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class Thrifty : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

    public Thrifty() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> statuses = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Status)
            .ToList();

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        decimal perCard = IsUpgraded ? 2m : 1m;
        foreach (CardModel c in statuses)
            await CardCmd.Exhaust(choiceContext, c);

        if (statuses.Count > 0)
            await CreatureCmd.GainBlock(Owner.Creature, statuses.Count * perCard, ValueProp.Move, cardPlay);
    }
}
