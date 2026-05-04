using BaseLib.Utils;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.NikaidoHiro.Relics
{

    [Pool(typeof(NikaidoHiroRelicPool))]
    public sealed class PenOfHiro : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.FromPower<SusPower>()];
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("VoteCost", 1m), new SummonVar(6)];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override async Task BeforeCombatStart()
        {
            if (Owner.Creature == null)
                return;

            await PowerCmd.Apply<MidStancePower>(Owner.Creature, 1m, Owner.Creature, null);
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            ApplyRelicLevelEffects();

            if (base.Owner.Creature == player.Creature)
            {
                decimal voteCost = base.DynamicVars["VoteCost"].BaseValue;
                decimal currentVote = base.Owner.Creature.GetPowerAmount<SusPower>();
                if (currentVote >= voteCost)
                {
                    await SakurabaEmaDogCmd.Summon(choiceContext, base.Owner, DynamicVars.Summon.BaseValue, this);
                    if (voteCost > 0)
                    {
                        await PowerCmd.Apply<SusPower>(base.Owner.Creature, -voteCost, player.Creature, null);
                    }
                }
            }
        }

        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            // Pen of Hiro level effects are defined on this relic:
            // Lv1: Summon 6
            // Lv2: Summon 8
            // Lv3: Summon 10
            // Lv4: Summon 12
            // Lv5: Summon 15
            int level = RelicLevel;
            base.DynamicVars.Summon.BaseValue = 6m + (level - 1) * 2m;
            base.DynamicVars.Summon.BaseValue += level == 5 ? 1m : 0m;
        }
    }
}
