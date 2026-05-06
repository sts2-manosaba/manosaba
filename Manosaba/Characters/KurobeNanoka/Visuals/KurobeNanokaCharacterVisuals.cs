using Godot;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.KurobeNanoka.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Characters.KurobeNanoka.Visuals;

public sealed partial class KurobeNanokaCharacterVisuals : NCreatureVisuals
{
    private const string DarkSightModelScenePath = "res://Manosaba/scenes/kurobe_nanoka/kurobe_nanoka_darksight.tscn";

    private Creature? _creature;
    private CanvasItem? _normalVisuals;
    private CanvasItem? _normalBaseVisual;
    private CanvasItem? _normalMajokaVisual;
    private Node2D? _darkSightInstance;
    private bool _lastHadDarkSight;
    private bool _lastHadMajoka;

    public override void _Ready()
    {
        base._Ready();

        _normalVisuals = GetNodeOrNull<CanvasItem>("%Visuals");
        _normalBaseVisual = GetNodeOrNull<CanvasItem>("Visuals/Sprite2D");
        _normalMajokaVisual = GetNodeOrNull<CanvasItem>("Visuals/Majoka");

        var creatureNode = GetParent() as NCreature;
        _creature = creatureNode?.Entity;
        _lastHadDarkSight = HasDarkSight();
        _lastHadMajoka = HasMajoka();

        ApplyVisualState(_lastHadDarkSight, _lastHadMajoka);
    }

    public override void _ExitTree()
    {
        if (_darkSightInstance != null && GodotObject.IsInstanceValid(_darkSightInstance))
        {
            _darkSightInstance.QueueFree();
        }

        _darkSightInstance = null;
        _creature = null;

        base._ExitTree();
    }

    public override void _Process(double delta)
    {
        _ = delta;

        bool hasDarkSight = HasDarkSight();
        bool hasMajoka = HasMajoka();
        if (hasDarkSight == _lastHadDarkSight && hasMajoka == _lastHadMajoka)
        {
            return;
        }

        _lastHadDarkSight = hasDarkSight;
        _lastHadMajoka = hasMajoka;
        ApplyVisualState(hasDarkSight, hasMajoka);
    }

    private bool HasDarkSight()
    {
        return _creature != null && _creature.GetPowerAmount<DarkSightPower>() > 0;
    }

    private bool HasMajoka()
    {
        return _creature != null && _creature.GetPowerAmount<MajokaPower>() >= 100;
    }

    private void ApplyVisualState(bool hasDarkSight, bool hasMajoka)
    {
        if (_normalVisuals != null)
        {
            _normalVisuals.Visible = !hasDarkSight;
        }

        if (_normalBaseVisual != null)
        {
            _normalBaseVisual.Visible = !hasMajoka;
        }

        if (_normalMajokaVisual != null)
        {
            _normalMajokaVisual.Visible = hasMajoka;
        }

        if (!hasDarkSight)
        {
            if (_darkSightInstance != null && GodotObject.IsInstanceValid(_darkSightInstance))
            {
                _darkSightInstance.Visible = false;
            }

            return;
        }

        if (_darkSightInstance == null || !GodotObject.IsInstanceValid(_darkSightInstance))
        {
            var packed = ResourceLoader.Load<PackedScene>(DarkSightModelScenePath);
            if (packed == null)
            {
                GD.PushWarning("[Manosaba] Missing DarkSight model scene: " + DarkSightModelScenePath);
                return;
            }

            _darkSightInstance = packed.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
            AddChild(_darkSightInstance);
        }

        _darkSightInstance.Visible = true;
        CanvasItem? darkBaseVisual = _darkSightInstance.GetNodeOrNull<CanvasItem>("Visuals/Sprite");
        CanvasItem? darkMajokaVisual = _darkSightInstance.GetNodeOrNull<CanvasItem>("Visuals/Majoka");
        if (darkBaseVisual != null)
        {
            darkBaseVisual.Visible = !hasMajoka;
        }

        if (darkMajokaVisual != null)
        {
            darkMajokaVisual.Visible = hasMajoka;
        }
    }
}

