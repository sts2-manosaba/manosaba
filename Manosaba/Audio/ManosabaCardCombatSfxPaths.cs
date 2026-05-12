namespace Manosaba.Audio;

/// <summary>
/// Combat card play SFX use FMOD-style <c>event:/Manosaba/audio/SFX/...</c> paths resolved by the game to
/// <c>res://Manosaba/audio/SFX/</c>. Sherry uses per-character subfolder VO; other characters currently map to
/// shared clips already shipped under <c>audio/SFX/</c> until per-character <c>{character_id}/attack.ogg</c> etc. exist.
/// </summary>
public static class ManosabaCardCombatSfxPaths
{
    public const string SfxRoot = "event:/Manosaba/audio/SFX/";

    /// <summary>Preferred layout once assets exist (same filenames for every character folder).</summary>
    public static string AttackInCharacterFolder(string characterId) => $"{SfxRoot}{characterId}/attack.ogg";

    public static string CastInCharacterFolder(string characterId) => $"{SfxRoot}{characterId}/cast.ogg";

    public static string DeathInCharacterFolder(string characterId) => $"{SfxRoot}{characterId}/death.ogg";

    // --- Shared clips (see res://Manosaba/audio/SFX/*.import in repo) ---

    public const string SharedAttackIntent = SfxRoot + "attack_intent.wav";
    public const string SharedGunShot = SfxRoot + "gun_shot.ogg";
    public const string SharedParrySuccess = SfxRoot + "parry_success.wav";
    public const string SharedGlassShatter = SfxRoot + "glass_shatter.ogg";
    public const string SharedReward = SfxRoot + "reward.mp3";
    public const string SharedAwp = SfxRoot + "AWP.mp3";
}
