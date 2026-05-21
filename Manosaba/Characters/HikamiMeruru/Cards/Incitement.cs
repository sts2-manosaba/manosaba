using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class Incitement : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyPlayer;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        base.EnergyHoverTip,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<MajokaPower>(10),
        new CardsVar(2),
        new EnergyVar(1),
    ];

    public Incitement() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } owner || cardPlay.Target?.Player is not { } targetPlayer)
        {
            return;
        }

        if (targetPlayer == owner)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, targetPlayer);
            return;
        }

await CommonActions.Apply<MajokaPower>(choiceContext,  targetPlayer.Creature, this, DynamicVars["MajokaPower"].BaseValue);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, targetPlayer);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MajokaPower"].UpgradeValueBy(5);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
