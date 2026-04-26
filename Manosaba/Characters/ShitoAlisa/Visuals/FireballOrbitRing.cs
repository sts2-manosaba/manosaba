using Godot;
using Manosaba.Characters.ShitoAlisa.Powers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace Manosaba.Characters.ShitoAlisa.Visuals;

/// <summary>
/// One orbiting sprite on the baked circle path; scale grows with stack count up to <see cref="MaxOrbs"/>.
/// Draws above <see cref="NCreature.Visuals"/> but <b>below</b> <see cref="NCreature.OrbManager"/> so player orb slots stay readable.
/// </summary>
public partial class FireballOrbitRing : Node2D
{
    public const int MaxOrbs = 12;

    private const float OrbitRadius = 236f;
    private const float OrbitSpeed = 60f;
    private const float BobAmplitude = 8f;
    private const float BobSpeed = 2.1f;
    private const int CurveBakeSegments = 48;

    /// <summary>Sprite scale at 1 stack (matches prior per-orb size).</summary>
    private const float MinOrbSpriteScale = 0.42f;

    /// <summary>Sprite scale at <see cref="MaxOrbs"/> stacks.</summary>
    private const float MaxOrbSpriteScale = 1.05f;

    private const float ScaleLerpSpeed = 7f;

    private readonly Curve2D _orbitCurve = new();
    private FireballOrbitUnit? _unit;

    private Creature? _owner;
    private NCreature? _creatureNode;
    private double _orbitProgress;
    private int _visibleCount;
    private int _hoverDepth;
    private float _spriteScaleDisplay = MinOrbSpriteScale;
    private float _spriteScaleTarget = MinOrbSpriteScale;

    public FireballOrbitRing()
    {
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
        // Same Z as NOrbManager (0): sibling index vs OrbManager decides stacking; ZIndex 1 would always paint above energy orbs.
        ZIndex = 0;

        _unit = new FireballOrbitUnit(this);
        _unit.Visible = false;
        AddChild(_unit);

        if (_owner != null)
            _owner.Died += OnOwnerDied;
    }

    public override void _ExitTree()
    {
        if (_owner != null)
            _owner.Died -= OnOwnerDied;

        _unit?.ReleaseHoverTip();

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
        if (_unit != null)
            _unit.Visible = _visibleCount > 0;

        _spriteScaleTarget = SpriteScaleForStackCount(_visibleCount);
        if (_visibleCount <= 0)
            _spriteScaleDisplay = MinOrbSpriteScale;
    }

