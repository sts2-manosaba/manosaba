using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class InkPainting : PathCustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<BlackPaintOrb>(), HoverTipFactory.FromOrb<WhitePaintOrb>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

    public InkPainting() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState == null)
        {
            return;
        }

        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            List<CardModel> options =
            [
                Owner.Creature.CombatState.CreateCard<InkPaintingBlackPaintOrbToken>(Owner),
                Owner.Creature.CombatState.CreateCard<InkPaintingWhitePaintOrbToken>(Owner)
            ];

            CardModel? selectedToken = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                options,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
            if (selectedToken == null)
            {
                return;
            }

            await CardCmd.AutoPlay(choiceContext, selectedToken, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public class InkPaintingBlackPaintOrbToken : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Token;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<BlackPaintOrb>()];

    public InkPaintingBlackPaintOrbToken() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return OrbCmd.Channel(choiceContext, ModelDb.Orb<BlackPaintOrb>().ToMutable(), Owner);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public class InkPaintingWhitePaintOrbToken : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Token;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<WhitePaintOrb>()];

    public InkPaintingWhitePaintOrbToken() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return OrbCmd.Channel(choiceContext, ModelDb.Orb<WhitePaintOrb>().ToMutable(), Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
