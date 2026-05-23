using BaseLib.Abstracts;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Manosaba.Characters.Common.Relics;

/// <summary>
/// Placeholder starting relic for locked character select slots.
/// Uses the same icon as the real starter but is excluded from the relic compendium.
/// </summary>
public sealed class LockedCharacterStarterRelic : CustomRelicModel
{
    private const string PlaceholderIconName = "leg_irons";

    public LockedCharacterStarterRelic()
        : base(autoAdd: false)
    {
    }

    public override RelicRarity Rarity => RelicRarity.None;

    public override string PackedIconPath => $"{PlaceholderIconName}.png".RelicImagePath();

    protected override string PackedIconOutlinePath => $"{PlaceholderIconName}.png".RelicImagePath();

    protected override string BigIconPath => $"{PlaceholderIconName}.png".RelicImagePath();
}
