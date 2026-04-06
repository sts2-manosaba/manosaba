namespace manosaba.Extensions;

/// <summary>
/// FMOD-style paths routed by GodotSfxRouter to res://Manosaba/audio/characters/{id}_select.(ogg|wav|mp3).
/// </summary>
public static class ManosabaCharacterSfx
{
    public static string CharacterSelectEvent(string characterId) =>
        $"event:/Manosaba/audio/characters/{characterId}_select";
}
