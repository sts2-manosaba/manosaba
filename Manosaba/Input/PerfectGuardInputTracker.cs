using Godot;
using Manosaba.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Input;

public sealed partial class PerfectGuardInputTracker : Node
{
    private readonly record struct ParryToken(ulong PlayerId, uint PromptId, double PressElapsedSeconds);

    private const double DefaultWindowSeconds = 0.15d;
    private const double DefaultInputCooldownSeconds = 1.0d;

    private static PerfectGuardInputTracker? _instance;
    private static double _lastSpacePressedAtSeconds = double.NegativeInfinity;
    private static double _nextSpaceInputAllowedAtSeconds = double.NegativeInfinity;
    private static double _lastGuardWindowOpenedAtSeconds = double.NegativeInfinity;
    private static double _lastGuardWindowClosedAtSeconds = double.NegativeInfinity;
    private static uint _activePromptId;
    private static uint _resolvedPromptId;
    private static readonly HashSet<ulong> _guardWindowTargetIds = [];
    private static readonly HashSet<ParryToken> _parryTokens = [];
    private static readonly HashSet<ulong> _resolvedParryTargetIds = [];
    private static readonly Dictionary<AttackCommand, HashSet<ulong>> _autoGuardTargetsByCommand = [];
    private static AttackCommand? _currentDamageCommand;
    private static bool _currentDamageHasPerfectGuardPrompt;
    private static RunLocationTargetedMessageBuffer? _registeredMessageBuffer;
    private static readonly MessageHandlerDelegate<WitchIslandExpeditionParryMessage> ParryMessageHandler = HandleParryMessage;
    private static readonly MessageHandlerDelegate<WitchIslandExpeditionParryResolutionMessage> ParryResolutionMessageHandler = HandleParryResolutionMessage;
    private static TaskCompletionSource<uint>? _resolutionReceivedSource;

    public static void EnsureInstalled()
    {
        if (_instance != null && GodotObject.IsInstanceValid(_instance) && _instance.IsInsideTree())
        {
            EnsureNetworkHandler();
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree sceneTree || sceneTree.Root == null)
        {
            return;
        }

        _instance = new PerfectGuardInputTracker
        {
            Name = nameof(PerfectGuardInputTracker),
        };
        sceneTree.Root.AddChild(_instance);
        EnsureNetworkHandler();
    }

    public static bool TryConsumePerfectGuard(Creature target, double windowSeconds = DefaultWindowSeconds)
    {
        EnsureInstalled();

        if (target.Player == null)
        {
            return false;
        }

        ulong targetPlayerId = target.Player.NetId;
        if (!_currentDamageHasPerfectGuardPrompt)
        {
            return false;
        }

        if (_currentDamageCommand != null &&
            _autoGuardTargetsByCommand.TryGetValue(_currentDamageCommand, out HashSet<ulong>? autoGuardTargets) &&
            autoGuardTargets.Contains(targetPlayerId))
        {
            return true;
        }

        if (IsMultiplayerRun())
        {
            if (_resolvedPromptId != _activePromptId || !_resolvedParryTargetIds.Contains(targetPlayerId))
            {
                return false;
            }

            MarkAutoGuardTarget(targetPlayerId);
            return true;
        }

        double now = NowSeconds();
        if (!TryFindValidParryToken(targetPlayerId, now, windowSeconds, out ParryToken token))
        {
            return false;
        }

        MarkAutoGuardTarget(targetPlayerId);
        if (LocalContext.NetId == token.PlayerId)
        {
            _nextSpaceInputAllowedAtSeconds = now;
        }

        _parryTokens.Remove(token);
        return true;
    }

