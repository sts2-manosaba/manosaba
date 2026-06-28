using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SawatariCoco.Powers;

public sealed class FanServiceSkillPower : PathCustomPowerModel
{
    public const int DefaultCooldownTurnsAfterUse = 3;
    public const int MinimumCooldownTurnsAfterUse = 1;

    private const string CooldownVarName = "Cooldown";
    private const string CooldownTurnsAfterUseVarName = "CooldownTurnsAfterUse";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(CooldownVarName, 0m),
        new DynamicVar(CooldownTurnsAfterUseVarName, DefaultCooldownTurnsAfterUse),
    ];

    public int CooldownRemaining => DynamicVars[CooldownVarName].IntValue;

    public int CooldownTurnsAfterUse => DynamicVars[CooldownTurnsAfterUseVarName].IntValue;

    public bool IsReady => CooldownRemaining <= 0;

    public void MarkUsed()
    {
        DynamicVars[CooldownVarName].BaseValue = CooldownTurnsAfterUse;
    }

    public void OnDuplicateCardPlayed()
    {
        DynamicVar cooldownTurnsAfterUse = DynamicVars[CooldownTurnsAfterUseVarName];
        if (cooldownTurnsAfterUse.IntValue <= MinimumCooldownTurnsAfterUse)
        {
            return;
        }

        cooldownTurnsAfterUse.BaseValue--;

        if (CooldownRemaining <= 0)
        {
            return;
        }

        DynamicVars[CooldownVarName].BaseValue--;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player != Owner.Player || CooldownRemaining <= 0)
        {
            return Task.CompletedTask;
        }

        DynamicVars[CooldownVarName].BaseValue--;
        return Task.CompletedTask;
    }
}
