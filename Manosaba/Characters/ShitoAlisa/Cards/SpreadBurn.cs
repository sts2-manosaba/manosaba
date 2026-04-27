using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class SpreadBurn : ShitoAlisaCardModel
{
    private new const int EnergyCost = 2;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0, new DynamicVar("FireballGainPerSpread", 1m));
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<FireballSwarmPower>()];

    public SpreadBurn() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        decimal sourceBurn = cardPlay.Target.GetPowerAmount<BurnPower>();
        if (sourceBurn <= 0m)
            return;

        int spreadCount = 0;
        foreach (Creature enemy in CombatState.GetOpponentsOf(Owner.Creature).ToList())
        {
            if (!enemy.IsAlive || !enemy.IsHittable || enemy == cardPlay.Target)
                continue;
            await PowerCmd.Apply<BurnPower>(enemy, sourceBurn, Owner.Creature, this);
            spreadCount++;
        }
        if (spreadCount <= 0)
            return;
        decimal gainPerSpread = DynamicVars["FireballGainPerSpread"].BaseValue;
        await PowerCmd.Apply<FireballSwarmPower>(Owner.Creature, gainPerSpread * spreadCount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FireballGainPerSpread"].UpgradeValueBy(1m);
    }
}
