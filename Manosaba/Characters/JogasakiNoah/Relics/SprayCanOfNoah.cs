using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.JogasakiNoah.Relics
{

    [Pool(typeof(JogasakiNoahRelicPool))]
    public sealed class SprayCanOfNoah : PathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("OrbSlots", 7m)];

        public override async Task BeforeCombatStart()
        {
            await OrbCmd.AddSlots(base.Owner, base.DynamicVars["OrbSlots"].IntValue);
        }
    }
}
