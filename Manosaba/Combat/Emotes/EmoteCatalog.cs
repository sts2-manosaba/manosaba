using System;
using Godot;

namespace Manosaba.Combat.Emotes;

public static class EmoteCatalog
{
    public const string EmoteFolderPath = "res://Manosaba/images/emotes/";

    private static readonly Dictionary<string, Texture2D> Textures = new(StringComparer.Ordinal);

    public static IReadOnlyList<string> StickerIds { get; private set; } = [];

    public static void Reload()
    {
        Textures.Clear();
        List<string> ids = [];
        HashSet<string> seenIds = new(StringComparer.Ordinal);

        string[] fileNames = CollectEmoteFileNames();
        foreach (string fileName in fileNames)
        {
            TryRegisterFile(fileName, ids, seenIds);
        }

        ids.Sort(StringComparer.Ordinal);
        StickerIds = ids;

        if (ids.Count == 0)
        {
            GD.PushWarning(
                $"[Manosaba] No emote textures loaded from {EmoteFolderPath}. " +
                $"Listed: [{string.Join(", ", fileNames)}]. " +
                "Re-export Manosaba.pck after adding .png under Manosaba/Manosaba/images/emotes/.");
        }
    }

    /// <summary>
    /// ListDirBegin/GetNext often returns nothing for mod PCK paths; GetFilesAt is more reliable.
    /// </summary>
    private static string[] CollectEmoteFileNames()
    {
        try
        {
            string[] atPath = DirAccess.GetFilesAt(EmoteFolderPath);
            if (atPath.Length > 0)
            {
                return atPath;
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[Manosaba] DirAccess.GetFilesAt failed for {EmoteFolderPath}: {ex.Message}");
        }

        using DirAccess? dir = DirAccess.Open(EmoteFolderPath);
        if (dir == null)
        {
            GD.PushWarning($"[Manosaba] Emote folder not found at runtime: {EmoteFolderPath}. Re-export Manosaba.pck after adding images.");
            return [];
        }

        return dir.GetFiles();
    }

    /// <summary>
    /// Mod PCK 的 GetFilesAt 常只列出 Godot 的 *.import，需還原成 01.png 再交給 ResourceLoader。
    /// （以前 sample_1.png 能用的是寫死檔名 fallback，不是掃到 .import 也能用。）
    /// </summary>
    private static string ResolveSourceFileName(string listedName)
    {
        if (listedName.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
        {
            return listedName[..^".import".Length];
        }

        return listedName;
    }

    private static void TryRegisterFile(string fileName, List<string> ids, HashSet<string> seenIds)
    {
        fileName = ResolveSourceFileName(fileName);

        string ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not ".png" and not ".webp" and not ".jpg" and not ".jpeg")
        {
            return;
        }

        string stickerId = System.IO.Path.GetFileNameWithoutExtension(fileName);
        if (!seenIds.Add(stickerId))
        {
            return;
        }

        string resourcePath = EmoteFolderPath + fileName;
        if (!ResourceLoader.Exists(resourcePath))
        {
            GD.PushWarning($"[Manosaba] Emote resource missing from pack: {resourcePath}");
            return;
        }

        Texture2D? texture = ResourceLoader.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            GD.PushWarning($"[Manosaba] Failed to load emote texture: {resourcePath}");
            return;
        }

        Textures[stickerId] = texture;
        ids.Add(stickerId);
    }

    public static Texture2D? TryGetTexture(string stickerId)
    {
        return Textures.TryGetValue(stickerId, out Texture2D? texture) ? texture : null;
    }
}
