using BaseLib.Utils;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.KurobeNanoka.Relics;

[Pool(typeof(KurobeNanokaRelicPool))]
public sealed class Ribbon : LevelingPathCustomRelicModel
{
    private const decimal GuardStartingHp = 10m;
    private const decimal SummonMajokaGain = 20m;
    private const decimal InterceptMajokaGain = 10m;

    private bool _hasSummonedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override int MaxRelicLevel => 5;

    public int CombatStartBulletBonus => RelicLevel >= 4 ? 2 : RelicLevel >= 2 ? 1 : 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(GuardStartingHp),
        new PowerVar<MajokaPower>(SummonMajokaGain),
        new DynamicVar("InterceptMajoka", InterceptMajokaGain),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public override Task AfterObtained()
    {
        ApplyRelicLevelEffects();
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _hasSummonedThisCombat = false;
        return Task.CompletedTask;
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (target != Owner.Creature || amount <= 0m)
        {
            return target;
        }

        Creature? guard = Owner.PlayerCombatState?.GetPet<Jailer>();
        if (guard is { IsAlive: true })
        {
            return guard;
        }

        if (_hasSummonedThisCombat)
        {
            return target;
        }

        guard = SummonGuard();
        return guard is { IsAlive: true } ? guard : target;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        Creature? guard = Owner.PlayerCombatState?.GetPet<Jailer>();
        if (target != guard)
        {
            return;
        }

        if (result.BlockedDamage <= 0m && result.UnblockedDamage <= 0m)
        {
            return;
        }

        Flash();
        await GainDefenseMajoka();
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = cardSource;

        if (power is not CoveredPower || power.Owner != Owner.Creature || amount <= 0m)
        {
            return;
        }

        if (applier == null || applier == Owner.Creature)
        {
            return;
        }

        decimal currentMajoka = Owner.Creature.GetPowerAmount<MajokaPower>();
        if (currentMajoka >= 100m)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, 100m - currentMajoka, applier, null);
    }

    protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
    {
        ApplyRelicLevelEffects();
    }

    private void ApplyRelicLevelEffects()
    {
        DynamicVars.Summon.BaseValue = GuardStartingHp + (RelicLevel - 1) * 5m;
    }

    private Creature? SummonGuard()
    {
        Creature? guard = JailerCmd.Summon(Owner, DynamicVars.Summon.BaseValue);
        if (guard == null)
        {
            return null;
        }

        _hasSummonedThisCombat = true;
        Flash();

        _ = PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, null);

        return guard;
    }

    private Task<MajokaPower?> GainDefenseMajoka()
    {
        return PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["InterceptMajoka"].BaseValue, Owner.Creature, null);
    }
}
