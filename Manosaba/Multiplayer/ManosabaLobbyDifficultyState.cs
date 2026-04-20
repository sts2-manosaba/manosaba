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
        _lobbyEnemyHpMultiplierPercent = ManosabaLobbyDefaults.EnemyHpMultiplierPercent;
        _lobbyEnemyAttackDamageMultiplierPercent = ManosabaLobbyDefaults.EnemyAttackDamageMultiplierPercent;
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent;
        _lobbyRandomCharacterPool = ManosabaLobbyDefaults.RandomCharacterPool;
    }

    public static void ApplyFromHost(ManosabaDifficultySettingsMessage message)
    {
        _lobbyEnemyHpMultiplierPercent = message.enemyHpMultiplierPercent;
        _lobbyEnemyAttackDamageMultiplierPercent = message.enemyAttackDamageMultiplierPercent;
        _lobbyMurderousImpulseAllyDamageMultiplierPercent = message.murderousImpulseAllyDamageMultiplierPercent;
        _lobbyRandomCharacterPool = ClampRandomPoolMode(message.randomCharacterPoolMode);
    }

    public static void ApplyFromHost(
        double enemyHpPercent,
        double enemyAttackPercent,
        double murderousPercent,
        RandomCharacterPoolMode randomPool)
    {
        _lobbyEnemyHpMultiplierPercent = enemyHpPercent;
        _lobbyEnemyAttackDamageMultiplierPercent = enemyAttackPercent;
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
        _snapEnemyHpMultiplierPercent = _lobbyEnemyHpMultiplierPercent;
        _snapEnemyAttackDamageMultiplierPercent = _lobbyEnemyAttackDamageMultiplierPercent;
        _snapMurderousImpulseAllyDamageMultiplierPercent = _lobbyMurderousImpulseAllyDamageMultiplierPercent;
        _snapRandomCharacterPool = _lobbyRandomCharacterPool;
    }

    /// <summary>讀檔進入 run：無選角階段，以內建預設凍結。</summary>
    public static void FreezeForRunFromDefaults()
    {
        _runFrozen = true;
        _snapEnemyHpMultiplierPercent = ManosabaLobbyDefaults.EnemyHpMultiplierPercent;
        _snapEnemyAttackDamageMultiplierPercent = ManosabaLobbyDefaults.EnemyAttackDamageMultiplierPercent;
        _snapMurderousImpulseAllyDamageMultiplierPercent = ManosabaLobbyDefaults.MurderousImpulseAllyDamageMultiplierPercent;
        _snapRandomCharacterPool = ManosabaLobbyDefaults.RandomCharacterPool;
    }

    public static decimal GetEnemyHpMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return PercentToMultiplier(_snapEnemyHpMultiplierPercent);
        }

        if (_lobbySessionActive)
        {
            return PercentToMultiplier(_lobbyEnemyHpMultiplierPercent);
        }

        return PercentToMultiplier(ManosabaLobbyDefaults.EnemyHpMultiplierPercent);
    }

    public static decimal GetEnemyAttackDamageMultiplierForGameplay()
    {
        if (_runFrozen)
        {
            return PercentToMultiplier(_snapEnemyAttackDamageMultiplierPercent);
        }

        if (_lobbySessionActive)
        {
            return PercentToMultiplier(_lobbyEnemyAttackDamageMultiplierPercent);
        }

        return PercentToMultiplier(ManosabaLobbyDefaults.EnemyAttackDamageMultiplierPercent);
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

    public static (double enemyHpPercent, double enemyAttackPercent, double murderousPercent, RandomCharacterPoolMode randomPool) GetLobbySnapshot()
    {
        return (
            _lobbyEnemyHpMultiplierPercent,
            _lobbyEnemyAttackDamageMultiplierPercent,
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
