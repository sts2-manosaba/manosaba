using Godot;
using Manosaba.Config;
using Manosaba.Multiplayer;
using Manosaba.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using System.Runtime.CompilerServices;

namespace Manosaba.Patches;

/// <summary>Panel rectangle relative to <see cref="Control.SetAnchorsPreset"/> corner.</summary>
public readonly record struct LobbyDifficultyPanelLayout(
    Control.LayoutPreset Anchor,
    float OffsetLeft,
    float OffsetRight,
    float OffsetTop,
    float OffsetBottom)
{
    public static LobbyDifficultyPanelLayout CharacterSelectDefault { get; } = new(
        Control.LayoutPreset.TopRight,
        OffsetLeft: -430f,
        OffsetRight: -12f,
        OffsetTop: 72f,
        OffsetBottom: 400f);

    /// <summary>Top-left strip so it does not cover vanilla top-right multiplayer UI on custom run.</summary>
    public static LobbyDifficultyPanelLayout CustomRunDefault { get; } = new(
        Control.LayoutPreset.TopLeft,
        OffsetLeft: 12f,
        OffsetRight: 430f,
        OffsetTop: 72f,
        OffsetBottom: 400f);

    /// <summary>Bottom-right so daily timer / embark area stays readable.</summary>
    public static LobbyDifficultyPanelLayout DailyRunDefault { get; } = new(
        Control.LayoutPreset.BottomRight,
        OffsetLeft: -430f,
        OffsetRight: -12f,
        OffsetTop: -340f,
        OffsetBottom: -12f);
}

public enum LobbyDifficultyUiEnterKind
{
    /// <summary>First lobby UI setup: reset lobby defaults from config.</summary>
    FirstOpen,
    /// <summary>Submenu reopened: clear run snapshot only (standard/custom character flow).</summary>
    SubmenuReopened,
    /// <summary>Daily lobby became ready after async init (no reset defaults — same session).</summary>
    DailyLobbyReady,
}

public static class ManosabaLobbyDifficultyUiHost
{
    private sealed class DifficultyUiState
    {
        public PanelContainer? Root;
        public Label? TitleLabel;
        public Label? EnemyHpRowLabel;
        public HSlider? EnemyHpSlider;
        public Label? EnemyAttackRowLabel;
        public HSlider? EnemyAttackSlider;
        public HSlider? MurderousSlider;
        public Label? MurderousRowLabel;
        public Label? EnemyHpValueLabel;
        public Label? EnemyAttackValueLabel;
        public Label? MurderousValueLabel;
        public Label? RandomPoolRowLabel;
        public OptionButton? RandomPoolOption;

        public MessageHandlerDelegate<ManosabaDifficultySettingsMessage>? Handler;
        public bool HandlerRegistered;

        public int LastSentPlayerCount = int.MinValue;
        public double LastSentEnemyHpPercent;
        public double LastSentEnemyAttackPercent;
        public double LastSentMurderousPercent;
        public byte LastSentRandomPool;
    }

    private static readonly ConditionalWeakTable<object, DifficultyUiState> States = new();

    private static DifficultyUiState GetState(object owner) => States.GetOrCreateValue(owner);

    public static void OnEnter(
        object owner,
        Node ownerNode,
        Control attachParent,
        Func<StartRunLobby?> getLobby,
        LobbyDifficultyPanelLayout layout,
        LobbyDifficultyUiEnterKind kind)
    {
        switch (kind)
        {
            case LobbyDifficultyUiEnterKind.FirstOpen:
                ManosabaLobbyDifficultyState.ResetToLobbyDefaults();
                ManosabaLobbyDifficultyState.SetLobbySessionActive(true);
                break;
            case LobbyDifficultyUiEnterKind.SubmenuReopened:
                ManosabaLobbyDifficultyState.ClearRunSnapshot();
                break;
            case LobbyDifficultyUiEnterKind.DailyLobbyReady:
                ManosabaLobbyDifficultyState.ResetToLobbyDefaults();
                ManosabaLobbyDifficultyState.SetLobbySessionActive(true);
                break;
        }

        EnsurePanel(owner, ownerNode, attachParent, getLobby, layout);
        ApplyStaticTexts(owner);
        RefreshUiFromState(owner, ownerNode, getLobby, pushLocalToRuntime: false);
        TryEarlyLobbyHook(owner, ownerNode, getLobby);
    }

