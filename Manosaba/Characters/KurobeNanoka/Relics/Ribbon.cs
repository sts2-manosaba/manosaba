using BaseLib.Utils;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
    private const decimal GuardMaxHp = 30m;
    private const decimal GuardSafeTurnMinHealAmount = 5m;
    private const decimal GuardSafeTurnMaxHealAmount = 10m;
    private const decimal SummonMajokaGain = 30m;
    private const decimal InterceptMajokaGain = 10m;

    private bool _hasSummonedThisCombat;
    private bool _redirectedDamageToGuard;
    private bool _isTrackingEnemyTurn;
    private bool _guardTookDamageThisEnemyTurn;
    private bool _guardSummonedThisEnemyTurn;

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
        _redirectedDamageToGuard = false;
        _isTrackingEnemyTurn = false;
        _guardTookDamageThisEnemyTurn = false;
        _guardSummonedThisEnemyTurn = false;
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        _ = combatState;

        if (side == CombatSide.Enemy)
        {
            _isTrackingEnemyTurn = true;
            _guardTookDamageThisEnemyTurn = false;
            _guardSummonedThisEnemyTurn = false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player != Owner)
        {
            return;
        }

        if (!_isTrackingEnemyTurn)
        {
            return;
        }

        _isTrackingEnemyTurn = false;

        if (_guardSummonedThisEnemyTurn)
        {
            _guardSummonedThisEnemyTurn = false;
            return;
        }

        if (_hasSummonedThisCombat && !_guardTookDamageThisEnemyTurn)
        {
            Flash();
            await JailerCmd.Heal(choiceContext, Owner, GetGuardSafeTurnHealAmount(), DynamicVars.Summon.BaseValue);
        }
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
            _redirectedDamageToGuard = true;
            return guard;
        }

        if (_hasSummonedThisCombat)
        {
            return target;
        }

        guard = SummonGuard();
        if (guard is { IsAlive: true })
        {
            _redirectedDamageToGuard = true;
            return guard;
        }

        return target;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!_redirectedDamageToGuard)
        {
            return;
        }

        if (_isTrackingEnemyTurn && result.UnblockedDamage > 0m)
        {
            _guardTookDamageThisEnemyTurn = true;
        }

        if (result.BlockedDamage <= 0m && result.UnblockedDamage <= 0m)
        {
            _redirectedDamageToGuard = false;
            return;
        }

        _redirectedDamageToGuard = false;
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
        decimal hpPerLevel = (GuardMaxHp - GuardStartingHp) / (MaxRelicLevel - 1);
        DynamicVars.Summon.BaseValue = GuardStartingHp + (RelicLevel - 1) * hpPerLevel;
    }

    private Creature? SummonGuard()
    {
        Creature? guard = JailerCmd.Summon(Owner, DynamicVars.Summon.BaseValue);
        if (guard == null)
        {
            return null;
        }

        _hasSummonedThisCombat = true;
        _guardSummonedThisEnemyTurn = _isTrackingEnemyTurn;
        Flash();

        _ = PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, null);

        return guard;
    }

    private Task<MajokaPower?> GainDefenseMajoka()
    {
        return PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["InterceptMajoka"].BaseValue, Owner.Creature, null);
    }

    private decimal GetGuardSafeTurnHealAmount()
    {
        decimal relicLevelRatio = MaxRelicLevel <= 1 ? 1m : (RelicLevel - 1m) / (MaxRelicLevel - 1m);
        relicLevelRatio = Math.Clamp(relicLevelRatio, 0m, 1m);
        return GuardSafeTurnMinHealAmount + (GuardSafeTurnMaxHealAmount - GuardSafeTurnMinHealAmount) * relicLevelRatio;
    }
}
