using System;
using Manosaba.Config;
using Manosaba.Multiplayer.Messages.Lobby;

namespace Manosaba.Multiplayer;

/// <summary>
/// 選角階段的大廳設定（HOST 權威）與開局後凍結快照；讀檔時使用 <see cref="ManosabaLobbyDefaults"/>。
/// </summary>
public static class ManosabaLobbyDifficultyState
{
    private static bool _lobbySessionActive;

    private static bool _lobbyEnableEnemyHpMultiplier;
    private static double _lobbyEnemyHpMultiplierPercent;
    private static double _lobbyMurderousImpulseAllyDamageMultiplierPercent;
    private static RandomCharacterPoolMode _lobbyRandomCharacterPool;

    private static bool _runFrozen;
    private static bool _snapEnableEnemyHpMultiplier;
    private static double _snapEnemyHpMultiplierPercent;
    private static double _snapMurderousImpulseAllyDamageMultiplierPercent;
    private static RandomCharacterPoolMode _snapRandomCharacterPool;

    public static void SetLobbySessionActive(bool active)
    {
        _lobbySessionActive = active;
    }

    public static void ResetToLobbyDefaults()
    {
        _lobbyEnableEnemyHpMultiplier = ManosabaLobbyDefaults.EnableEnemyHpMultiplier;
        _lobbyEnemyHpMultiplierPercent = ManosabaLobbyDefaults.EnemyHpMultiplierPercent;
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent;
        _lobbyRandomCharacterPool = ManosabaLobbyDefaults.RandomCharacterPool;
    }

    public static void ApplyFromHost(ManosabaDifficultySettingsMessage message)
    {
        _lobbyEnableEnemyHpMultiplier = message.enableEnemyHpMultiplier;
        _lobbyEnemyHpMultiplierPercent = message.enemyHpMultiplierPercent;
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = message.murderousImpulseAllyDamageMultiplierPercent;
        _lobbyRandomCharacterPool = ClampRandomPoolMode(message.randomCharacterPoolMode);
    }

    public static void ApplyFromHost(
        bool enableEnemyHp,
        double enemyHpPercent,
        double murderousPercent,
        RandomCharacterPoolMode randomPool)
    {
        _lobbyEnableEnemyHpMultiplier = enableEnemyHp;
        _lobbyEnemyHpMultiplierPercent = enemyHpPercent;
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = murderousPercent;
        _lobbyRandomCharacterPool = randomPool;
    }

    public static void ClearRunSnapshot()
    {
        _runFrozen = false;
    }

    public static void FreezeForRun()
    {
        _runFrozen = true;
        _snapEnableEnemyHpMultiplier = _lobbyEnableEnemyHpMultiplier;
        _snapEnemyHpMultiplierPercent = _lobbyEnemyHpMultiplierPercent;
        _snapMurderousImpulseAllyDamageMultiplierPercent = _lobbyMurderousImpulseAllyDamageMultiplierPercent;
        _snapRandomCharacterPool = _lobbyRandomCharacterPool;
    }

    /// <summary>讀檔進入 run：無選角階段，以內建預設凍結。</summary>
    public static void FreezeForRunFromDefaults()
    {
        _runFrozen = true;
        _snapEnableEnemyHpMultiplier = ManosabaLobbyDefaults.EnableEnemyHpMultiplier;
        _snapEnemyHpMultiplierPercent = ManosabaLobbyDefaults.EnemyHpMultiplierPercent;
        _snapMurderousImpulseAllyDamageMultiplierPercent = ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent;
        _snapRandomCharacterPool = ManosabaLobbyDefaults.RandomCharacterPool;
    }

    public static bool GetEnableEnemyHpMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return _snapEnableEnemyHpMultiplier;
        }

        if (_lobbySessionActive)
        {
            return _lobbyEnableEnemyHpMultiplier;
        }

        return ManosabaLobbyDefaults.EnableEnemyHpMultiplier;
    }

    public static decimal GetEnemyHpMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return ComputeEnemyHpMultiplier(_snapEnableEnemyHpMultiplier, _snapEnemyHpMultiplierPercent);
        }

        if (_lobbySessionActive)
        {
            return ComputeEnemyHpMultiplier(_lobbyEnableEnemyHpMultiplier, _lobbyEnemyHpMultiplierPercent);
        }

        return ComputeEnemyHpMultiplier(
            ManosabaLobbyDefaults.EnableEnemyHpMultiplier,
            ManosabaLobbyDefaults.EnemyHpMultiplierPercent);
    }

    public static decimal GetMurderousImpulseAllyDamageMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return ComputeMurderousMultiplier(_snapMurderousImpulseAllyDamageMultiplierPercent);
        }

        if (_lobbySessionActive)
        {
            return ComputeMurderousMultiplier(_lobbyMurderousImpulseAllyDamageMultiplierPercent);
        }

        return ComputeMurderousMultiplier(ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent);
    }

    public static RandomCharacterPoolMode GetRandomCharacterPoolModeForGameplay()
    {
        if (_runFrozen)
        {
            return _snapRandomCharacterPool;
        }

        if (_lobbySessionActive)
        {
            return _lobbyRandomCharacterPool;
        }

        return ManosabaLobbyDefaults.RandomCharacterPool;
    }

    public static bool LobbySessionActive => _lobbySessionActive;

    public static bool RunFrozen => _runFrozen;

    public static (bool enable, double enemyHpPercent, double murderousPercent, RandomCharacterPoolMode randomPool) GetLobbySnapshot()
    {
        return (
            _lobbyEnableEnemyHpMultiplier,
            _lobbyEnemyHpMultiplierPercent,
            _lobbyMurderousImpulseAllyDamageMultiplierPercent,
            _lobbyRandomCharacterPool);
    }

    private static RandomCharacterPoolMode ClampRandomPoolMode(byte raw)
    {
        return raw switch
        {
            (byte)RandomCharacterPoolMode.AllCharacters => RandomCharacterPoolMode.AllCharacters,
            _ => RandomCharacterPoolMode.ManosabaCharactersOnly,
        };
    }

    private static decimal ComputeEnemyHpMultiplier(bool enable, double percent)
    {
        if (!enable)
        {
            return 1m;
        }

        double p = percent;
        if (p < 1d)
        {
            p = 1d;
        }

        return (decimal)(p / 100d);
    }

    private static decimal ComputeMurderousMultiplier(double percent)
    {
        double p = Math.Clamp(percent, 0d, 100d);
        return (decimal)(p / 100d);
    }
}
