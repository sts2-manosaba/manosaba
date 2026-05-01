using System.Text.Json;
using BaseLib.Config;
using Manosaba.Config;
using Manosaba.Multiplayer.Messages.Lobby;

namespace Manosaba.Multiplayer;

/// <summary>
/// Binds Manosaba lobby difficulty to vanilla save identity: <see cref="MegaCrit.Sts2.Core.Saves.SerializableRun.StartTime"/>
/// plus solo vs multi (<see cref="MegaCrit.Sts2.Core.Saves.SerializableRun.Players"/> count).
/// </summary>
public static class ManosabaPerSaveDifficultyStore
{
    private const int MaxEntries = 32;

    /// <summary>1 = singleplayer-sized save; 2 = multiplayer-sized save (2+ players).</summary>
    private const byte SaveKindSolo = 1;

    private const byte SaveKindMulti = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private sealed class Payload
    {
        public List<Entry> Entries { get; set; } = [];
    }

    private sealed class Entry
    {
        public long StartTime { get; set; }

        /// <summary>0 = legacy entry before this field existed; 1 = solo; 2 = multi.</summary>
        public byte SaveKind { get; set; }

        public double EnemyHpMultiplierPercent { get; set; }

        public double EnemyAttackDamageMultiplierPercent { get; set; }

        public double MurderousImpulseAllyDamageMultiplierPercent { get; set; }

        public byte RandomCharacterPoolMode { get; set; }
    }

    private static byte KindFromPlayerCount(int playerCount) => playerCount >= 2 ? SaveKindMulti : SaveKindSolo;

    /// <summary>Saves the difficulty snapshot for this run id (unix start time from vanilla save).</summary>
    public static void SaveForRun(
        long runStartTime,
        int playerCount,
        double enemyHpPercent,
        double enemyAttackPercent,
        double murderousPercent,
        RandomCharacterPoolMode randomPool)
    {
        if (runStartTime == 0)
        {
            return;
        }

        byte kind = KindFromPlayerCount(playerCount);
        enemyHpPercent = Math.Clamp(enemyHpPercent, 100d, 400d);
        enemyAttackPercent = Math.Clamp(enemyAttackPercent, 100d, 400d);
        murderousPercent = Math.Clamp(murderousPercent, 0d, 100d);
        byte poolByte = randomPool is RandomCharacterPoolMode.AllCharacters
            ? (byte)RandomCharacterPoolMode.AllCharacters
            : (byte)RandomCharacterPoolMode.ManosabaCharactersOnly;

        Payload payload = LoadPayload();
        List<Entry> list = payload.Entries;
        if (kind == SaveKindSolo)
        {
            list.RemoveAll(e => e.StartTime == runStartTime && (e.SaveKind == SaveKindSolo || e.SaveKind == 0));
        }
        else
        {
            list.RemoveAll(e => e.StartTime == runStartTime && e.SaveKind == SaveKindMulti);
        }

        list.Add(new Entry
        {
            StartTime = runStartTime,
            SaveKind = kind,
            EnemyHpMultiplierPercent = enemyHpPercent,
            EnemyAttackDamageMultiplierPercent = enemyAttackPercent,
            MurderousImpulseAllyDamageMultiplierPercent = murderousPercent,
            RandomCharacterPoolMode = poolByte,
        });

        PruneEntries(list);
        PersistPayload(payload);
    }

    public static bool TryLoadForRun(long runStartTime, int playerCount, out ManosabaDifficultySettingsMessage message)
    {
        message = default;
        if (runStartTime == 0)
        {
            return false;
        }

        byte want = KindFromPlayerCount(playerCount);
        Payload payload = LoadPayload();

        Entry? entry = payload.Entries.Find(e => e.StartTime == runStartTime && e.SaveKind == want)
            ?? (want == SaveKindSolo ? payload.Entries.Find(e => e.StartTime == runStartTime && e.SaveKind == 0) : null);

        if (entry == null)
        {
            return false;
        }

        message = new ManosabaDifficultySettingsMessage
        {
            enemyHpMultiplierPercent = Math.Clamp(entry.EnemyHpMultiplierPercent, 100d, 400d),
            enemyAttackDamageMultiplierPercent = Math.Clamp(entry.EnemyAttackDamageMultiplierPercent, 100d, 400d),
            murderousImpulseAllyDamageMultiplierPercent = Math.Clamp(entry.MurderousImpulseAllyDamageMultiplierPercent, 0d, 100d),
            randomCharacterPoolMode = entry.RandomCharacterPoolMode == (byte)RandomCharacterPoolMode.AllCharacters
                ? (byte)RandomCharacterPoolMode.AllCharacters
                : (byte)RandomCharacterPoolMode.ManosabaCharactersOnly,
        };
        return true;
    }

    private static void PruneEntries(List<Entry> list)
    {
        if (list.Count <= MaxEntries)
        {
            return;
        }

        list.Sort((a, b) =>
        {
            int c = b.StartTime.CompareTo(a.StartTime);
            return c != 0 ? c : b.SaveKind.CompareTo(a.SaveKind);
        });
        list.RemoveRange(MaxEntries, list.Count - MaxEntries);
    }

    private static Payload LoadPayload()
    {
        string json = ManosabaConfig.PerSaveLobbyDifficultyJson;
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Payload();
        }

        try
        {
            return JsonSerializer.Deserialize<Payload>(json, JsonOptions) ?? new Payload();
        }
        catch (JsonException)
        {
            return new Payload();
        }
    }

    private static void PersistPayload(Payload payload)
    {
        string json = JsonSerializer.Serialize(payload, JsonOptions);
        ManosabaConfig.PerSaveLobbyDifficultyJson = json;
        ModConfig.SaveDebounced<ManosabaConfig>(250);
    }
}
