using Manosaba;

namespace manosaba.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", path);
    }

    public static string CardsImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "cards", path);
    }

    public static string CardImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "card_portraits", path);
    }
    public static string BigCardImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "card_portraits", "big", path);
    }

    public static string PowerImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "powers", path);
    }

    public static string BigPowerImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "powers", "big", path);
    }

    public static string RelicImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "relics", path);
    }

    public static string BigRelicImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "relics", "big", path);
    }

    public static string CharacterImgPath(this string path, String character)
    {
        return Path.Join(Entry.ModId, "images", "characters", character, path);
    }

    public static string CharacterScenePath(this string path, String character)
    {
        return Path.Join(Entry.ModId, "scenes", character, path);
    }

    public static string MonsterScenePath(this string path)
    {
        return Path.Join(Entry.ModId, "scenes", "creature_visuals", path);
    }

    public static string PotionImagePath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "potions", path);
    }

    public static string OrbImgPath(this string path)
    {
        return Path.Join(Entry.ModId, "images", "orbs", path);
    }

    public static string OrbScenePath(this string path)
    {
        return Path.Join(Entry.ModId, "scenes", "orbs", "orb_visuals", path);
    }
}