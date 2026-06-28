using Godot;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Equipment;
using manosaba.Characters.SawatariCoco.Helper;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Manosaba.Characters.SawatariCoco.Visuals;

/// <summary>
/// Coco combat visuals: default body + majoka body in tscn; full-set outfit replaces the active sprite only.
/// Godot scene (sawatari_coco.tscn):
/// - Visuals/Sprite2D — default body (SawatariCoco.png in tscn)
/// - Visuals/Majoka — majoka body (SawatariCocoM.png in tscn)
/// Full set textures: res://Manosaba/images/characters/sawatari_coco/equipment/{series}.png
/// Majoka full set: .../equipment/{series}_majoka.png (e.g. punk_cat.png, punk_cat_majoka.png)
/// </summary>
public sealed partial class SawatariCocoCharacterVisuals : ManosabaCharacterVisuals
{
    private Creature? _creature;
    private Sprite2D? _normalSprite;
    private Sprite2D? _majokaSprite;
    private Texture2D? _defaultNormalTexture;
    private Texture2D? _defaultMajokaTexture;
    private EquipmentSeries? _lastFullSetSeries;

    public override void _Ready()
    {
        base._Ready();
        _creature = (GetParent() as MegaCrit.Sts2.Core.Nodes.Combat.NCreature)?.Entity;
        _normalSprite = GetNodeOrNull<Sprite2D>("Visuals/Sprite2D");
        _majokaSprite = GetNodeOrNull<Sprite2D>("Visuals/Majoka");
        _defaultNormalTexture = _normalSprite?.Texture;
        _defaultMajokaTexture = _majokaSprite?.Texture;
        _lastFullSetSeries = GetFullSetSeries();
        ApplyBodyTextures();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_creature == null)
        {
            return;
        }

        EquipmentSeries? fullSetSeries = GetFullSetSeries();
        if (fullSetSeries == _lastFullSetSeries)
        {
            return;
        }

        _lastFullSetSeries = fullSetSeries;
        ApplyBodyTextures();
    }

    private EquipmentSeries? GetFullSetSeries()
        => _creature == null ? null : SawatariCocoEquipmentHelper.GetFullSetSeries(_creature);

    private void ApplyBodyTextures()
    {
        ApplySpriteTexture(_normalSprite, _defaultNormalTexture, isMajokaLayer: false);
        ApplySpriteTexture(_majokaSprite, _defaultMajokaTexture, isMajokaLayer: true);
    }

    private void ApplySpriteTexture(Sprite2D? sprite, Texture2D? defaultTexture, bool isMajokaLayer)
    {
        if (sprite == null)
        {
            return;
        }

        EquipmentSeries? fullSetSeries = _lastFullSetSeries;
        if (fullSetSeries is EquipmentSeries series)
        {
            string texturePath = SawatariCocoEquipmentHelper.GetFullSetVisualTexturePath(series, isMajokaLayer);
            if (!string.IsNullOrEmpty(texturePath)
                && ResourceLoader.Exists(texturePath)
                && ResourceLoader.Load<Texture2D>(texturePath) is { } outfitTexture)
            {
                sprite.Texture = outfitTexture;
                return;
            }
        }

        if (defaultTexture != null)
        {
            sprite.Texture = defaultTexture;
        }
    }
}
