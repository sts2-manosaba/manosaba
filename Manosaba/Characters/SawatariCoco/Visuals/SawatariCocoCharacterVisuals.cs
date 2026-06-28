using Godot;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using manosaba.Characters.SawatariCoco.Helper;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Manosaba.Characters.SawatariCoco.Visuals;

/// <summary>
/// Coco combat visuals with equipment sprite overlays (Coco scene only; other characters use equipment powers without overlay).
/// Godot scene naming (under sawatari_coco.tscn):
/// - Visuals/Sprite2D — base body
/// - Visuals/Majoka — majoka form (100+ stacks)
/// - Visuals/Equipment/Headwear|Top|Skirt|Shoes — normal equipment Sprite2D nodes
/// - Visuals/Majoka/Equipment/Headwear|Top|Skirt|Shoes — majoka equipment Sprite2D nodes
/// Texture paths: res://Manosaba/images/characters/sawatari_coco/equipment/{series}_{slot}[ _majoka].png
/// Example: punk_cat_headwear.png, cybercat_top_majoka.png
/// </summary>
public sealed partial class SawatariCocoCharacterVisuals : ManosabaCharacterVisuals
{
    private Creature? _creature;
    private readonly Dictionary<EquipmentSlot, CanvasItem?> _normalEquipmentNodes = new();
    private readonly Dictionary<EquipmentSlot, CanvasItem?> _majokaEquipmentNodes = new();
    private EquipmentSeries _lastHeadwearSeries = EquipmentSeries.None;
    private EquipmentSeries _lastTopSeries = EquipmentSeries.None;
    private EquipmentSeries _lastSkirtSeries = EquipmentSeries.None;
    private EquipmentSeries _lastShoesSeries = EquipmentSeries.None;
    private bool _lastHadMajoka;

    public override void _Ready()
    {
        base._Ready();
        _creature = (GetParent() as MegaCrit.Sts2.Core.Nodes.Combat.NCreature)?.Entity;
        CacheEquipmentNodes();
        _lastHadMajoka = HasMajoka();
        ApplyEquipmentVisuals(_lastHadMajoka);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_creature == null)
        {
            return;
        }

        bool hasMajoka = HasMajoka();
        EquipmentSeries headwear = GetSlotSeries(EquipmentSlot.Headwear);
        EquipmentSeries top = GetSlotSeries(EquipmentSlot.Top);
        EquipmentSeries skirt = GetSlotSeries(EquipmentSlot.Skirt);
        EquipmentSeries shoes = GetSlotSeries(EquipmentSlot.Shoes);

        if (hasMajoka == _lastHadMajoka
            && headwear == _lastHeadwearSeries
            && top == _lastTopSeries
            && skirt == _lastSkirtSeries
            && shoes == _lastShoesSeries)
        {
            return;
        }

        _lastHadMajoka = hasMajoka;
        _lastHeadwearSeries = headwear;
        _lastTopSeries = top;
        _lastSkirtSeries = skirt;
        _lastShoesSeries = shoes;
        ApplyEquipmentVisuals(hasMajoka);
    }

    private void CacheEquipmentNodes()
    {
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            string nodeName = slot.ToString();
            _normalEquipmentNodes[slot] = GetNodeOrNull<CanvasItem>($"Visuals/Equipment/{nodeName}");
            _majokaEquipmentNodes[slot] = GetNodeOrNull<CanvasItem>($"Visuals/Majoka/Equipment/{nodeName}");
        }
    }

    private bool HasMajoka()
        => _creature != null && _creature.GetPowerAmount<MajokaPower>() >= 100;

    private EquipmentSeries GetSlotSeries(EquipmentSlot slot)
    {
        if (_creature == null)
        {
            return EquipmentSeries.None;
        }

        EquipmentSlotPowerBase? power = slot switch
        {
            EquipmentSlot.Headwear => _creature.GetPower<EquipmentHeadwearSlotPower>(),
            EquipmentSlot.Top => _creature.GetPower<EquipmentTopSlotPower>(),
            EquipmentSlot.Skirt => _creature.GetPower<EquipmentSkirtSlotPower>(),
            EquipmentSlot.Shoes => _creature.GetPower<EquipmentShoesSlotPower>(),
            _ => null,
        };

        return power?.EquippedSeries ?? EquipmentSeries.None;
    }

    private void ApplyEquipmentVisuals(bool hasMajoka)
    {
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            EquipmentSeries series = GetSlotSeries(slot);
            ApplySlotVisual(slot, series, hasMajoka, _normalEquipmentNodes, false);
            ApplySlotVisual(slot, series, hasMajoka, _majokaEquipmentNodes, true);
        }
    }

    private void ApplySlotVisual(
        EquipmentSlot slot,
        EquipmentSeries series,
        bool hasMajoka,
        Dictionary<EquipmentSlot, CanvasItem?> nodes,
        bool isMajokaLayer)
    {
        if (!nodes.TryGetValue(slot, out CanvasItem? node) || node == null)
        {
            return;
        }

        bool shouldShow = series != EquipmentSeries.None && hasMajoka == isMajokaLayer;
        if (!shouldShow)
        {
            node.Visible = false;
            return;
        }

        string texturePath = SawatariCocoEquipmentHelper.GetVisualTexturePath(slot, series, isMajokaLayer);
        if (string.IsNullOrEmpty(texturePath) || !ResourceLoader.Exists(texturePath))
        {
            node.Visible = false;
            return;
        }

        Texture2D? texture = ResourceLoader.Load<Texture2D>(texturePath);
        if (texture == null)
        {
            node.Visible = false;
            return;
        }

        if (node is Sprite2D sprite)
        {
            sprite.Texture = texture;
        }

        node.Visible = true;
    }
}
