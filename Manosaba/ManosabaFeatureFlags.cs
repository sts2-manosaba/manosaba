using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Manosaba;

public static class ManosabaFeatureFlags
{
    // Startup toggle:
    // set environment variable MANOSABA_APRIL_FOOLS_MODE=1 before launching the game.
    // Any other value (or unset) means disabled.
    public static bool AprilFoolsModeEnabled { get; private set; } = IsEnabledFromEnv("MANOSABA_APRIL_FOOLS_MODE");

    public static bool ToggleAprilFoolsMode()
    {
        AprilFoolsModeEnabled = !AprilFoolsModeEnabled;
        // Keep card text synchronized immediately after runtime toggle.
        TryApplyQuickWitCardDescription();
        return AprilFoolsModeEnabled;
    }

    public static void SetAprilFoolsMode(bool enabled)
    {
        AprilFoolsModeEnabled = enabled;
        // Used by multiplayer sync packets to update local UI text.
        TryApplyQuickWitCardDescription();
    }

    private static bool IsEnabledFromEnv(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("on", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public static void TryApplyQuickWitCardDescription()
    {
        try
        {
            // CardModel.Description cannot be overridden, so we swap the active text
            // by rewriting the localized value behind MANOSABA-QUICK_WIT.description.
            LocTable table = LocManager.Instance.GetTable("cards");
            var translationsField = AccessTools.Field(typeof(LocTable), "_translations");
            if (translationsField?.GetValue(table) is not Dictionary<string, string> translations)
                return;

            string targetKey = "MANOSABA-QUICK_WIT.description";
            string sourceKey = AprilFoolsModeEnabled ? "MANOSABA-QUICK_WIT.description.april" : "MANOSABA-QUICK_WIT.description.normal";
            if (!translations.TryGetValue(sourceKey, out string source))
                return;

            translations[targetKey] = source;
        }
        catch
        {
            // Ignore localization sync failures; this is only for UI text switching.
        }
    }
}
