using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;

namespace Manosaba.Extensions;

public abstract class PathCustomPotionModel : CustomPotionModel
{
    public override string PackedImagePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string PackedOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
}

