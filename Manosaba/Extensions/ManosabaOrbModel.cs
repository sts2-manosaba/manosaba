using manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Extensions
{
    public abstract class ManosabaOrbModel : OrbModel
    {
        public string CustomIconPath => $"{Id.Entry.ToLowerInvariant()}.png".OrbImgPath();

        public string CustomScenePath => $"{Id.Entry.ToLowerInvariant()}.tscn".OrbScenePath();

        protected override string ChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";

    }
}