    private static float SpriteScaleForStackCount(int stacks)
    {
        if (stacks <= 0)
            return MinOrbSpriteScale;
        float t = (stacks - 1) / (float)Math.Max(1, MaxOrbs - 1);
        return Mathf.Lerp(MinOrbSpriteScale, MaxOrbSpriteScale, t);
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

    private void OnOwnerDied(Creature _)
    {
        _unit?.ReleaseHoverTip();
    }

    /// <summary>
    /// Keep the ring directly under <see cref="NCreature.OrbManager"/> in the sibling list so it draws behind energy orbs
    /// (Godot: lower index = earlier draw). Monsters have no orb manager → keep on top of other creature children.
    /// </summary>
    private void EnsureSiblingDrawOrderUnderOrbs()
    {
        if (_creatureNode == null || GetParent() != _creatureNode)
            return;

        NOrbManager? orbManager = _creatureNode.OrbManager;
        if (orbManager != null && GodotObject.IsInstanceValid(orbManager) && orbManager.GetParent() == _creatureNode)
        {
            int orbIdx = orbManager.GetIndex();
            int ringIdx = GetIndex();
            if (ringIdx > orbIdx)
                _creatureNode.MoveChild(this, orbIdx);
            else if (ringIdx < orbIdx - 1)
                _creatureNode.MoveChild(this, orbIdx - 1);
            return;
        }

        int top = _creatureNode.GetChildCount() - 1;
        if (top >= 0 && GetIndex() != top)
            _creatureNode.MoveChild(this, top);
    }

    public override void _Process(double delta)
    {
        if (_creatureNode == null || _owner == null || !GodotObject.IsInstanceValid(_creatureNode))
            return;

        if (!GodotObject.IsInstanceValid(_creatureNode.Visuals))
            return;

        Marker2D spawn = _creatureNode.Visuals.VfxSpawnPosition;
        if (!GodotObject.IsInstanceValid(spawn))
            return;

        EnsureSiblingDrawOrderUnderOrbs();

        if (IsCapstoneBlocking())
        {
            Visible = false;
            _unit?.ReleaseHoverTip();
            return;
        }

        Visible = _visibleCount > 0;

        GlobalPosition = spawn.GlobalPosition;

        float bakedLength = _orbitCurve.GetBakedLength();
        if (bakedLength <= 0f || _visibleCount <= 0 || _unit == null)
            return;

        _spriteScaleDisplay = Mathf.Lerp(_spriteScaleDisplay, _spriteScaleTarget, (float)(1f - Math.Exp(-ScaleLerpSpeed * delta)));
        _unit.SetSpriteUniformScale(_spriteScaleDisplay);

        if (_hoverDepth <= 0)
            _orbitProgress -= OrbitSpeed * delta / bakedLength;

        double lead = OrbitPhase01(_orbitProgress);
        double p = OrbitPhase01(lead);
        Transform2D xf = _orbitCurve.SampleBakedWithRotation((float)p * bakedLength);
        Vector2 pos = xf.Origin;
        float t = (float)Time.GetTicksMsec() / 1000f;
        float bob = Mathf.Sin(t * BobSpeed) * BobAmplitude;
        pos += Vector2.Up * bob;
        _unit.UpdateOrbitPosition(pos);
    }

    internal FireballSwarmPower? GetSwarmPower() => _owner?.GetPower<FireballSwarmPower>();

    internal bool IsLocalOwner() => LocalContext.IsMe(_owner);

    internal bool ShouldAllowHoverTip() =>
        !IsHandInCardPlay()
        && NTargetManager.Instance != null
        && !NTargetManager.Instance.IsInSelection;

    private static double OrbitPhase01(double progress) => progress - Math.Floor(progress);
}

/// <summary>Single orb: sprite + hitbox for tooltips (vanilla sovereign blade pattern).</summary>
public partial class FireballOrbitUnit : Node2D
{
    private const float HitboxSize = 72f;

    private readonly FireballOrbitRing _ring;
    private readonly Sprite2D _sprite = new();
    private readonly Control _hitbox = new();
    private NHoverTipSet? _hoverTip;
    private bool _focused;

    public FireballOrbitUnit(FireballOrbitRing ring)
    {
        _ring = ring;
    }

    public override void _Ready()
    {
        Texture2D? tex = ResourceLoader.Load<Texture2D>(FireballOrbitTexturePaths.OrbitSprite);
        if (tex != null)
            _sprite.Texture = tex;
        _sprite.Centered = true;
        _sprite.Scale = Vector2.One * 0.42f;
        AddChild(_sprite);

        _hitbox.CustomMinimumSize = new Vector2(HitboxSize, HitboxSize);
        _hitbox.Position = new Vector2(-HitboxSize * 0.5f, -HitboxSize * 0.5f);
        AddChild(_hitbox);

        _hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnHitboxMouseEntered));
        _hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnHitboxMouseExited));
        _hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnHitboxFocusEntered));
        _hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnHitboxFocusExited));
    }

    public void SetSpriteUniformScale(float uniformScale)
    {
        float s = Mathf.Max(0.05f, uniformScale);
        _sprite.Scale = new Vector2(s, s);
        float box = HitboxSize * Mathf.Clamp(s / 0.42f, 1f, 2.4f);
        _hitbox.CustomMinimumSize = new Vector2(box, box);
        _hitbox.Position = new Vector2(-box * 0.5f, -box * 0.5f);
    }

    public void UpdateOrbitPosition(Vector2 localPos)
    {
        Position = localPos;
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
