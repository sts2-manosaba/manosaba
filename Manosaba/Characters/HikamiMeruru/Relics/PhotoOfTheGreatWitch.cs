using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.HikamiMeruru.Relics
{

    [Pool(typeof(HikamiMeruruRelicPool))]
    public sealed class PhotoOfTheGreatWitch : PathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PotionSlots", 3m)];

        public override async Task AfterObtained()
        {
            await PlayerCmd.GainMaxPotionCount(base.DynamicVars["PotionSlots"].IntValue, base.Owner);
        }

        public override decimal ModifyHealAmount(Creature creature, decimal amount)
        {
            if (creature.IsPlayer)
            {
                return amount * 1.2m;
            }
            else
            {
                return amount;
            }
        }
    }
}
