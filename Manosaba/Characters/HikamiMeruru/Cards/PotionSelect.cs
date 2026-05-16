using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class PotionSelect : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPotion<LesserPainKillerPotion>(),
        HoverTipFactory.FromPotion<LesserBlockPotion>(),
        HoverTipFactory.FromPotion<LesserFlexPotion>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Repeat", 2m)];

    public PotionSelect() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState == null)
        {
            return;
        }

        for (int i = 0; i < DynamicVars["Repeat"].IntValue; i++)
        {
            List<CardModel> options =
            [
                Owner.Creature.CombatState.CreateCard<PotionSelectLesserPainKillerPotionToken>(Owner),
                Owner.Creature.CombatState.CreateCard<PotionSelectLesserBlockPotionToken>(Owner),
                Owner.Creature.CombatState.CreateCard<PotionSelectLesserFlexPotionToken>(Owner)
            ];

            CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
            if (selected == null)
            {
                return;
            }

            await CardCmd.AutoPlay(choiceContext, selected, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Repeat"].UpgradeValueBy(1m);
    }
}

public abstract class PotionSelectPotionToken<TPotion> : PathCustomCardModel
    where TPotion : PotionModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<TPotion>()];

    protected PotionSelectPotionToken() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PotionCmd.TryToProcure<TPotion>(Owner);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class PotionSelectLesserPainKillerPotionToken : PotionSelectPotionToken<LesserPainKillerPotion>
{
}

[Pool(typeof(TokenCardPool))]
public sealed class PotionSelectLesserBlockPotionToken : PotionSelectPotionToken<LesserBlockPotion>
{
}

[Pool(typeof(TokenCardPool))]
public sealed class PotionSelectLesserFlexPotionToken : PotionSelectPotionToken<LesserFlexPotion>
{
}
