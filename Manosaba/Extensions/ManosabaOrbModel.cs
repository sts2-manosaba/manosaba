using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
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
            decimal amplified = orbModifiedValue * (1m + majokaAmount / 100m);
            return Math.Max(1m, Math.Floor(amplified));
        }

    }
}
