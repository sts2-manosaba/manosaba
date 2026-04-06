using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.TachibanaSherry.Relics
{
    [Pool(typeof(TachibanaSherryRelicPool))]
    public sealed class FairyHeart : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override async Task BeforeCombatStart()
        {
            if (Owner.Creature == null)
                return;

            await PowerCmd.Apply<StrengthPower>(Owner.Creature, RelicLevel, Owner.Creature, null);
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature != player.Creature)
                return;

            if (RelicLevel >= 2)
            {
                await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m, ValueProp.Unpowered, base.Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
            }

            decimal majoka = base.Owner.Creature.GetPowerAmount<MajokaPower>();
            if (majoka > 0)
                await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, -majoka, player.Creature, null);
        }

        // Victory flow strips powers on the player before AfterCombatVictory; read Strength here (same window as Hook.AfterCombatEnd).
        public override async Task AfterCombatEnd(CombatRoom room)
        {
            if (RelicLevel < 4 || Owner.Creature == null || Owner.Creature.IsDead)
                return;

            decimal str = Owner.Creature.GetPowerAmount<StrengthPower>();
            if (str <= 0m)
                return;

            Flash();
            await CreatureCmd.Heal(Owner.Creature, str);
        }

        public override Task AfterCombatVictory(CombatRoom room) => base.AfterCombatVictory(room);

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != base.Owner.Creature)
                return 0m;

            if (dealer == null)
                return 0m;

            // Match AttackCommand targeting: dealer must be on the opposing combat side (see CombatState.GetOpponentsOf).
            CombatState? combatState = base.Owner.Creature.CombatState;
            if (combatState == null || !combatState.GetOpponentsOf(base.Owner.Creature).Contains(dealer))
                return 0m;

            // Same condition as ValuePropExtensions.IsPoweredAttack (internal in sts2 — inlined here).
            if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
                return 0m;

            decimal strength = base.Owner.Creature.GetPowerAmount<StrengthPower>();
            if (strength <= 0m)
                return 0m;

            return -(strength * 0.5m);
        }
    }
}
