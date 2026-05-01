using Godot;

namespace Manosaba.Extensions;

/// <summary>Maps a character accent <see cref="Color"/> to card-back HSV shader inputs (0..1).</summary>
public static class CardPoolTintFromCharacterColor
{
    public static (float H, float S, float V) ToCardBackHsv(Color c)
    {
        c.ToHsv(out float h, out float s, out float v);
        return (Clamp01(h), Clamp01(s), Clamp01(v));
    }

    private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}
