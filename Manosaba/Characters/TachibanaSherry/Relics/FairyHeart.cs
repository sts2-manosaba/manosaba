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
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.TachibanaSherry.Relics
{
    [Pool(typeof(TachibanaSherryRelicPool))]
    public sealed class MagnifyingGlass : LevelingPathCustomRelicModel
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

            decimal majoka = base.Owner.Creature.GetPowerAmount<MajokaPower>();
            if (majoka > 0)
                await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, -majoka, player.Creature, null);
        }

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

            decimal factor = RelicLevel >= 4 ? 0.7m : RelicLevel >= 2 ? 0.6m : 0.5m;
            return -(strength * factor);
        }
    }
}
