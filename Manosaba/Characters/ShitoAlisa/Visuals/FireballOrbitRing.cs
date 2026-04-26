using Godot;
using Manosaba.Characters.ShitoAlisa.Powers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace Manosaba.Characters.ShitoAlisa.Visuals;

/// <summary>
/// Regent <c>NSovereignBladeVfx</c> parity (see <c>_Process</c> there): sample a baked path with <c>SampleBakedWithRotation</c>,
/// advance progress by <c>60 * delta / bakedLength</c> (here subtracted so the procedural circle reads counter-clockwise like the sovereign path) while no hover tooltip.
/// Unlike sovereign (one blade), we do not <c>MoveChild</c> the ring to index 0 — that would draw every orb behind <see cref="NCreature.Visuals"/> and look like they vanish.
/// Differences: one shared <c>_orbitProgress</c> for all orbs with phase <c>i / N</c>; procedural circle (not authored Path2D); small Y bob; all orbs stay above <see cref="NCreature.Visuals"/> (no per-orb negative <c>ZIndex</c>);
/// Orbit center follows <see cref="NCreature.VfxSpawnPosition"/> each frame so the ring matches the character scene <c>%CenterPos</c>, not the Control origin at the feet.
/// no blade-damage X lerp toward +200 nor spine <c>Lerp</c> chase; any orb hover pauses the whole ring (shared counter).
/// </summary>
public partial class FireballOrbitRing : Node2D
{
    public const int MaxOrbs = 12;

    private const float OrbitRadius = 236f;
    private const float OrbitSpeed = 60f;
    private const float BobAmplitude = 8f;
    private const float BobSpeed = 2.1f;
    private const int CurveBakeSegments = 48;

    private readonly Curve2D _orbitCurve = new();
    private readonly FireballOrbitUnit[] _units = new FireballOrbitUnit[MaxOrbs];

    private Creature? _owner;
    private NCreature? _creatureNode;
    private double _orbitProgress;
    private int _visibleCount;
    private int _hoverDepth;

    private readonly Callable _targetingBeganCallable;
    private readonly Callable _targetingEndedCallable;

    public FireballOrbitRing()
    {
        _targetingBeganCallable = Callable.From(OnTargetingBegan);
        _targetingEndedCallable = Callable.From(OnTargetingEnded);
    }

    public static FireballOrbitRing GetOrCreate(NCreature creature)
    {
        foreach (Node child in creature.GetChildren())
        {
            if (child is FireballOrbitRing existing)
                return existing;
        }

        var ring = new FireballOrbitRing();
        creature.AddChildSafely(ring);
        ring.Position = Vector2.Zero;
        ring._creatureNode = creature;
        ring._owner = creature.Entity;
        ring.SetupCurve();
        return ring;
    }

    public override void _Ready()
    {
        // Positive z so orb subtree never sorts under NCreature siblings (e.g. Visuals) when units used path-based negative z.
        ZIndex = 1;

        for (int i = 0; i < MaxOrbs; i++)
        {
            var unit = new FireballOrbitUnit(this, i);
            unit.Visible = false;
            AddChild(unit);
            _units[i] = unit;
        }

        if (NTargetManager.Instance != null)
        {
            NTargetManager.Instance.Connect(NTargetManager.SignalName.TargetingBegan, _targetingBeganCallable);
            NTargetManager.Instance.Connect(NTargetManager.SignalName.TargetingEnded, _targetingEndedCallable);
        }

        if (_owner != null)
            _owner.Died += OnOwnerDied;
    }

    public override void _ExitTree()
    {
        if (_owner != null)
            _owner.Died -= OnOwnerDied;

        if (NTargetManager.Instance != null)
        {
            if (NTargetManager.Instance.IsConnected(NTargetManager.SignalName.TargetingBegan, _targetingBeganCallable))
                NTargetManager.Instance.Disconnect(NTargetManager.SignalName.TargetingBegan, _targetingBeganCallable);
            if (NTargetManager.Instance.IsConnected(NTargetManager.SignalName.TargetingEnded, _targetingEndedCallable))
                NTargetManager.Instance.Disconnect(NTargetManager.SignalName.TargetingEnded, _targetingEndedCallable);
        }

        foreach (FireballOrbitUnit u in _units)
            u.ReleaseHoverTip();

        base._ExitTree();
    }

