using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;

namespace Manosaba.Characters.Common.Relics;

/// <summary>
/// Placeholder starting relic for locked character select slots.
/// Registered on <see cref="LockedCharacterStarterRelicPool"/>, the <c>RelicPool</c> for locked characters.
/// Uses the vanilla locked-model silhouette for icons; select UI uses main_menu_ui locked relic strings.
/// </summary>
[Pool(typeof(LockedCharacterStarterRelicPool))]
public sealed class LockedCharacterStarterRelic : CustomRelicModel
{
    private static readonly string LockedIconPath =
        ImageHelper.GetImagePath("packed/common_ui/locked_model.png");

    public override RelicRarity Rarity => RelicRarity.None;

    public override string PackedIconPath => LockedIconPath;

    protected override string PackedIconOutlinePath => LockedIconPath;

    protected override string BigIconPath => LockedIconPath;
}
