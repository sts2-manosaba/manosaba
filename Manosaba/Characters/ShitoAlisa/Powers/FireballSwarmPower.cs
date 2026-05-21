using System.Collections.Generic;
using Manosaba.Characters.ShitoAlisa.Visuals;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Powers;

/// <summary>Amount = orbiting fireball stacks (max <see cref="FireballOrbitRing.MaxOrbs"/>). After block, each orb can absorb up to <see cref="DamageAbsorbedPerOrb"/> HP loss.</summary>
public sealed class FireballSwarmPower : PathCustomPowerModel
{
    public const int DamageAbsorbedPerOrb = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>與一般能力相同顯示在角色能力列；數字為層數。可吸收總量見 smartDescription 的 <c>{TotalAbsorbHp}</c>。</summary>
    protected override bool IsVisibleInternal => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("AbsorbPerOrb", DamageAbsorbedPerOrb),
        new DynamicVar("MaxStacks", FireballOrbitRing.MaxOrbs),
        new DynamicVar("TotalAbsorbHp", 0m),
    ];

    /// <summary>
    /// Tooltip for orb hover. Vanilla <see cref="PowerModel.DumbHoverTip"/> does not call <c>LocString.Add("Amount", …)</c>
    /// (only <c>AddDumbVariablesToDescription</c> for star/energy), so <c>{Amount}</c> would stay unreplaced.
    /// This mirrors the variable injection used in <see cref="PowerModel.HoverTips"/> for smart descriptions.
    /// </summary>
    public HoverTip CreateOrbitHoverTip()
    {
        SyncAbsorbTooltipDynamicVars();
        LocString locString = HasSmartDescription ? SmartDescription : Description;
        Creature? owner = Owner;
        bool onPlayer = owner?.IsPlayer == true;
        int playerCount = owner?.CombatState?.Players?.Count ?? 1;

        locString.Add("Amount", Amount);
        locString.Add("OnPlayer", onPlayer);
        locString.Add("IsMultiplayer", playerCount > 1);
        locString.Add("PlayerCount", playerCount);
        locString.Add("OwnerName", owner == null ? string.Empty : GetCreatureDisplayName(owner));
        if (Applier != null)
            locString.Add("ApplierName", GetCreatureDisplayName(Applier));
        if (Target != null)
            locString.Add("TargetName", GetCreatureDisplayName(Target));
        DynamicVars.AddTo(locString);
        locString.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
        locString.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
        bool isSmart = HasSmartDescription && IsMutable;
        return new HoverTip(this, locString.GetFormattedText(), isSmart);
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SyncAbsorbTooltipDynamicVars();
        if (!TestMode.IsOn && Owner != null)
            FireballOrbitVisuals.Sync(Owner, VisibleCount);
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        if (power != this || TestMode.IsOn || Owner?.GetPower<FireballSwarmPower>() != this)
            return;

        if (Amount > FireballOrbitRing.MaxOrbs)
            SetAmount(FireballOrbitRing.MaxOrbs, silent: true);

        SyncAbsorbTooltipDynamicVars();
        FireballOrbitVisuals.Sync(Owner, VisibleCountFor(Amount));
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (!TestMode.IsOn)
            FireballOrbitVisuals.Sync(oldOwner, 0);
        return Task.CompletedTask;
    }

    /// <summary>Runs after block; reduces HP loss and consumes whole orbs (each worth up to <see cref="DamageAbsorbedPerOrb"/>).</summary>
    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0m)
            return amount;

        int orbs = Math.Min(FireballOrbitRing.MaxOrbs, Math.Max(0, Amount));
        if (orbs <= 0)
            return amount;

        int orbsNeeded = (int)Math.Min(orbs, Math.Ceiling(amount / (decimal)DamageAbsorbedPerOrb));
        decimal absorbed = orbsNeeded * (decimal)DamageAbsorbedPerOrb;
        decimal newAmount = decimal.Max(0m, amount - absorbed);

        if (orbsNeeded > 0)
        {
            Flash();
            Creature ownerRef = Owner;
            SetAmount(Amount - orbsNeeded, silent: true);
            if (ShouldRemoveDueToAmount())
            {
                RemoveInternal();
                if (!TestMode.IsOn)
                    FireballOrbitVisuals.Sync(ownerRef, 0);
            }
            else if (!TestMode.IsOn)
            {
                SyncAbsorbTooltipDynamicVars();
                FireballOrbitVisuals.Sync(ownerRef, VisibleCountFor(Amount));
            }
        }

        return newAmount;
    }

    private int VisibleCount => VisibleCountFor(Amount);

    private static string GetCreatureDisplayName(Creature creature)
    {
        if (creature.IsPlayer)
        {
            return creature.Player?.Character?.Title?.GetFormattedText() ?? creature.Name;
        }

        return creature.Monster?.Title?.GetFormattedText() ?? creature.Name;
    }

    private static int VisibleCountFor(decimal amount) =>
        (int)Math.Clamp(Math.Floor(amount), 0, FireballOrbitRing.MaxOrbs);

    /// <summary>同步 <see cref="CanonicalVars"/> 內 <c>TotalAbsorbHp</c>（層數上限內 × 每層吸收上限），供能力列／浮球說明替換。</summary>
    private void SyncAbsorbTooltipDynamicVars()
    {
        if (!IsMutable)
        {
            return;
        }

        int stacks = Math.Min(Math.Max(0, Amount), FireballOrbitRing.MaxOrbs);
        DynamicVars["TotalAbsorbHp"].BaseValue = stacks * (decimal)DamageAbsorbedPerOrb;
    }
}
