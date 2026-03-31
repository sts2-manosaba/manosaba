using Godot;
using HarmonyLib;
using Manosaba;
using Manosaba.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using System.Collections.Generic;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NCharacterSelectScreen_Input_AprilFoolsToggle
{
    // Runtime node names used by this patch-generated UI.
    private const string PanelName = "ManosabaAprilFoolsPanel";
    private const string LabelName = "StatusLabel";
    private const string ButtonName = "ToggleButton";
    // Track which screen instances already registered network handlers.
    private static readonly HashSet<ulong> RegisteredScreens = [];
    // Cache last host broadcast state per screen to avoid redundant network sends.
    private static readonly Dictionary<ulong, bool> LastBroadcastModeByScreen = [];
    private static readonly Dictionary<ulong, int> LastBroadcastPlayerCountByScreen = [];

    // Harmony: after character-select _Ready, create UI and sync first text state.
    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Ready))]
    [HarmonyPostfix]
    public static void ReadyPostfix(NCharacterSelectScreen __instance)
    {
        ManosabaFeatureFlags.TryApplyQuickWitCardDescription();
        EnsureUi(__instance);
        RefreshUi(__instance);
    }

    // Harmony: every frame while character select is active.
    // Keeps network handler and host/client state synchronization healthy.
    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Process))]
    [HarmonyPostfix]
    public static void ProcessPostfix(NCharacterSelectScreen __instance)
    {
        // Register network handler lazily because lobby may be null during early _Ready.
        TryRegisterNetworkHandler(__instance);
        // Host continuously ensures late-joining clients get current mode without manual retoggle.
        TryBroadcastCurrentModeFromHost(__instance);
        RefreshUi(__instance);
    }

    // Harmony: before character-select closes, clean per-screen registration/cache.
    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuClosed))]
    [HarmonyPrefix]
    public static void OnSubmenuClosedPrefix(NCharacterSelectScreen __instance)
    {
        TryUnregisterNetworkHandler(__instance);
        ulong id = __instance.GetInstanceId();
        LastBroadcastModeByScreen.Remove(id);
        LastBroadcastPlayerCountByScreen.Remove(id);
    }

    // Harmony: keyboard input hook; F8 toggles mode when allowed.
    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Input))]
    [HarmonyPostfix]
    public static void InputPostfix(NCharacterSelectScreen __instance, InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey keyEvent)
            return;

        if (keyEvent.Echo || keyEvent.Pressed)
            return;

        // Toggle April Fools mode directly in character select.
        if (keyEvent.Keycode != Key.F8)
            return;

        ToggleIfAllowed(__instance, "F8");
        RefreshUi(__instance);
    }

    // Build the top-right status panel once per screen instance.
    private static void EnsureUi(NCharacterSelectScreen screen)
    {
        // Create lightweight runtime UI instead of editing game scene assets.
        if (screen.GetNodeOrNull<Control>(PanelName) != null)
            return;

        Control panel = new()
        {
            Name = PanelName,
            MouseFilter = Control.MouseFilterEnum.Stop,
            AnchorLeft = 1,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 0,
            OffsetLeft = -420,
            OffsetTop = 16,
            OffsetRight = -16,
            OffsetBottom = 108
        };

        Label label = new()
        {
            Name = LabelName,
            HorizontalAlignment = HorizontalAlignment.Right,
            Position = new Vector2(0, 0),
            Size = new Vector2(404, 60)
        };

        Button button = new()
        {
            Name = ButtonName,
            Position = new Vector2(248, 64),
            Size = new Vector2(156, 32),
            Text = "Toggle (F8)"
        };
        button.Pressed += () =>
        {
            ToggleIfAllowed(screen, "mouse");
            RefreshUi(screen);
        };

        panel.AddChild(label);
        panel.AddChild(button);
        screen.AddChild(panel);
    }

    // Refresh label text, permission state, and button tooltip.
    private static void RefreshUi(NCharacterSelectScreen screen)
    {
        Control? panel = screen.GetNodeOrNull<Control>(PanelName);
        if (panel == null)
            return;

        Label? label = panel.GetNodeOrNull<Label>(LabelName);
        Button? button = panel.GetNodeOrNull<Button>(ButtonName);
        if (label == null || button == null)
            return;

        bool canToggle = CanToggle(screen);
        string mode = ManosabaFeatureFlags.AprilFoolsModeEnabled ? "ON" : "OFF";
        label.Text = $"April Fools: {mode}\nPress F8 or click button to toggle";
        button.Disabled = !canToggle;
        if (!canToggle)
            button.TooltipText = "Only singleplayer or multiplayer host can toggle.";
        else
            button.TooltipText = string.Empty;
    }

    // Permission gate: only singleplayer or host can toggle.
    // During very early init (Lobby null), allow local toggle to avoid dead key.
    private static bool CanToggle(NCharacterSelectScreen screen)
    {
        // Character select can receive input very early; allow local toggle before lobby is initialized.
        if (screen.Lobby == null)
            return true;

        NetGameType netType = screen.Lobby.NetService.Type;
        return netType == NetGameType.Singleplayer || netType == NetGameType.Host;
    }

    // Common toggle entry for both keyboard and mouse actions.
    private static void ToggleIfAllowed(NCharacterSelectScreen screen, string source)
    {
        if (!CanToggle(screen))
        {
            Log.Info("[Manosaba] April Fools mode unchanged (only host/singleplayer can toggle).");
            return;
        }

        bool enabled = ManosabaFeatureFlags.ToggleAprilFoolsMode();
        SyncUiModeToClientsIfHost(screen, enabled);
        Log.Info($"[Manosaba] April Fools mode: {(enabled ? "ON" : "OFF")} (toggled by {source} on character select)");
    }

    // Register client/host message receiver once Lobby.NetService becomes available.
    private static void TryRegisterNetworkHandler(NCharacterSelectScreen screen)
    {
        if (screen.Lobby?.NetService == null)
            return;

        ulong id = screen.GetInstanceId();
        if (RegisteredScreens.Contains(id))
            return;

        screen.Lobby.NetService.RegisterMessageHandler<AprilFoolsModeChangedMessage>(HandleModeChangedMessage);
        RegisteredScreens.Add(id);
    }

    // Remove message receiver during screen teardown.
    private static void TryUnregisterNetworkHandler(NCharacterSelectScreen screen)
    {
        if (screen.Lobby?.NetService == null)
            return;

        ulong id = screen.GetInstanceId();
        if (!RegisteredScreens.Contains(id))
            return;

        screen.Lobby.NetService.UnregisterMessageHandler<AprilFoolsModeChangedMessage>(HandleModeChangedMessage);
        RegisteredScreens.Remove(id);
    }

    // Host-only sender for April Fools UI mode sync packet.
    private static void SyncUiModeToClientsIfHost(NCharacterSelectScreen screen, bool enabled)
    {
        if (screen.Lobby == null || screen.Lobby.NetService.Type != NetGameType.Host)
            return;

        // UI-state sync only; gameplay logic still reads local feature flag at runtime.
        screen.Lobby.NetService.SendMessage(new AprilFoolsModeChangedMessage
        {
            enabled = enabled
        });

        ulong id = screen.GetInstanceId();
        LastBroadcastModeByScreen[id] = enabled;
        LastBroadcastPlayerCountByScreen[id] = screen.Lobby.Players.Count;
    }

    // Network callback: apply mode pushed by host.
    private static void HandleModeChangedMessage(AprilFoolsModeChangedMessage message, ulong _)
    {
        // Client mirrors host mode for consistent character-select display.
        ManosabaFeatureFlags.SetAprilFoolsMode(message.enabled);
    }

    // Host watchdog:
    // - first frame broadcast
    // - mode changed broadcast
    // - player-count changed broadcast (join/leave)
    private static void TryBroadcastCurrentModeFromHost(NCharacterSelectScreen screen)
    {
        if (screen.Lobby == null || screen.Lobby.NetService.Type != NetGameType.Host)
            return;

        ulong id = screen.GetInstanceId();
        bool currentMode = ManosabaFeatureFlags.AprilFoolsModeEnabled;
        int currentPlayers = screen.Lobby.Players.Count;

        bool modeKnown = LastBroadcastModeByScreen.TryGetValue(id, out bool lastMode);
        bool playerCountKnown = LastBroadcastPlayerCountByScreen.TryGetValue(id, out int lastPlayerCount);

        // Broadcast on first host frame, when mode changed, or when players changed (new client joined/left).
        if (!modeKnown || !playerCountKnown || lastMode != currentMode || lastPlayerCount != currentPlayers)
        {
            SyncUiModeToClientsIfHost(screen, currentMode);
        }
    }
}
