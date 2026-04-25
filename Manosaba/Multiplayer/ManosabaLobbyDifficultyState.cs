using System;
using BaseLib.Config;
using Manosaba.Config;
using Manosaba.Multiplayer.Messages.Lobby;

namespace Manosaba.Multiplayer;

public static class ManosabaLobbyDifficultyState
{
    private static bool _lobbySessionActive;
    private static bool _lobbySnapshotInitialized;

    private static double _lobbyEnemyHpMultiplierPercent;
    private static double _lobbyEnemyAttackDamageMultiplierPercent;
    private static double _lobbyMurderousImpulseAllyDamageMultiplierPercent;
    private static RandomCharacterPoolMode _lobbyRandomCharacterPool;

    private static bool _runFrozen;
    private static double _snapEnemyHpMultiplierPercent;
    private static double _snapEnemyAttackDamageMultiplierPercent;
    private static double _snapMurderousImpulseAllyDamageMultiplierPercent;
    private static RandomCharacterPoolMode _snapRandomCharacterPool;

    public static void SetLobbySessionActive(bool active)
    {
        _lobbySessionActive = active;
    }

    public static void ResetToLobbyDefaults()
    {
        SetLobbySnapshot(
            ManosabaLobbyDefaults.EnemyHpMultiplierPercent,
            ManosabaLobbyDefaults.EnemyAttackDamageMultiplierPercent,
            ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent,
            ManosabaLobbyDefaults.RandomCharacterPool);
    }

    public static void ResetToSafeDefaults()
    {
        SetLobbySnapshot(
            ManosabaLobbyDefaults.SafeEnemyHpMultiplierPercent,
            ManosabaLobbyDefaults.SafeEnemyAttackDamageMultiplierPercent,
            ManosabaLobbyDefaults.SafeMurderousImpulseAllyDamageMultiplierPercent,
            ManosabaLobbyDefaults.SafeRandomCharacterPool);
    }

    public static void EnsureLobbySnapshotFromDefaults()
    {
        if (!_lobbySnapshotInitialized)
        {
            ResetToLobbyDefaults();
        }
    }

    public static void ApplyFromHost(ManosabaDifficultySettingsMessage message)
    {
        SetLobbySnapshot(
            message.enemyHpMultiplierPercent,
            message.enemyAttackDamageMultiplierPercent,
            message.murderousImpulseAllyDamageMultiplierPercent,
            ClampRandomPoolMode(message.randomCharacterPoolMode));
    }

    public static void ApplyFromHost(
        double enemyHpPercent,
        double enemyAttackPercent,
        double murderousPercent,
        RandomCharacterPoolMode randomPool)
    {
        SetLobbySnapshot(enemyHpPercent, enemyAttackPercent, murderousPercent, randomPool);
    }

    public static void ClearRunSnapshot()
    {
        _runFrozen = false;
    }

    public static void FreezeForRun()
    {
        if (!_lobbySnapshotInitialized)
        {
            ResetToSafeDefaults();
        }

        _runFrozen = true;
        _snapEnemyHpMultiplierPercent = _lobbyEnemyHpMultiplierPercent;
        _snapEnemyAttackDamageMultiplierPercent = _lobbyEnemyAttackDamageMultiplierPercent;
        _snapMurderousImpulseAllyDamageMultiplierPercent = _lobbyMurderousImpulseAllyDamageMultiplierPercent;
        _snapRandomCharacterPool = _lobbyRandomCharacterPool;
    }

    public static void FreezeForRunFromDefaults()
    {
        ResetToLobbyDefaults();
        FreezeForRun();
    }

    public static void FreezeForRunFromSafeDefaults()
    {
        ResetToSafeDefaults();
        FreezeForRun();
    }

    public static void FreezeForRunFromHost(ManosabaDifficultySettingsMessage message)
    {
        ApplyFromHost(message);
        FreezeForRun();
    }

