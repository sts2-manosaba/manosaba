using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Powers;

/// <summary>本場戰鬥已打出過「三！找到線索！」（供六！免費耗能）。</summary>
public sealed class PlayedSanThisTurnPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>僅供六！免費耗能等邏輯，不顯示在角色下方 Power 列。</summary>
    protected override bool IsVisibleInternal => false;
}
