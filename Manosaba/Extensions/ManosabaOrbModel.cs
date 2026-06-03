using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.JogasakiNoah.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Extensions
{
    public abstract class ManosabaOrbModel : OrbModel
    {
        public string CustomIconPath => $"{Id.Entry.ToLowerInvariant()}.png".OrbImgPath();

        public string CustomScenePath => $"{Id.Entry.ToLowerInvariant()}.tscn".OrbScenePath();

        protected override string ChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";

        protected override string PassiveSfx => "event:/sfx/characters/defect/defect_dark_passive";

        protected override string EvokeSfx => "event:/sfx/characters/defect/defect_dark_evoke";

        protected decimal ModifyPaintOrbValue(decimal baseValue)
        {
            decimal orbModifiedValue = ModifyOrbValue(baseValue);
            decimal majokaAmount = Owner?.Creature?.GetPowerAmount<MajokaPower>() ?? 0m;
            decimal zumaMultiplier = ZumaPower.GetCurrentEvokeMultiplier();
            decimal amplified = orbModifiedValue * MajokaPower.GetMajokaDamageMultiplier(majokaAmount) * zumaMultiplier;
            return Math.Max(1m, Math.Floor(amplified));
        }

        protected Creature? SelectRandomOtherAlivePlayerTeammate()
        {
            if (CombatState == null || Owner?.Creature == null)
            {
                return null;
            }

            List<Creature> candidates = CombatState.GetTeammatesOf(Owner.Creature)
                .Where(c => c != null && c.IsAlive && c.IsPlayer && c != Owner.Creature)
                .ToList();

            return candidates.Count == 0
                ? null
                : Owner.RunState.Rng.CombatTargets.NextItem(candidates);
        }
    }
}
