using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;

namespace Manosaba.Extensions
{
    public abstract class PathCustomRelicModel : CustomRelicModel
    {
        public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
        protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
        protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
    }
}
