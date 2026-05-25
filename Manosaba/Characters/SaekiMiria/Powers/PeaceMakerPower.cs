using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System;

namespace Manosaba.Characters.SaekiMiria.Powers
{
    public sealed class PeacemakerPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override bool TryModifyPowerAmountReceived(
            PowerModel canonicalPower,
            Creature target,
            decimal amount,
            Creature? applier,
            out decimal modifiedAmount)
        {
            _ = applier;
            modifiedAmount = amount;

            if (canonicalPower is not StrengthPower || amount <= 0m)
                return false;

            if (!target.IsMonster || target.Side == Owner.Side)
                return false;

            modifiedAmount = Math.Max(0m, amount - Amount);
            return modifiedAmount != amount;
        }
    }
}