    public static decimal GetEnemyHpMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return PercentToMultiplier(_snapEnemyHpMultiplierPercent);
        }

        if (_lobbySessionActive && _lobbySnapshotInitialized)
        {
            return PercentToMultiplier(_lobbyEnemyHpMultiplierPercent);
        }

        return PercentToMultiplier(ManosabaLobbyDefaults.SafeEnemyHpMultiplierPercent);
    }

    public static decimal GetEnemyAttackDamageMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return PercentToMultiplier(_snapEnemyAttackDamageMultiplierPercent);
        }

        if (_lobbySessionActive && _lobbySnapshotInitialized)
        {
            return PercentToMultiplier(_lobbyEnemyAttackDamageMultiplierPercent);
        }

        return PercentToMultiplier(ManosabaLobbyDefaults.SafeEnemyAttackDamageMultiplierPercent);
    }

    public static decimal GetMurderousImpulseAllyDamageMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return ComputeMurderousMultiplier(_snapMurderousImpulseAllyDamageMultiplierPercent);
        }

        if (_lobbySessionActive && _lobbySnapshotInitialized)
        {
            return ComputeMurderousMultiplier(_lobbyMurderousImpulseAllyDamageMultiplierPercent);
        }

        return ComputeMurderousMultiplier(ManosabaLobbyDefaults.SafeMurderousImpulseAllyDamageMultiplierPercent);
    }

    public static RandomCharacterPoolMode GetRandomCharacterPoolModeForGameplay()
    {
        if (_runFrozen)
        {
            return _snapRandomCharacterPool;
        }

        if (_lobbySessionActive && _lobbySnapshotInitialized)
        {
            return _lobbyRandomCharacterPool;
        }

        return ManosabaLobbyDefaults.SafeRandomCharacterPool;
    }

    public static bool LobbySessionActive => _lobbySessionActive;

    public static bool RunFrozen => _runFrozen;

    public static (double enemyHpPercent, double enemyAttackPercent, double murderousPercent, RandomCharacterPoolMode randomPool) GetLobbySnapshot()
    {
        if (!_lobbySnapshotInitialized)
        {
            return (
                ManosabaLobbyDefaults.SafeEnemyHpMultiplierPercent,
                ManosabaLobbyDefaults.SafeEnemyAttackDamageMultiplierPercent,
                ManosabaLobbyDefaults.SafeMurderousImpulseAllyDamageMultiplierPercent,
                ManosabaLobbyDefaults.SafeRandomCharacterPool);
        }

        return (
            _lobbyEnemyHpMultiplierPercent,
            _lobbyEnemyAttackDamageMultiplierPercent,
            _lobbyMurderousImpulseAllyDamageMultiplierPercent,
            _lobbyRandomCharacterPool);
    }

    public static void SaveLobbySnapshotAsDefaults()
    {
        if (!_lobbySnapshotInitialized)
        {
            return;
        }

        ManosabaConfig.LobbyEnemyHpMultiplierPercent = Math.Clamp(_lobbyEnemyHpMultiplierPercent, 100d, 400d);
        ManosabaConfig.LobbyEnemyAttackDamageMultiplierPercent = Math.Clamp(_lobbyEnemyAttackDamageMultiplierPercent, 100d, 400d);
        ManosabaConfig.LobbyMurderousImpulseAllyDamageMultiplierPercent = Math.Clamp(_lobbyMurderousImpulseAllyDamageMultiplierPercent, 0d, 100d);
        ManosabaConfig.LobbyRandomCharacterPool = _lobbyRandomCharacterPool is RandomCharacterPoolMode.AllCharacters
            ? RandomCharacterPoolMode.AllCharacters
            : RandomCharacterPoolMode.ManosabaCharactersOnly;

        ModConfig.SaveDebounced<ManosabaConfig>(250);
    }

    private static RandomCharacterPoolMode ClampRandomPoolMode(byte raw)
    {
        return raw switch
        {
            (byte)RandomCharacterPoolMode.AllCharacters => RandomCharacterPoolMode.AllCharacters,
            _ => RandomCharacterPoolMode.ManosabaCharactersOnly,
        };
    }

    private static void SetLobbySnapshot(
        double enemyHpPercent,
        double enemyAttackPercent,
        double murderousPercent,
        RandomCharacterPoolMode randomPool)
    {
        _lobbyEnemyHpMultiplierPercent = Math.Clamp(enemyHpPercent, 100d, 400d);
        _lobbyEnemyAttackDamageMultiplierPercent = Math.Clamp(enemyAttackPercent, 100d, 400d);
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = Math.Clamp(murderousPercent, 0d, 100d);
        _lobbyRandomCharacterPool = randomPool is RandomCharacterPoolMode.AllCharacters
            ? RandomCharacterPoolMode.AllCharacters
            : RandomCharacterPoolMode.ManosabaCharactersOnly;
        _lobbySnapshotInitialized = true;
    }

    private static decimal PercentToMultiplier(double percent)
    {
        double p = percent < 1d ? 1d : percent;
        return (decimal)(p / 100d);
    }

    private static decimal ComputeMurderousMultiplier(double percent)
    {
        double p = Math.Clamp(percent, 0d, 100d);
        return (decimal)(p / 100d);
    }
}
