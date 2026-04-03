using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;

namespace Manosaba.Extensions;

public abstract class PathCustomPotionModel : CustomPotionModel
{
    public override string CustomPackedImagePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string CustomPackedOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
}

