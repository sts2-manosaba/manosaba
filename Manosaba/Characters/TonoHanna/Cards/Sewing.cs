using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class Sewing : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    public Sewing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        static bool IsUpgradablePuppet(CardModel c) =>
            c.Tags.Contains(ManosabaCardTags.Puppet) && c.IsUpgradable;

        List<CardModel> upgradablePuppets = PileType.Hand.GetPile(Owner).Cards
            .Where(IsUpgradablePuppet)
            .ToList();
        if (upgradablePuppets.Count == 0)
            return;

        if (IsUpgraded)
        {
            foreach (CardModel c in upgradablePuppets)
                CardCmd.Upgrade(c);
            return;
        }

        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-SEWING.selectionScreenPrompt"), 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, IsUpgradablePuppet, this);
        foreach (CardModel c in picked)
            CardCmd.Upgrade(c);
    }
}
