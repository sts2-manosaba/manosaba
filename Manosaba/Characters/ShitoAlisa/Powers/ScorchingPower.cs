using BaseLib.Utils;
using System.Linq;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Powers;

/// <summary>回合開始對全體敵人施加灼燒；Amount = 每次施加的層數。</summary>
public sealed class ScorchingPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || CombatState == null)
            return;

        decimal stacks = Amount;
        foreach (Creature e in CombatState.GetOpponentsOf(Owner).ToList())
        {
            if (e.IsAlive && e.IsHittable)
                await CommonActions.Apply<BurnPower>(choiceContext, e, null, stacks);
        }
        await CommonActions.Apply<FireballSwarmPower>(choiceContext, Owner, null, Amount);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<FireballSwarmPower>()];
}
