using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class Cotton : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<CluePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<CluePower>(2m)];

    public Cotton() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        CardModel? picked = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).FirstOrDefault();
        if (picked != null)
        {
            await CardCmd.Exhaust(choiceContext, picked);
        }

        await PowerCmd.Apply<CluePower>(ownerCreature, DynamicVars["CluePower"].BaseValue, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CluePower"].UpgradeValueBy(1m);
    }
}
