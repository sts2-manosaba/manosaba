using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public sealed class CounterViolence : ShitoAlisaCardModel
{
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.CombustIgnite];

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0, new BlockVar(8m, ValueProp.Move));
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
        HoverTipFactory.FromKeyword(ManosabaKeywords.CombustIgnite),
    ];

    public CounterViolence() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-LIGHTER.selectionScreenPrompt"), 1);
        CardModel? target = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            c => ShitoCombustOperations.HasActiveCombust(c),
            this)).FirstOrDefault();
        if (target == null)
            return;

        await ShitoCombustOperations.ApplyIgniteToCard(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}

