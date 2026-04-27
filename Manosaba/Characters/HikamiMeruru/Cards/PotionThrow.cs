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
public class PotionThrow : PathCustomCardModel
{
    private const int EnergyCostValue = 1;
    private const CardType TypeValue = CardType.Power;
    private const CardRarity RarityValue = CardRarity.Rare;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PotionThrowPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotionThrowPower>(1)];

    public PotionThrow() : base(EnergyCostValue, TypeValue, RarityValue, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PotionThrowPower>(Owner.Creature, DynamicVars["PotionThrowPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PotionThrowPower"].UpgradeValueBy(1);
    }
}
