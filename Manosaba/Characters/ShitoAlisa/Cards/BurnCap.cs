using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class BurnCap : ShitoAlisaCardModel
{
    private new const int EnergyCost = 1;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Common;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0, new PowerVar<BurnPower>(5m));
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<FireballSwarmPower>()];

    public BurnCap() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<BurnPower>(cardPlay.Target, DynamicVars["BurnPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FireballSwarmPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPower"].UpgradeValueBy(2m);
    }
}
