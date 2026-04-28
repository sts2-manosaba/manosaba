using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class EmberSpark : ShitoAlisaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0, new BlockVar(5m, ValueProp.Move), new DynamicVar("CombustStacks", 3m));

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
        HoverTipFactory.FromKeyword(ManosabaKeywords.CombustIgnite),
    ];

    public EmberSpark() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        int picks = IsUpgraded ? 2 : 1;
        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-EMBER_SPARK.selectionScreenPrompt"), picks);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            c => ShitoCombustOperations.CanAttachCombust(c),
            this);
        foreach (CardModel card in selected.Distinct())
            ShitoCombustOperations.AttachCombust(card, (int)DynamicVars["CombustStacks"].BaseValue);
    }

    protected override void OnUpgrade()
    {
    }
}