    private void SetupCurve()
    {
        _orbitCurve.ClearPoints();
        for (int i = 0; i <= CurveBakeSegments; i++)
        {
            float a = Mathf.Tau * i / CurveBakeSegments;
            _orbitCurve.AddPoint(new Vector2(Mathf.Cos(a) * OrbitRadius, Mathf.Sin(a) * OrbitRadius));
        }

        _orbitCurve.BakeInterval = 4f;
    }

    public void SetVisibleCount(int count)
    {
        _visibleCount = Math.Clamp(count, 0, MaxOrbs);
        Visible = _visibleCount > 0;
        for (int i = 0; i < MaxOrbs; i++)
            _units[i].Visible = i < _visibleCount;
    }

    internal void NotifyUnitHoverEntered() => _hoverDepth++;

    internal void NotifyUnitHoverExited()
    {
        _hoverDepth = Math.Max(0, _hoverDepth - 1);
    }

    private static bool IsCapstoneBlocking() =>
        NCapstoneContainer.Instance != null && NCapstoneContainer.Instance.InUse;

    private static bool IsHandInCardPlay() =>
        NCombatRoom.Instance?.Ui.Hand.InCardPlay == true;

    private void OnTargetingBegan()
    {
        foreach (FireballOrbitUnit u in _units)
            u.SetHitboxMouseFilter(Control.MouseFilterEnum.Ignore);
    }

    private void OnTargetingEnded()
    {
        foreach (FireballOrbitUnit u in _units)
            u.SetHitboxMouseFilter(Control.MouseFilterEnum.Stop);
    }

    private void OnOwnerDied(Creature _)
    {
        foreach (FireballOrbitUnit u in _units)
        {
            u.SetHitboxMouseFilter(Control.MouseFilterEnum.Ignore);
            u.ReleaseHoverTip();
        }
    }

    public override void _Process(double delta)
    {
        if (_creatureNode == null || _owner == null || !GodotObject.IsInstanceValid(_creatureNode))
            return;

        if (IsCapstoneBlocking())
        {
            Visible = false;
            foreach (FireballOrbitUnit u in _units)
                u.ReleaseHoverTip();
            return;
        }

        Visible = _visibleCount > 0;

        // NCreature (Control) origin is often low on the sprite; CenterPos / VfxSpawn is the authored torso anchor.
        GlobalPosition = _creatureNode.VfxSpawnPosition;

        float bakedLength = _orbitCurve.GetBakedLength();
        if (bakedLength <= 0f || _visibleCount <= 0)
            return;

        if (_hoverDepth <= 0)
            _orbitProgress -= OrbitSpeed * delta / bakedLength;

        // C# % keeps sign of dividend; subtracting progress makes lead negative and breaks SampleBakedWithRotation.
        double lead = OrbitPhase01(_orbitProgress);

        // Keep the ring above creature art. Sovereign blade toggles MoveChild(0) for one sprite; doing that here hides all orbs behind the full portrait.
        int top = _creatureNode.GetChildCount() - 1;
        if (GetIndex() != top)
            _creatureNode.MoveChild(this, top);

        float t = (float)Time.GetTicksMsec() / 1000f;

        for (int i = 0; i < _visibleCount; i++)
        {
            double p = OrbitPhase01(lead + (double)i / _visibleCount);
            Transform2D xf = _orbitCurve.SampleBakedWithRotation((float)p * bakedLength);
            Vector2 pos = xf.Origin;
            float bob = Mathf.Sin(t * BobSpeed + i * 0.61f) * BobAmplitude;
            pos += Vector2.Up * bob;
            _units[i].UpdateOrbitPosition(pos);
        }
    }

