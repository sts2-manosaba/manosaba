using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Characters.KurobeNanoka.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public class WarningShot : GunBase
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.GunShot, CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StrengthLoss", 8m),
        new DynamicVar("BulletCost", 1m),
        new PowerVar<AccuratePower>(30m),
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    public WarningShot() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TrySpendBulletsOnPlay())
            return;

        if (base.CombatState is not { } combatState || base.Owner.Creature is not { } ownerCreature)
            return;

        NanokaHelper.PlayGunFireSfx();
        foreach (Creature hittableEnemy in combatState.HittableEnemies)
        {
            await CommonActions.Apply<WarningShotPower>(choiceContext, hittableEnemy, this, base.DynamicVars["StrengthLoss"].BaseValue);
        }

        await CommonActions.Apply<AccuratePower>(choiceContext, ownerCreature, this, DynamicVars["AccuratePower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["StrengthLoss"].UpgradeValueBy(3m);
    }
}
