using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;

namespace Manosaba.Extensions
{
    public abstract class PathCustomPowerModel : CustomPowerModel
    {
        public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
        public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
        public override string CustomBigBetaIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    }
}
