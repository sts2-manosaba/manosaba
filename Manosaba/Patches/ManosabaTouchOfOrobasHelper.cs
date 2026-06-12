using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

internal static class ManosabaTouchOfOrobasHelper
{
    internal static bool UsesManosabaLevelUpgrade(TouchOfOrobas touch)
    {
        if (touch.StarterRelic != null && IsManosabaLevelingStarterId(touch.StarterRelic))
        {
            return true;
        }

        Player? owner = touch.Owner;
        if (owner == null || !ManosabaPlayerHelper.IsManosabaPlayer(owner))
        {
            return false;
        }

        return owner.Relics.FirstOrDefault(relic => relic.Rarity == RelicRarity.Starter)
            is LevelingPathCustomRelicModel;
    }

    internal static LocString BuildDescription(TouchOfOrobas touch, string keySuffix)
    {
        LocString description = new("relics", "MANOSABA-TOUCH_OF_OROBAS." + keySuffix);
        touch.DynamicVars.AddTo(description);
        description.Add("energyPrefix", EnergyIconHelper.GetPrefix(touch));
        description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
        return description;
    }

    private static bool IsManosabaLevelingStarterId(ModelId relicId)
        => ModelDb.GetByIdOrNull<RelicModel>(relicId) is LevelingPathCustomRelicModel;
}
