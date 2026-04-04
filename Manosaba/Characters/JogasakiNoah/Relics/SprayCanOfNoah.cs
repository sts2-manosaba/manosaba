using BaseLib.Utils;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.JogasakiNoah.Relics
{

    [Pool(typeof(JogasakiNoahRelicPool))]
    public sealed class SprayCanOfNoah : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("OrbSlots", 6m)];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override async Task BeforeCombatStart()
        {
            ApplyRelicLevelEffects();
            await OrbCmd.AddSlots(base.Owner, base.DynamicVars["OrbSlots"].IntValue);
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature != player.Creature)
            {
                return;
            }

            int randomOrbsToChannel = RelicLevel switch
            {
                >= 4 => 2,
                >= 2 => 1,
                _ => 0
            };

            for (int i = 0; i < randomOrbsToChannel; i++)
            {
                OrbModel randomOrb = RollRandomPaintOrb();
                await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), base.Owner);
            }
        }

        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            int bonusSlots = (RelicLevel - 1) / 2;
            base.DynamicVars["OrbSlots"].BaseValue = 6m + bonusSlots;
        }

        private OrbModel RollRandomPaintOrb()
        {
            IReadOnlyList<OrbModel> paintOrbs = JogasakiNoahOrbPool.AllOrbs;
            int idx = base.Owner.RunState.Rng.CombatOrbGeneration.NextInt(paintOrbs.Count);
            return paintOrbs[idx];
        }
    }
}
