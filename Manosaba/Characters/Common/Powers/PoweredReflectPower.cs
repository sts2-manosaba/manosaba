using BaseLib.Extensions;
using Manosaba.Characters.HikamiMeruru.Cards;
using Manosaba.Characters.JogasakiNoahCard.Cards;
using Manosaba.Characters.NikaidoHiro.Cards;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.Common.Powers
{
    public sealed class PoweredReflectPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == base.Owner && result.BlockedDamage > 0 && props.IsPoweredAttack_() && dealer != null)
            {
                await CreatureCmd.Damage(choiceContext, dealer, result.BlockedDamage, ValueProp.Move, base.Owner, null);
            }
        }

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side == base.Owner.Side)
            {
                await PowerCmd.Decrement(this);
            }
        }
    }


}
