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
public class FireOnFire : ShitoAlisaCardModel
{
    private new const int EnergyCost = 1;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0, new PowerVar<BurnPower>(9m));
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
        HoverTipFactory.FromKeyword(ManosabaKeywords.CombustIgnite),
    ];

    public FireOnFire() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        if (!cardPlay.Target.HasPower<BurnPower>())
            return;

        await PowerCmd.Apply<BurnPower>(cardPlay.Target, DynamicVars["BurnPower"].BaseValue, Owner.Creature, this);

        var prefs = new CardSelectorPrefs(new LocString("cards", "MANOSABA-LIGHTER.selectionScreenPrompt"), 1);
        CardModel? handCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            c => ShitoCombustOperations.HasActiveCombust(c),
            this)).FirstOrDefault();
        if (handCard == null)
            return;

        await ShitoCombustOperations.ApplyIgniteToCard(choiceContext, handCard, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPower"].UpgradeValueBy(3m);
    }
}
