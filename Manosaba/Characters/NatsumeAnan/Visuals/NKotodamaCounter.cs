using System;
using Godot;
using manosaba.Characters.NatsumeAnan;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Manosaba.Characters.NatsumeAnan.Visuals;

public sealed partial class NKotodamaCounter : Control
{
    private static readonly Vector2 HideDelta = new(-480f, 128f);

    private Player? _player;
    private RichTextLabel? _countLabel;
    private int _displayedValue = int.MinValue;
    private bool _showUntilCombatEnds;
    private Vector2? _showGlobalPosition;
    private Tween? _animInTween;
    private Tween? _animOutTween;
    private HoverTip? _hoverTip;

    public void Initialize(Player player)
    {
        _player = player;
        Refresh(force: true);
    }

    public void AnimIn()
    {
        _animOutTween?.Kill();
        _animInTween?.Kill();

        _showGlobalPosition ??= GlobalPosition;
        GlobalPosition = _showGlobalPosition.Value + HideDelta;
        _animInTween = CreateTween();
        _animInTween.TweenProperty(this, "global_position", _showGlobalPosition.Value, 0.6)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
    }

    public void AnimOut()
    {
        _animInTween?.Kill();
        _animOutTween?.Kill();

        _showGlobalPosition ??= GlobalPosition;
        _animOutTween = CreateTween();
        _animOutTween.TweenProperty(this, "global_position", _showGlobalPosition.Value + HideDelta, 0.6)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
    }

    public override void _Ready()
    {
        _countLabel = GetNodeOrNull<RichTextLabel>("%CountLabel") ??
                      GetNodeOrNull<RichTextLabel>("MarginContainer/CountLabel");
        _hoverTip = new HoverTip(
            new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.title"),
            new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.description"));
        Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
        Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
        Refresh(force: true);
    }

    public override void _Process(double delta)
    {
        _ = delta;
        Refresh(force: false);
    }

    private void Refresh(bool force)
    {
        if (_countLabel == null || _player == null)
        {
            return;
        }

        Player player = ResolveLocalPlayer();
        int value = KotodamaEnergy.Get(player);
        if (value > 0)
        {
            _showUntilCombatEnds = true;
        }

        Visible = _showUntilCombatEnds;
        if (!force && value == _displayedValue)
        {
            return;
        }

        _displayedValue = value;
        _countLabel.Text = $"[center]{value}[/center]";
        _countLabel.AddThemeColorOverride("default_color", value == 0 ? StsColors.red : StsColors.cream);
    }

    private Player ResolveLocalPlayer()
    {
        if (_player?.Creature?.CombatState is { } state)
        {
            try
            {
                Player? me = LocalContext.GetMe(state);
                if (me != null)
                {
                    _player = me;
                    return me;
                }
            }
            catch (Exception)
            {
                // Fall through to initialized player.
            }
        }

        return _player!;
    }

    private void OnHovered()
    {
        if (_hoverTip == null)
            return;

        NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
        nHoverTipSet.GlobalPosition = GlobalPosition + new Vector2(-70f, -200f);
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }
}
