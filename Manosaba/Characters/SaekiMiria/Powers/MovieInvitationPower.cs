using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class MovieInvitationPower : PathCustomPowerModel
{
    private Creature? _invitedTarget;
    private bool _consumed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool IsInstanced => true;
    public override bool AllowNegative => false;

    public void SetInvitedTarget(Creature target)
    {
        _invitedTarget = target;
    }

    public Player? ConsumeInvitedPlayerForRelicMovies()
    {
        if (_consumed || _invitedTarget is not { IsAlive: true, IsPlayer: true } target || target.Player == null)
        {
            return null;
        }

        _consumed = true;
        return target.Player;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player.Creature != Owner)
        {
            return;
        }

        if (_consumed)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        _ = side;
    }
}
