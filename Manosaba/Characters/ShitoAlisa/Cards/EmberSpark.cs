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

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class EmberSpark : ShitoAlisaCardModel
{
    private new const int EnergyCost = 0;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Basic;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0, new DynamicVar("CombustStacks", 3m));

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ManosabaKeywords.Combust)];

    public EmberSpark() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
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
