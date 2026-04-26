using MegaCrit.Sts2.Core.Entities.Players;

namespace Manosaba.Characters.Common.Resources;

public static class CharacterCustomEnergyService
{
    private static readonly Dictionary<string, CharacterCustomEnergyDefinition> Definitions = new(StringComparer.Ordinal);
    private static readonly Dictionary<(ulong PlayerNetId, string EnergyId), int> Values = new();
    private static readonly HashSet<(ulong PlayerNetId, string EnergyId)> InitializedThisCombat = new();

    public static void Register(CharacterCustomEnergyDefinition definition)
    {
        Definitions[definition.EnergyId] = definition;
    }

    public static int Get(Player player, CharacterCustomEnergyDefinition definition)
    {
        if (player == null)
        {
            return definition.MinEnergy;
        }

        EnsureInitializedForCombat(player, definition);
        if (Values.TryGetValue(GetKey(player, definition), out int value))
        {
            return value;
        }

        if (TryGetFallbackValue(player, definition, out int fallback))
        {
            return fallback;
        }

        return definition.MinEnergy;
    }

    public static int Set(Player player, CharacterCustomEnergyDefinition definition, int value)
    {
        if (player == null)
        {
            return definition.Clamp(value);
        }

        (ulong, string) key = GetKey(player, definition);
        InitializedThisCombat.Add(key);
        int clamped = definition.Clamp(value);
        Values[key] = clamped;
        SyncSinglePlayerAliases(player, definition, clamped);
        return clamped;
    }

    public static int Gain(Player player, CharacterCustomEnergyDefinition definition, int amount)
    {
        int safeAmount = Math.Max(0, amount);
        return Set(player, definition, Get(player, definition) + safeAmount);
    }

    public static int Lose(Player player, CharacterCustomEnergyDefinition definition, int amount)
    {
        int safeAmount = Math.Max(0, amount);
        return Set(player, definition, Get(player, definition) - safeAmount);
    }

    public static bool HasEnough(Player player, CharacterCustomEnergyDefinition definition, int amount)
    {
        if (player == null)
        {
            return Math.Max(0, amount) <= 0;
        }

        return Get(player, definition) >= Math.Max(0, amount);
    }

    public static bool TrySpend(Player player, CharacterCustomEnergyDefinition definition, int amount)
    {
        if (player == null)
        {
            return Math.Max(0, amount) <= 0;
        }

        int safeAmount = Math.Max(0, amount);
        if (!HasEnough(player, definition, safeAmount))
        {
            return false;
        }

        Lose(player, definition, safeAmount);
        return true;
    }

    public static void OnCombatEnd(Player player)
    {
        if (player == null)
        {
            return;
        }

        foreach (CharacterCustomEnergyDefinition definition in Definitions.Values)
        {
            (ulong, string) key = GetKey(player, definition);
            InitializedThisCombat.Remove(key);
            Values.Remove(key);
        }
    }

    public static void ClearAll()
    {
        Values.Clear();
        InitializedThisCombat.Clear();
    }

    private static (ulong PlayerNetId, string EnergyId) GetKey(Player player, CharacterCustomEnergyDefinition definition)
    {
        return (player.NetId, definition.EnergyId);
    }

    private static bool TryGetFallbackValue(Player player, CharacterCustomEnergyDefinition definition, out int value)
    {
        value = definition.MinEnergy;
        if (player?.RunState == null)
        {
            return false;
        }

        // In multiplayer, fallback lookup can accidentally mirror another player's
        // custom-energy pool when this player has no explicit value yet.
        if (player.RunState.Players.Count > 1)
        {
            return false;
        }

        bool foundAny = false;
        foreach ((ulong PlayerNetId, string EnergyId) key in Values.Keys)
        {
            if (!string.Equals(key.EnergyId, definition.EnergyId, StringComparison.Ordinal))
            {
                continue;
            }

            Player? candidate = player.RunState.GetPlayer(key.PlayerNetId);
            if (candidate == null || !definition.AppliesTo(candidate))
            {
                continue;
            }

            int candidateValue = Values[key];
            if (!foundAny || candidateValue > value)
            {
                value = candidateValue;
                foundAny = true;
            }
        }

        return foundAny;
    }

    private static void EnsureInitializedForCombat(Player player, CharacterCustomEnergyDefinition definition)
    {
        if (player == null)
        {
            return;
        }

        if (!definition.AppliesTo(player))
        {
            return;
        }

        (ulong, string) key = GetKey(player, definition);
        if (InitializedThisCombat.Contains(key))
        {
            return;
        }

        InitializedThisCombat.Add(key);
        Values[key] = definition.Clamp(definition.InitialEnergyPerCombat);
    }

    private static void SyncSinglePlayerAliases(Player player, CharacterCustomEnergyDefinition definition, int value)
    {
        if (player?.RunState == null)
        {
            return;
        }

        if (player.RunState.Players.Count != 1)
        {
            return;
        }

        List<(ulong PlayerNetId, string EnergyId)> keys = Values.Keys.ToList();
        foreach ((ulong PlayerNetId, string EnergyId) aliasKey in keys)
        {
            if (!string.Equals(aliasKey.EnergyId, definition.EnergyId, StringComparison.Ordinal))
            {
                continue;
            }

            Values[aliasKey] = value;
            InitializedThisCombat.Add(aliasKey);
        }
    }
}
