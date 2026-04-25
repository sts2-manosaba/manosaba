using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.KurobeNanoka.Cards;

public abstract class GunBase : PathCustomCardModel
{
    public bool SpentBulletsThisPlay { get; private set; }

    protected GunBase(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    private bool HasEnoughBulletsForPlay()
    {
        if (!DynamicVars.TryGetValue("BulletCost", out var bulletCostVar))
        {
            return true;
        }

        return Owner.GetRelic<MagicalGun>()?.HasEnoughBullets(bulletCostVar.IntValue) == true;
    }

    protected bool TrySpendBulletsOnPlay()
    {
        SpentBulletsThisPlay = false;

        if (!DynamicVars.TryGetValue("BulletCost", out var bulletCostVar))
        {
            SpentBulletsThisPlay = true;
            return true;
        }

        MagicalGun? magicalGun = Owner.GetRelic<MagicalGun>();
        if (magicalGun == null)
        {
            return false;
        }

        int bulletCost = bulletCostVar.IntValue;
        if (!magicalGun.ConsumeBullets(bulletCost))
        {
            return false;
        }

        SpentBulletsThisPlay = true;
        return true;
    }

    protected Task ExecuteGunAttack(PlayerChoiceContext choiceContext, Creature target, decimal damage)
    {
        return DamageCmd.Attack(damage)
            .WithAttackerFx(vfx: null, sfx: null)
            .WithHitFx(
                vfx: "vfx/vfx_attack_blunt",
                sfx: NanokaHelper.GUN_SHOT_SFX)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override bool IsPlayable => base.IsPlayable && HasEnoughBulletsForPlay();
}