    internal FireballSwarmPower? GetSwarmPower() => _owner?.GetPower<FireballSwarmPower>();

    internal bool IsLocalOwner() => LocalContext.IsMe(_owner);

    internal bool ShouldAllowHoverTip() =>
        !IsHandInCardPlay()
        && NTargetManager.Instance != null
        && !NTargetManager.Instance.IsInSelection;

    /// <summary>Fractional part in [0, 1) for any finite <paramref name="progress"/> (unlike C# <c>%</c> for negatives).</summary>
    private static double OrbitPhase01(double progress) => progress - Math.Floor(progress);
}

/// <summary>One orb: sprite + hitbox for tooltips (like sovereign blade hitbox).</summary>
public partial class FireballOrbitUnit : Node2D
{
    private const float HitboxSize = 72f;

    private readonly FireballOrbitRing _ring;
    private readonly Sprite2D _sprite = new();
    private readonly Control _hitbox = new();
    private NHoverTipSet? _hoverTip;
    private bool _focused;

    public FireballOrbitUnit(FireballOrbitRing ring, int _)
    {
        _ring = ring;
    }

    public override void _Ready()
    {
        Texture2D? tex = ResourceLoader.Load<Texture2D>(FireballOrbitTexturePaths.OrbitSprite);
        if (tex != null)
            _sprite.Texture = tex;
        _sprite.Centered = true;
        _sprite.Scale = new Vector2(0.42f, 0.42f);
        AddChild(_sprite);

        _hitbox.CustomMinimumSize = new Vector2(HitboxSize, HitboxSize);
        _hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
        _hitbox.Position = new Vector2(-HitboxSize * 0.5f, -HitboxSize * 0.5f);
        AddChild(_hitbox);

        _hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnHitboxMouseEntered));
        _hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnHitboxMouseExited));
        _hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnHitboxFocusEntered));
        _hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnHitboxFocusExited));
    }

    public void UpdateOrbitPosition(Vector2 localPos)
    {
        Position = localPos;
        // Keep other players' fireballs less visually dominant.
        _sprite.Modulate = _ring.IsLocalOwner() ? Colors.White : new Color(1f, 1f, 1f, 0.58f);
    }

    public void SetHitboxMouseFilter(Control.MouseFilterEnum filter) => _hitbox.MouseFilter = filter;

    public void ReleaseHoverTip()
    {
        if (_hoverTip != null)
        {
            NHoverTipSet.Remove(_hitbox);
            _hoverTip = null;
        }

        if (_focused)
        {
            _focused = false;
            _ring.NotifyUnitHoverExited();
        }
    }

    private void OnHitboxMouseEntered() => SetFocused(true);

    private void OnHitboxMouseExited() => SetFocused(false);

    private void OnHitboxFocusEntered() => SetFocused(true);

    private void OnHitboxFocusExited() => SetFocused(false);

    private void SetFocused(bool focused)
    {
        if (_focused == focused)
            return;

        if (_focused)
        {
            _ring.NotifyUnitHoverExited();
            if (_hoverTip != null)
            {
                NHoverTipSet.Remove(_hitbox);
                _hoverTip = null;
            }
        }

        _focused = focused;

        if (_focused)
            _ring.NotifyUnitHoverEntered();

        UpdateHoverTip();
    }

    private void UpdateHoverTip()
    {
        bool want = _focused
            && _ring.ShouldAllowHoverTip()
            && _hitbox.MouseFilter != Control.MouseFilterEnum.Ignore;

        if (want && _hoverTip == null)
        {
            FireballSwarmPower? power = _ring.GetSwarmPower();
            if (power == null)
                return;

            HoverTip tip = power.CreateOrbitHoverTip();
            _hoverTip = NHoverTipSet.CreateAndShow(_hitbox, tip);
            _hoverTip.GlobalPosition = _hitbox.GlobalPosition + Vector2.Right * _hitbox.Size.X;
        }
        else if (!want && _hoverTip != null)
        {
            NHoverTipSet.Remove(_hitbox);
            _hoverTip = null;
        }
    }
}
