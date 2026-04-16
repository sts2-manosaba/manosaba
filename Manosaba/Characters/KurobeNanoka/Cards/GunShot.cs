using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public class GunShot : GunBase
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.GunShot];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("BulletCost", 1m),
    ];

    public GunShot() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TrySpendBulletsOnPlay())
            return;

        var target = cardPlay.Target;
        if (target == null)
            return;
        decimal damage = DynamicVars.Damage.BaseValue;

        NanokaHelper.PlayGunFireSfx();
        await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

}
