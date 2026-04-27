using Godot;
using HarmonyLib;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NHealthBar))]
public static class Patch_NHealthBar_BurnPreview
{
    private static readonly AccessTools.FieldRef<NHealthBar, Control> _hpForegroundContainerRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForegroundContainer");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> _hpForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForeground");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> _poisonForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_poisonForeground");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> _doomForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_doomForeground");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> _hpMiddlegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpMiddleground");
    private static readonly AccessTools.FieldRef<NHealthBar, MegaLabel> _hpLabelRef =
        AccessTools.FieldRefAccess<NHealthBar, MegaLabel>("_hpLabel");
    private static readonly AccessTools.FieldRef<NHealthBar, TextureRect> _infinityTexRef =
        AccessTools.FieldRefAccess<NHealthBar, TextureRect>("_infinityTex");
    private static readonly AccessTools.FieldRef<NHealthBar, Creature> _creatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");
    private static readonly AccessTools.FieldRef<NHealthBar, Creature?> _blockTrackingCreatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature?>("_blockTrackingCreature");
    private static readonly AccessTools.FieldRef<NHealthBar, LocString> _healthBarDeadRef =
        AccessTools.FieldRefAccess<NHealthBar, LocString>("_healthBarDead");
    private static readonly AccessTools.FieldRef<NHealthBar, Tween?> _middlegroundTweenRef =
        AccessTools.FieldRefAccess<NHealthBar, Tween?>("_middlegroundTween");
    private static readonly AccessTools.FieldRef<NHealthBar, int> _currentHpOnLastRefreshRef =
        AccessTools.FieldRefAccess<NHealthBar, int>("_currentHpOnLastRefresh");
    private static readonly AccessTools.FieldRef<NHealthBar, int> _maxHpOnLastRefreshRef =
        AccessTools.FieldRefAccess<NHealthBar, int>("_maxHpOnLastRefresh");
    private static readonly AccessTools.FieldRef<NHealthBar, float> _expectedMaxFgWidthRef =
        AccessTools.FieldRefAccess<NHealthBar, float>("_expectedMaxFgWidth");

    private static readonly Color _defaultFontColor = StsColors.cream;
    private static readonly Color _defaultFontOutlineColor = new("900000");
    private static readonly Color _blockOutlineColor = new("1B3045");
    private static readonly Color _invincibleForegroundColor = new("C5BBED");
    private static readonly Color _burnPreviewColor = new("6C0000");

    [HarmonyPatch("RefreshValues")]
    [HarmonyPrefix]
    private static bool RefreshValuesPrefix(NHealthBar __instance)
    {
        return IsHealthBarUiAlive(__instance);
    }

    [HarmonyPatch("RefreshBlockUi")]
    [HarmonyPrefix]
    private static bool RefreshBlockUiPrefix(NHealthBar __instance)
    {
        return IsHealthBarUiAlive(__instance);
    }

    [HarmonyPatch("RefreshForeground")]
    [HarmonyPrefix]
    private static bool RefreshForegroundPrefix(NHealthBar __instance)
    {
        if (!IsHealthBarUiAlive(__instance))
        {
            return false;
        }

        Creature creature = _creatureRef(__instance);
        Control hpForeground = _hpForegroundRef(__instance);
        Control poisonForeground = _poisonForegroundRef(__instance);
        Control burnForeground = GetOrCreateBurnForeground(__instance);
        Control doomForeground = _doomForegroundRef(__instance);

        if (creature.CurrentHp <= 0)
        {
            burnForeground.Visible = false;
            poisonForeground.Visible = false;
            doomForeground.Visible = false;
            hpForeground.Visible = false;
            return false;
        }

        hpForeground.Visible = true;
        float maxFgWidth = GetMaxFgWidth(__instance);
        float offsetRight = GetFgWidth(creature, creature.CurrentHp, maxFgWidth) - maxFgWidth;
        hpForeground.OffsetRight = offsetRight;

        if (creature.ShowsInfiniteHp)
        {
            hpForeground.SelfModulate = _invincibleForegroundColor;
            burnForeground.Visible = false;
            poisonForeground.Visible = false;
            return false;
        }

        int doomAmount = creature.GetPowerAmount<DoomPower>();
        bool hasPoison = creature.HasPower<PoisonPower>();
        bool hasBurn = creature.HasPower<BurnPower>();
        int poisonDamage = creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0;
        int burnDamage = creature.GetPower<BurnPower>()?.CalculateTotalDamageNextTurn() ?? 0;
        int periodicDamage = poisonDamage + burnDamage;

        if (burnForeground is CanvasItem burnCanvas)
        {
            burnCanvas.SelfModulate = _burnPreviewColor;
        }
        burnForeground.Visible = false;

        if (periodicDamage > 0)
        {
            poisonForeground.Visible = true;
            if (periodicDamage >= creature.CurrentHp)
            {
                poisonForeground.OffsetLeft = 0f;
                poisonForeground.OffsetRight = offsetRight;
                hpForeground.Visible = false;
            }
            else
            {
                float projectedFgWidth = GetFgWidth(creature, creature.CurrentHp - periodicDamage, maxFgWidth);
                hpForeground.OffsetRight = projectedFgWidth - maxFgWidth;
                hpForeground.Visible = true;
                int patchMarginLeft = ((NinePatchRect)poisonForeground).PatchMarginLeft;
                poisonForeground.OffsetLeft = Math.Max(0f, projectedFgWidth - patchMarginLeft);
                poisonForeground.OffsetRight = offsetRight;
            }

            if (hasBurn && burnDamage > 0)
            {
                burnForeground.Visible = true;
                if (burnDamage >= creature.CurrentHp)
                {
                    burnForeground.OffsetLeft = 0f;
                    burnForeground.OffsetRight = offsetRight;
                    poisonForeground.Visible = false;
                }
                else
                {
                    float burnStartWidth = GetFgWidth(creature, Math.Max(0, creature.CurrentHp - burnDamage), maxFgWidth);
                    int burnPatchMarginLeft = ((NinePatchRect)burnForeground).PatchMarginLeft;
                    burnForeground.OffsetLeft = Math.Max(0f, burnStartWidth - burnPatchMarginLeft);
                    burnForeground.OffsetRight = offsetRight;

                    if (hasPoison && poisonDamage > 0)
                    {
                        int poisonPatchMarginLeft = ((NinePatchRect)poisonForeground).PatchMarginLeft;
                        float poisonStartWidth = GetFgWidth(creature, Math.Max(0, creature.CurrentHp - burnDamage - poisonDamage), maxFgWidth);
                        poisonForeground.OffsetLeft = Math.Max(0f, poisonStartWidth - poisonPatchMarginLeft);
                        poisonForeground.OffsetRight = burnStartWidth - maxFgWidth;
                    }
                    else
                    {
                        poisonForeground.Visible = false;
                    }
                }
            }
        }
        else
        {
            burnForeground.Visible = false;
            poisonForeground.Visible = false;
            poisonForeground.OffsetLeft = 0f;
        }

        if (creature.HasPower<DoomPower>())
        {
            if (doomAmount > 0)
            {
                doomForeground.Visible = true;
                float doomFgOffsetRight = GetFgWidth(creature, doomAmount, maxFgWidth) - maxFgWidth;
                bool isDoomLethal = doomAmount >= creature.CurrentHp - periodicDamage;
                if (isDoomLethal)
                {
                    if (periodicDamage < creature.CurrentHp)
                    {
                        doomForeground.OffsetRight = hpForeground.OffsetRight;
                        hpForeground.Visible = false;
                    }
                    else
                    {
                        hpForeground.Visible = false;
                        doomForeground.Visible = false;
                    }
                }
                else
                {
                    int patchMarginRight = ((NinePatchRect)doomForeground).PatchMarginRight;
                    doomForeground.OffsetRight = Math.Min(0f, doomFgOffsetRight + patchMarginRight);
                    hpForeground.Visible = true;
                }
            }
            else
            {
                doomForeground.Visible = false;
            }
        }
        else
        {
            doomForeground.Visible = false;
        }

        return false;
    }

    [HarmonyPatch("RefreshMiddleground")]
    [HarmonyPrefix]
    private static bool RefreshMiddlegroundPrefix(NHealthBar __instance)
    {
        if (!IsHealthBarUiAlive(__instance))
        {
            return false;
        }

        Creature creature = _creatureRef(__instance);
        Control hpMiddleground = _hpMiddlegroundRef(__instance);
        Control hpForeground = _hpForegroundRef(__instance);
        Control poisonForeground = _poisonForegroundRef(__instance);
        Control burnForeground = GetOrCreateBurnForeground(__instance);

        if (creature.CurrentHp <= 0)
        {
            hpMiddleground.Visible = false;
            return false;
        }

        hpMiddleground.Visible = true;
        hpMiddleground.Position = new Vector2(1f, hpMiddleground.Position.Y);

        int currentHp = creature.CurrentHp;
        int maxHp = creature.MaxHp;
        if (currentHp != _currentHpOnLastRefreshRef(__instance) || maxHp != _maxHpOnLastRefreshRef(__instance))
        {
            _currentHpOnLastRefreshRef(__instance) = currentHp;
            _maxHpOnLastRefreshRef(__instance) = maxHp;

            int poisonDamage = creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0;
            int burnDamage = creature.GetPower<BurnPower>()?.CalculateTotalDamageNextTurn() ?? 0;
            bool hasPeriodicPreview = poisonDamage + burnDamage > 0;
            float targetOffset = hasPeriodicPreview
                ? (poisonDamage > 0 ? poisonForeground.OffsetRight : burnForeground.OffsetRight)
                : hpForeground.OffsetRight;
            bool shouldAnimateImmediately = targetOffset >= hpMiddleground.OffsetRight;

            hpMiddleground.OffsetRight += 1f;
            _middlegroundTweenRef(__instance)?.Kill();
            Tween tween = __instance.CreateTween();
            _middlegroundTweenRef(__instance) = tween;
            tween.TweenProperty(hpMiddleground, "offset_right", targetOffset - 2f, 1.0)
                .SetDelay(shouldAnimateImmediately ? 0.0 : 1.0)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Expo);
        }

        return false;
    }

    [HarmonyPatch("RefreshText")]
    [HarmonyPrefix]
    private static bool RefreshTextPrefix(NHealthBar __instance)
    {
        if (!IsHealthBarUiAlive(__instance))
        {
            return false;
        }

        Creature creature = _creatureRef(__instance);
        MegaLabel hpLabel = _hpLabelRef(__instance);
        Control doomForeground = _doomForegroundRef(__instance);
        TextureRect infinityTex = _infinityTexRef(__instance);

        if (creature.CurrentHp <= 0)
        {
            hpLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, _defaultFontColor);
            hpLabel.AddThemeColorOverride(ThemeConstants.Label.FontOutlineColor, _defaultFontOutlineColor);
            hpLabel.SetTextAutoSize(_healthBarDeadRef(__instance).GetRawText());
            return false;
        }

        if (creature.ShowsInfiniteHp)
        {
            infinityTex.Visible = creature.IsAlive;
            doomForeground.Modulate = Colors.Transparent;
            hpLabel.Visible = !infinityTex.Visible;
            return false;
        }

        hpLabel.Visible = true;
        int periodicDamage = CalculatePeriodicDamageNextTurn(creature);
        int doomAmount = creature.GetPowerAmount<DoomPower>();
        bool isPeriodicLethal = periodicDamage > 0 && periodicDamage >= creature.CurrentHp;
        bool isDoomLethal = doomAmount > 0 && creature.HasPower<DoomPower>() && doomAmount >= creature.CurrentHp - periodicDamage;

        Color fontColor;
        Color outlineColor;
        if (isPeriodicLethal)
        {
            fontColor = new Color("76FF40");
            outlineColor = new Color("074700");
        }
        else if (isDoomLethal)
        {
            fontColor = new Color("FB8DFF");
            outlineColor = new Color("2D1263");
        }
        else
        {
            Creature? blockTrackingCreature = _blockTrackingCreatureRef(__instance);
            bool hasBlockOutline = creature.Block > 0 || blockTrackingCreature?.Block > 0;
            fontColor = _defaultFontColor;
            outlineColor = hasBlockOutline ? _blockOutlineColor : _defaultFontOutlineColor;
        }

        hpLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, fontColor);
        hpLabel.AddThemeColorOverride(ThemeConstants.Label.FontOutlineColor, outlineColor);
        hpLabel.SetTextAutoSize($"{creature.CurrentHp}/{creature.MaxHp}");
        return false;
    }

    private static int CalculatePeriodicDamageNextTurn(Creature creature)
    {
        int poisonDamage = creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0;
        int burnDamage = creature.GetPower<BurnPower>()?.CalculateTotalDamageNextTurn() ?? 0;
        return poisonDamage + burnDamage;
    }

    private static float GetMaxFgWidth(NHealthBar healthBar)
    {
        float expected = _expectedMaxFgWidthRef(healthBar);
        if (expected > 0f)
        {
            return expected;
        }

        return _hpForegroundContainerRef(healthBar).Size.X;
    }

    private static float GetFgWidth(Creature creature, int amount, float maxFgWidth)
    {
        if (creature.MaxHp <= 0)
        {
            return 0f;
        }

        float val = (float)amount / creature.MaxHp * maxFgWidth;
        return Math.Max(val, creature.CurrentHp > 0 ? 12f : 0f);
    }

    private static Control GetOrCreateBurnForeground(NHealthBar healthBar)
    {
        Control hpForegroundContainer = _hpForegroundContainerRef(healthBar);
        if (!GodotObject.IsInstanceValid(hpForegroundContainer))
        {
            return _poisonForegroundRef(healthBar);
        }

        Control? existing = hpForegroundContainer.GetNodeOrNull<Control>("Mask/BurnForegroundPatched");
        if (existing != null && GodotObject.IsInstanceValid(existing))
        {
            return existing;
        }

        NinePatchRect poisonForeground = (NinePatchRect)_poisonForegroundRef(healthBar);
        if (!GodotObject.IsInstanceValid(poisonForeground))
        {
            return hpForegroundContainer;
        }

        Node? parentNode = poisonForeground.GetParent();
        if (parentNode == null || !GodotObject.IsInstanceValid(parentNode))
        {
            return poisonForeground;
        }

        NinePatchRect burnForeground = (NinePatchRect)poisonForeground.Duplicate();
        burnForeground.Name = "BurnForegroundPatched";
        burnForeground.Visible = false;
        parentNode.AddChild(burnForeground);
        parentNode.MoveChild(burnForeground, poisonForeground.GetIndex());
        return burnForeground;
    }

    private static bool IsHealthBarUiAlive(NHealthBar healthBar)
    {
        return GodotObject.IsInstanceValid(healthBar)
            && GodotObject.IsInstanceValid(_hpForegroundContainerRef(healthBar))
            && GodotObject.IsInstanceValid(_hpForegroundRef(healthBar))
            && GodotObject.IsInstanceValid(_poisonForegroundRef(healthBar))
            && GodotObject.IsInstanceValid(_doomForegroundRef(healthBar))
            && GodotObject.IsInstanceValid(_hpMiddlegroundRef(healthBar))
            && GodotObject.IsInstanceValid(_hpLabelRef(healthBar))
            && GodotObject.IsInstanceValid(_infinityTexRef(healthBar));
    }
}