    public static void TryEarlyLobbyHook(object owner, Node ownerNode, Func<StartRunLobby?> getLobby)
    {
        StartRunLobby? lobby = getLobby();
        if (lobby == null || !lobby.NetService.IsConnected)
        {
            return;
        }

        DifficultyUiState state = GetState(owner);
        TryRegisterHandler(owner, ownerNode, lobby, state, getLobby);

        if (lobby.NetService.Type == NetGameType.Host)
        {
            TryHostWatchdogBroadcast(lobby, state, force: true);
        }
    }

    public static void OnProcess(object owner, Node ownerNode, Func<StartRunLobby?> getLobby)
    {
        StartRunLobby? lobby = getLobby();
        if (lobby == null || !lobby.NetService.IsConnected)
        {
            return;
        }

        DifficultyUiState state = GetState(owner);
        TryRegisterHandler(owner, ownerNode, lobby, state, getLobby);

        if (CanEditDifficulty(lobby))
        {
            PushUiToRuntime(owner, getLobby, state);
        }

        TryHostWatchdogBroadcast(lobby, state);
    }

    public static void OnCleanup(object owner, Func<StartRunLobby?> getLobby)
    {
        UnregisterHandlerIfNeeded(owner, getLobby);
        ManosabaLobbyDifficultyState.SetLobbySessionActive(false);
        if (States.TryGetValue(owner, out DifficultyUiState? st))
        {
            st.Root?.QueueFree();
            st.Root = null;
            st.TitleLabel = null;
            st.EnemyHpRowLabel = null;
            st.EnemyHpSlider = null;
            st.EnemyAttackRowLabel = null;
            st.EnemyAttackSlider = null;
            st.MurderousSlider = null;
            st.MurderousRowLabel = null;
            st.EnemyHpValueLabel = null;
            st.EnemyAttackValueLabel = null;
            st.MurderousValueLabel = null;
            st.RandomPoolRowLabel = null;
            st.RandomPoolOption = null;
            st.HandlerRegistered = false;
        }
    }

    private static void ApplyStaticTexts(object owner)
    {
        if (!States.TryGetValue(owner, out DifficultyUiState? state))
        {
            return;
        }

        if (state.TitleLabel != null)
        {
            state.TitleLabel.Text = T("MANOSABA-LOBBY_SETTINGS.title");
        }

        if (state.EnemyHpRowLabel != null)
        {
            state.EnemyHpRowLabel.Text = T("MANOSABA-LOBBY_SETTINGS.enemy_hp_percent");
        }

        if (state.EnemyAttackRowLabel != null)
        {
            state.EnemyAttackRowLabel.Text = T("MANOSABA-LOBBY_SETTINGS.enemy_attack_percent");
        }

        if (state.MurderousRowLabel != null)
        {
            state.MurderousRowLabel.Text = T("MANOSABA-LOBBY_SETTINGS.murderous_percent");
        }

        if (state.RandomPoolRowLabel != null)
        {
            state.RandomPoolRowLabel.Text = T("MANOSABA-LOBBY_SETTINGS.random_pool");
        }

        if (state.RandomPoolOption != null)
        {
            state.RandomPoolOption.Clear();
            state.RandomPoolOption.AddItem(T("MANOSABA-LOBBY_SETTINGS.random_pool_manosaba_only"));
            state.RandomPoolOption.AddItem(T("MANOSABA-LOBBY_SETTINGS.random_pool_all"));
        }
    }