    public static async Task ResolveMultiplayerParriesAsync(float timeoutSeconds)
    {
        EnsureInstalled();

        RunManager runManager = RunManager.Instance;
        if (!IsMultiplayerRun() || runManager.NetService == null)
        {
            return;
        }

        if (_resolvedPromptId == _activePromptId)
        {
            return;
        }

        if (runManager.NetService.Type == NetGameType.Host)
        {
            List<ulong> parriedPlayerIds = GetValidParryTargetIds().ToList();
            ApplyParryResolution(_activePromptId, parriedPlayerIds);
            WitchIslandExpeditionParryResolutionMessage message = new()
            {
                promptId = _activePromptId,
                Location = runManager.RunLocationTargetedBuffer.CurrentLocation,
            };
            message.parriedPlayerIds.AddRange(parriedPlayerIds);
            runManager.NetService.SendMessage(message);
            return;
        }

        if (runManager.NetService.Type != NetGameType.Client)
        {
            return;
        }

        _resolutionReceivedSource ??= new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task completed = await Task.WhenAny(_resolutionReceivedSource.Task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
        if (completed == _resolutionReceivedSource.Task && _resolutionReceivedSource.Task.Result == _activePromptId)
        {
            return;
        }
    }

    public static void OpenPerfectGuardWindow(double maxDurationSeconds, IEnumerable<ulong> targetPlayerIds)
    {
        double now = NowSeconds();
        _activePromptId++;
        _lastGuardWindowOpenedAtSeconds = now;
        _lastGuardWindowClosedAtSeconds = now + maxDurationSeconds;
        _guardWindowTargetIds.Clear();
        _guardWindowTargetIds.UnionWith(targetPlayerIds);
        _parryTokens.RemoveWhere(token => token.PromptId != _activePromptId);
        _resolvedParryTargetIds.Clear();
        _resolutionReceivedSource = null;
    }

    public static void OpenPerfectGuardWindow(double maxDurationSeconds)
    {
        OpenPerfectGuardWindow(maxDurationSeconds, []);
    }

    public static void ClosePerfectGuardWindow()
    {
        double now = NowSeconds();
        if (now >= _lastGuardWindowOpenedAtSeconds && now < _lastGuardWindowClosedAtSeconds)
        {
            _lastGuardWindowClosedAtSeconds = now;
        }
    }

    public static void BeginDamageResolution(AttackCommand command, bool hasPerfectGuardPrompt)
    {
        _currentDamageCommand = command;
        _currentDamageHasPerfectGuardPrompt = hasPerfectGuardPrompt;
    }

    public static void EndAttack(AttackCommand command)
    {
        if (ReferenceEquals(_currentDamageCommand, command))
        {
            _currentDamageCommand = null;
            _currentDamageHasPerfectGuardPrompt = false;
        }

        _autoGuardTargetsByCommand.Remove(command);
    }

    public override void _Process(double delta)
    {
        _ = delta;
        EnsureNetworkHandler();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true, Echo: false } keyEvent)
        {
            return;
        }

        if (keyEvent.Keycode != Key.Space && keyEvent.PhysicalKeycode != Key.Space)
        {
            return;
        }

        double now = NowSeconds();
        if (now < _nextSpaceInputAllowedAtSeconds)
        {
            return;
        }

