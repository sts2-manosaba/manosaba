using Godot;
using HarmonyLib;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;

[HarmonyPatch]
public static class ManosabaOrbVisualPathPatches
{
    [HarmonyPatch(typeof(OrbModel), nameof(OrbModel.Icon), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool IconPrefix(OrbModel __instance, ref CompressedTexture2D __result)
    {
        if (__instance is not ManosabaOrbModel)
        {
            return true;
        }

        __result = PreloadManager.Cache.GetCompressedTexture2D(((ManosabaOrbModel)__instance).CustomIconPath);
        return false;
    }

    [HarmonyPatch(typeof(OrbModel), nameof(OrbModel.AssetPaths), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool AssetPathsPrefix(OrbModel __instance, ref IEnumerable<string> __result)
    {
        if (__instance is not ManosabaOrbModel)
        {
            return true;
        }

        __result = new[] { ((ManosabaOrbModel)__instance).CustomIconPath, ((ManosabaOrbModel)__instance).CustomScenePath };
        return false;
    }

    [HarmonyPatch(typeof(OrbModel), nameof(OrbModel.CreateSprite))]
    [HarmonyPrefix]
    public static bool CreateSpritePrefix(OrbModel __instance, ref Node2D __result)
    {
        if (__instance is not ManosabaOrbModel)
        {
            return true;
        }

        Node2D node = PreloadManager.Cache.GetScene(((ManosabaOrbModel)__instance).CustomScenePath)
            .Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        __result = node;
        return false;
    }
}
