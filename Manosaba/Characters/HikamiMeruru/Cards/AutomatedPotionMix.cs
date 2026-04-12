using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class AutomatedPotionMix : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType TypeValue = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetTypeValue = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<AutomatedPotionMixPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<AutomatedPotionMixPower>(1)];

    public AutomatedPotionMix() : base(energyCost, TypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AutomatedPotionMixPower>(Owner.Creature, DynamicVars["AutomatedPotionMixPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AutomatedPotionMixPower"].UpgradeValueBy(1m);
    }
}