        _lastSpacePressedAtSeconds = now;
        _nextSpaceInputAllowedAtSeconds = now + DefaultInputCooldownSeconds;
        if (TryRecordLocalParryPress(now))
        {
            SendLocalParryMessage(now);
        }
    }

    private static bool TryRecordLocalParryPress(double now)
    {
        if (!TryGetLocalExpeditionTargetId(out ulong localPlayerId))
        {
            return false;
        }

        if (now < _lastGuardWindowOpenedAtSeconds || now > _lastGuardWindowClosedAtSeconds)
        {
            return false;
        }

        double pressElapsedSeconds = now - _lastGuardWindowOpenedAtSeconds;
        _parryTokens.Add(new ParryToken(localPlayerId, _activePromptId, pressElapsedSeconds));
        return true;
    }

    private static bool TryGetLocalExpeditionTargetId(out ulong localPlayerId)
    {
        localPlayerId = 0;
        if (!LocalContext.NetId.HasValue || !_guardWindowTargetIds.Contains(LocalContext.NetId.Value))
        {
            return false;
        }

        localPlayerId = LocalContext.NetId.Value;
        return true;
    }

    private static bool TryFindValidParryToken(
        ulong targetPlayerId,
        double now,
        double windowSeconds,
        out ParryToken token)
    {
        double damageElapsedSeconds = Math.Max(0d, Math.Min(now, _lastGuardWindowClosedAtSeconds) - _lastGuardWindowOpenedAtSeconds);
        foreach (ParryToken candidate in _parryTokens)
        {
            if (candidate.PlayerId != targetPlayerId || candidate.PromptId != _activePromptId)
            {
                continue;
            }

            if (candidate.PressElapsedSeconds < 0d || candidate.PressElapsedSeconds > damageElapsedSeconds)
            {
                continue;
            }

            if (damageElapsedSeconds - candidate.PressElapsedSeconds <= windowSeconds)
            {
                token = candidate;
                return true;
            }
        }

        token = default;
        return false;
    }

    private static void SendLocalParryMessage(double now)
    {
        RunManager runManager = RunManager.Instance;
        if (!runManager.IsInProgress ||
            runManager.NetService == null ||
            !runManager.NetService.Type.IsMultiplayer() ||
            !runManager.NetService.IsConnected)
        {
            return;
        }

        runManager.NetService.SendMessage(new WitchIslandExpeditionParryMessage
        {
            promptId = _activePromptId,
            pressElapsedSeconds = now - _lastGuardWindowOpenedAtSeconds,
            Location = runManager.RunLocationTargetedBuffer.CurrentLocation,
        });
    }

    private static void EnsureNetworkHandler()
    {
        RunManager runManager = RunManager.Instance;
        if (!runManager.IsInProgress ||
            runManager.NetService == null ||
            !runManager.NetService.Type.IsMultiplayer() ||
            runManager.RunLocationTargetedBuffer == null)
        {
            return;
        }

        if (ReferenceEquals(_registeredMessageBuffer, runManager.RunLocationTargetedBuffer))
        {
            return;
        }

        if (_registeredMessageBuffer != null)
        {
            _registeredMessageBuffer.UnregisterMessageHandler(ParryMessageHandler);
            _registeredMessageBuffer.UnregisterMessageHandler(ParryResolutionMessageHandler);
        }

        _registeredMessageBuffer = runManager.RunLocationTargetedBuffer;
        _registeredMessageBuffer.RegisterMessageHandler(ParryMessageHandler);
        _registeredMessageBuffer.RegisterMessageHandler(ParryResolutionMessageHandler);
    }

    private static void HandleParryMessage(WitchIslandExpeditionParryMessage message, ulong senderId)
    {
        if (!_guardWindowTargetIds.Contains(senderId) || message.promptId != _activePromptId)
        {
            return;
        }

        _parryTokens.Add(new ParryToken(senderId, message.promptId, message.pressElapsedSeconds));
    }

    private static void HandleParryResolutionMessage(WitchIslandExpeditionParryResolutionMessage message, ulong _)
    {
        ApplyParryResolution(message.promptId, message.parriedPlayerIds);
    }

    private static void MarkAutoGuardTarget(ulong targetPlayerId)
    {
        if (_currentDamageCommand == null)
        {
            return;
        }

        if (!_autoGuardTargetsByCommand.TryGetValue(_currentDamageCommand, out HashSet<ulong>? autoGuardTargets))
        {
            autoGuardTargets = [];
            _autoGuardTargetsByCommand[_currentDamageCommand] = autoGuardTargets;
        }

        autoGuardTargets.Add(targetPlayerId);
    }

    private static IEnumerable<ulong> GetValidParryTargetIds()
    {
        double damageElapsedSeconds = Math.Max(0d, _lastGuardWindowClosedAtSeconds - _lastGuardWindowOpenedAtSeconds);
        foreach (ParryToken candidate in _parryTokens)
        {
            if (candidate.PromptId != _activePromptId || !_guardWindowTargetIds.Contains(candidate.PlayerId))
            {
                continue;
            }

            if (candidate.PressElapsedSeconds < 0d || candidate.PressElapsedSeconds > damageElapsedSeconds)
            {
                continue;
            }

            if (damageElapsedSeconds - candidate.PressElapsedSeconds <= DefaultWindowSeconds)
            {
                yield return candidate.PlayerId;
            }
        }
    }

    private static void ApplyParryResolution(uint promptId, IEnumerable<ulong> parriedPlayerIds)
    {
        if (promptId != _activePromptId)
        {
            return;
        }

        _resolvedPromptId = promptId;
        _resolvedParryTargetIds.Clear();
        _resolvedParryTargetIds.UnionWith(parriedPlayerIds.Where(_guardWindowTargetIds.Contains));
        _resolutionReceivedSource?.TrySetResult(promptId);
    }

    private static bool IsMultiplayerRun()
    {
        RunManager runManager = RunManager.Instance;
        return runManager.IsInProgress &&
            runManager.NetService != null &&
            runManager.NetService.Type.IsMultiplayer() &&
            runManager.NetService.IsConnected;
    }

    private static double NowSeconds()
    {
        return Time.GetTicksUsec() / 1_000_000d;
    }
}