    private static string T(string key) => new LocString("settings_ui", key).GetRawText() ?? key;

    private static void EnsurePanel(
        object owner,
        Node ownerNode,
        Control attachParent,
        Func<StartRunLobby?> getLobby,
        LobbyDifficultyPanelLayout layout)
    {
        DifficultyUiState state = GetState(owner);
        if (state.Root != null && GodotObject.IsInstanceValid(state.Root))
        {
            return;
        }

        var panel = new PanelContainer { Name = "ManosabaLobbyDifficultyPanel" };
        panel.MouseFilter = Control.MouseFilterEnum.Stop;
        panel.SetAnchorsPreset(layout.Anchor);
        panel.OffsetLeft = layout.OffsetLeft;
        panel.OffsetRight = layout.OffsetRight;
        panel.OffsetTop = layout.OffsetTop;
        panel.OffsetBottom = layout.OffsetBottom;

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);
        margin.AddChild(vbox);

        var title = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        vbox.AddChild(title);

        var enemyRow = new HBoxContainer();
        enemyRow.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(enemyRow);
        var enemyLbl = new Label { CustomMinimumSize = new Vector2(140, 0) };
        enemyRow.AddChild(enemyLbl);
        var enemySlider = new HSlider
        {
            MinValue = 100,
            MaxValue = 400,
            Step = 5,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        enemySlider.ValueChanged += _ => MarkHostDirty(owner, ownerNode, getLobby);
        enemyRow.AddChild(enemySlider);
        var enemyVal = new Label { CustomMinimumSize = new Vector2(48, 0) };
        enemyRow.AddChild(enemyVal);

        var attackRow = new HBoxContainer();
        attackRow.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(attackRow);
        var attackLbl = new Label { CustomMinimumSize = new Vector2(140, 0) };
        attackRow.AddChild(attackLbl);
        var attackSlider = new HSlider
        {
            MinValue = 100,
            MaxValue = 400,
            Step = 5,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        attackSlider.ValueChanged += _ => MarkHostDirty(owner, ownerNode, getLobby);
        attackRow.AddChild(attackSlider);
        var attackVal = new Label { CustomMinimumSize = new Vector2(48, 0) };
        attackRow.AddChild(attackVal);

        var murderRow = new HBoxContainer();
        murderRow.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(murderRow);
        var murderLbl = new Label { CustomMinimumSize = new Vector2(140, 0) };
        murderRow.AddChild(murderLbl);
        var murderSlider = new HSlider
        {
            MinValue = 0,
            MaxValue = 100,
            Step = 5,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        murderSlider.ValueChanged += _ => MarkHostDirty(owner, ownerNode, getLobby);
        murderRow.AddChild(murderSlider);
        var murderVal = new Label { CustomMinimumSize = new Vector2(48, 0) };
        murderRow.AddChild(murderVal);

        var randomRow = new HBoxContainer();
        randomRow.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(randomRow);
        var randomLbl = new Label { CustomMinimumSize = new Vector2(140, 0) };
        randomRow.AddChild(randomLbl);
        var randomOpt = new OptionButton
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        randomOpt.ItemSelected += _ => MarkHostDirty(owner, ownerNode, getLobby);
        randomRow.AddChild(randomOpt);

        attachParent.AddChild(panel);
        state.Root = panel;
        state.TitleLabel = title;
        state.EnemyHpRowLabel = enemyLbl;
        state.EnemyHpSlider = enemySlider;
        state.EnemyAttackRowLabel = attackLbl;
        state.EnemyAttackSlider = attackSlider;
        state.MurderousRowLabel = murderLbl;
        state.MurderousSlider = murderSlider;
        state.EnemyHpValueLabel = enemyVal;
        state.EnemyAttackValueLabel = attackVal;
        state.MurderousValueLabel = murderVal;
        state.RandomPoolRowLabel = randomLbl;
        state.RandomPoolOption = randomOpt;

        ApplyStaticTexts(owner);
        ApplyEditableMask(owner, getLobby, state);
    }

    private static void MarkHostDirty(object owner, Node ownerNode, Func<StartRunLobby?> getLobby)
    {
        StartRunLobby? lobby = getLobby();
        if (lobby == null || !CanEditDifficulty(lobby))
        {
            return;
        }

        DifficultyUiState st = GetState(owner);
        PushUiToRuntime(owner, getLobby, st);
        TryHostWatchdogBroadcast(lobby, st, force: true);
    }

    private static void ApplyEditableMask(object owner, Func<StartRunLobby?> getLobby, DifficultyUiState state)
    {
        StartRunLobby? lobby = getLobby();
        bool editable = lobby != null && CanEditDifficulty(lobby);
        if (state.EnemyHpSlider != null)
        {
            state.EnemyHpSlider.Editable = editable;
        }

        if (state.EnemyAttackSlider != null)
        {
            state.EnemyAttackSlider.Editable = editable;
        }

        if (state.MurderousSlider != null)
        {
            state.MurderousSlider.Editable = editable;
        }

        if (state.RandomPoolOption != null)
        {
            state.RandomPoolOption.Disabled = !editable;
        }
    }

    private static bool CanEditDifficulty(StartRunLobby lobby)
    {
        NetGameType t = lobby.NetService.Type;
        return t == NetGameType.Singleplayer || t == NetGameType.Host;
    }

    private static void TryRegisterHandler(
        object owner,
        Node ownerNode,
        StartRunLobby lobby,
        DifficultyUiState state,
        Func<StartRunLobby?> getLobby)
    {
        if (state.HandlerRegistered)
        {
            return;
        }

        state.Handler ??= delegate(ManosabaDifficultySettingsMessage message, ulong _)
        {
            ManosabaLobbyDifficultyState.ApplyFromHost(message);
            if (GodotObject.IsInstanceValid(ownerNode))
            {
                RefreshUiFromState(owner, ownerNode, getLobby, pushLocalToRuntime: false);
            }
        };

        lobby.NetService.RegisterMessageHandler<ManosabaDifficultySettingsMessage>(state.Handler);
        state.HandlerRegistered = true;
    }

    private static void UnregisterHandlerIfNeeded(object owner, Func<StartRunLobby?> getLobby)
    {
        StartRunLobby? lobby = getLobby();
        if (!States.TryGetValue(owner, out DifficultyUiState? state))
        {
            return;
        }

        if (state.HandlerRegistered && lobby != null && state.Handler != null)
        {
            lobby.NetService.UnregisterMessageHandler<ManosabaDifficultySettingsMessage>(state.Handler);
        }

        state.HandlerRegistered = false;
    }

    private static ManosabaDifficultySettingsMessage BuildMessageFromLobby()
    {
        (double hpPct, double atkPct, double murPct, RandomCharacterPoolMode pool) = ManosabaLobbyDifficultyState.GetLobbySnapshot();
        return new ManosabaDifficultySettingsMessage
        {
            enemyHpMultiplierPercent = hpPct,
            enemyAttackDamageMultiplierPercent = atkPct,
            murderousImpulseAllyDamageMultiplierPercent = murPct,
            randomCharacterPoolMode = (byte)pool,
        };
    }

    private static void TryHostWatchdogBroadcast(StartRunLobby lobby, DifficultyUiState state, bool force = false)
    {
        if (lobby.NetService.Type == NetGameType.Client)
        {
            return;
        }

        int players = lobby.Players.Count;
        (double hpPct, double atkPct, double murPct, RandomCharacterPoolMode pool) = ManosabaLobbyDifficultyState.GetLobbySnapshot();
        byte poolByte = (byte)pool;
        bool changed = force
            || players != state.LastSentPlayerCount
            || Math.Abs(hpPct - state.LastSentEnemyHpPercent) > 0.0001d
            || Math.Abs(atkPct - state.LastSentEnemyAttackPercent) > 0.0001d
            || Math.Abs(murPct - state.LastSentMurderousPercent) > 0.0001d
            || poolByte != state.LastSentRandomPool;

        if (!changed)
        {
            return;
        }

        state.LastSentPlayerCount = players;
        state.LastSentEnemyHpPercent = hpPct;
        state.LastSentEnemyAttackPercent = atkPct;
        state.LastSentMurderousPercent = murPct;
        state.LastSentRandomPool = poolByte;

        ManosabaDifficultySettingsMessage message = BuildMessageFromLobby();

        if (lobby.NetService.Type == NetGameType.Singleplayer)
        {
            ManosabaLobbyDifficultyState.ApplyFromHost(message);
            return;
        }

        lobby.NetService.SendMessage(message);
    }

    private static void PushUiToRuntime(object owner, Func<StartRunLobby?> getLobby, DifficultyUiState state)
    {
        if (state.EnemyHpSlider == null || state.EnemyAttackSlider == null || state.MurderousSlider == null)
        {
            return;
        }

        RandomCharacterPoolMode pool = RandomCharacterPoolMode.ManosabaCharactersOnly;
        if (state.RandomPoolOption != null)
        {
            int sel = state.RandomPoolOption.Selected;
            pool = sel <= 0 ? RandomCharacterPoolMode.ManosabaCharactersOnly : RandomCharacterPoolMode.AllCharacters;
        }

        ManosabaLobbyDifficultyState.ApplyFromHost(
            state.EnemyHpSlider.Value,
            state.EnemyAttackSlider.Value,
            state.MurderousSlider.Value,
            pool);

        if (state.EnemyHpValueLabel != null)
        {
            state.EnemyHpValueLabel.Text = $"{state.EnemyHpSlider.Value:0}%";
        }

        if (state.EnemyAttackValueLabel != null)
        {
            state.EnemyAttackValueLabel.Text = $"{state.EnemyAttackSlider.Value:0}%";
        }

        if (state.MurderousValueLabel != null)
        {
            state.MurderousValueLabel.Text = $"{state.MurderousSlider.Value:0}%";
        }
    }

    private static void RefreshUiFromState(object owner, Node ownerNode, Func<StartRunLobby?> getLobby, bool pushLocalToRuntime)
    {
        if (!States.TryGetValue(owner, out DifficultyUiState? state)
            || state.EnemyHpSlider == null
            || state.EnemyAttackSlider == null)
        {
            return;
        }

        (double hpPct, double atkPct, double murPct, RandomCharacterPoolMode pool) = ManosabaLobbyDifficultyState.GetLobbySnapshot();
        state.EnemyHpSlider.Value = hpPct;
        state.EnemyAttackSlider.Value = atkPct;
        if (state.MurderousSlider != null)
        {
            state.MurderousSlider.Value = murPct;
        }

        if (state.RandomPoolOption != null)
        {
            state.RandomPoolOption.SetBlockSignals(true);
            state.RandomPoolOption.Selected = pool == RandomCharacterPoolMode.AllCharacters ? 1 : 0;
            state.RandomPoolOption.SetBlockSignals(false);
        }

        if (state.EnemyHpValueLabel != null)
        {
            state.EnemyHpValueLabel.Text = $"{state.EnemyHpSlider.Value:0}%";
        }

        if (state.EnemyAttackValueLabel != null)
        {
            state.EnemyAttackValueLabel.Text = $"{state.EnemyAttackSlider.Value:0}%";
        }

        if (state.MurderousValueLabel != null && state.MurderousSlider != null)
        {
            state.MurderousValueLabel.Text = $"{state.MurderousSlider.Value:0}%";
        }

        ApplyEditableMask(owner, getLobby, state);

        if (pushLocalToRuntime)
        {
            PushUiToRuntime(owner, getLobby, state);
        }
    }
}
