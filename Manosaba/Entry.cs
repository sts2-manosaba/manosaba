using BaseLib.Config;
using Godot.Bridge;
using HarmonyLib;
using manosaba.Characters.HikamiMeruru.Relics;
using manosaba.Characters.JogasakiNoah.Relics;
using manosaba.Characters.KurobeNanoka.Relics;
using manosaba.Characters.NatsumeAnan.Relics;
using manosaba.Characters.NikaidoHiro.Relics;
using manosaba.Characters.SaekiMiria.Relics;
using manosaba.Characters.TachibanaSherry.Relics;
using manosaba.Characters.ShitoAlisa;
using manosaba.Characters.TonoHanna.Relics;
using Manosaba.Characters.JogasakiNoah.Potions;
using Manosaba.Config;
using Manosaba.Input;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Manosaba;

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
[ModInitializer("Init")]
public class Entry
{
    public static string ModId = "Manosaba";
    // 初始化函数
    public static void Init()
    {
        ModConfigRegistry.Register(ModId, new ManosabaConfig());

        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(DrawingBoard));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(PhotoOfTheGreatWitch));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(PenOfHiro));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(SprayCanOfNoah));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(CabinetKey));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(MagnifyingGlass));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(FeatherFan));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(Ribbon));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(LegIrons));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(Clipboard));
        PerfectGuardInputTracker.EnsureInstalled();
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        Log.Debug("Mod initialized!");
    }
}
