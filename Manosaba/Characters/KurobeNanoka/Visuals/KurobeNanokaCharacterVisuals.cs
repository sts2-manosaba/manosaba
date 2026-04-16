using Godot;
using Manosaba.Characters.KurobeNanoka.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Characters.KurobeNanoka.Visuals;

public sealed partial class KurobeNanokaCharacterVisuals : NCreatureVisuals
{
    private const string DarkSightModelScenePath = "res://Manosaba/scenes/kurobe_nanoka/kurobe_nanoka_darksight.tscn";

    private Creature? _creature;
    private CanvasItem? _normalVisuals;
    private Node2D? _darkSightInstance;
    private bool _lastHadDarkSight;

    public override void _Ready()
    {
        base._Ready();

        _normalVisuals = GetNodeOrNull<CanvasItem>("%Visuals");

        var creatureNode = GetParent() as NCreature;
        _creature = creatureNode?.Entity;
        _lastHadDarkSight = HasDarkSight();

        ApplyVisualState(_lastHadDarkSight);
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
        if (hasDarkSight == _lastHadDarkSight)
        {
            return;
        }

        _lastHadDarkSight = hasDarkSight;
        ApplyVisualState(hasDarkSight);
    }

    private bool HasDarkSight()
    {
        return _creature != null && _creature.GetPowerAmount<DarkSightPower>() > 0;
    }

    private void ApplyVisualState(bool hasDarkSight)
    {
        if (_normalVisuals != null)
        {
            _normalVisuals.Visible = !hasDarkSight;
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
    }
}

