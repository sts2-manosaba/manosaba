using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;

[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.FlipX))]
public static class Patch_NRestSiteCharacter_FlipX
{
    static void Postfix(NRestSiteCharacter __instance)
    {

        foreach (var sprite in __instance.GetChildren().OfType<Node2D>()
                     .Where(n => n.GetClass() == "Sprite2D"))
        {
            var s = sprite.Scale; s.X = -s.X; sprite.Scale = s;
            var p = sprite.Position; p.X = -p.X; sprite.Position = p;
        }
    }
}
