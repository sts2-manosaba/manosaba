using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;

namespace Manosaba.Characters.HikamiMeruru.Powers
{
    public class PotionFactoryPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {

            List<PotionModel> PotionPool =
            [
                ModelDb.Potion<BlockPotion>(),
                ModelDb.Potion<PainKillerPotion>(),
                ModelDb.Potion<FlexPotion>(),
                ModelDb.Potion<BeetleJuice>(),
                ModelDb.Potion<EnergyPotion>(),
            ];
            if (side != base.Owner.Side)
            {
                return;
            }

            if (base.Owner.Player == null)
            {
                return;
            }

            for (int i = 0; i < base.Amount; i++)
            {
                PotionModel potionModel = PotionPool[Owner.Player.RunState.Rng.CombatPotionGeneration.NextInt(PotionPool.Count)];
                await PotionCmd.TryToProcure(potionModel.ToMutable(), Owner.Player);
            }
        }
    }
}
