using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class SnakeFire : ShitoAlisaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0, new PowerVar<BurnPower>(7m), new DynamicVar("CombustStacks", 3m));
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
        HoverTipFactory.FromKeyword(ManosabaKeywords.CombustIgnite),
    ];

    public SnakeFire() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CommonActions.Apply<BurnPower>(choiceContext, cardPlay.Target, this, DynamicVars["BurnPower"].BaseValue);

        int picks = IsUpgraded ? 2 : 1;
        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-SNAKE_FIRE.selectionScreenPrompt"), picks);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            c => ShitoCombustOperations.CanAttachCombust(c),
            this);
        foreach (CardModel card in selected.Distinct())
            ShitoCombustOperations.AttachCombust(card, (int)DynamicVars["CombustStacks"].BaseValue);
    }
}
