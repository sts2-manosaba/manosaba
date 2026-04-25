using Godot;
using MegaCrit.Sts2.addons.mega_text;

namespace manosaba.Characters.Common;

public partial class ManosabaLabel : MegaLabel
{
	private Font? _font;
	private int _fontSize;

	private Color _fontColor;
	private Color _fontShadowColor;
	private Color _fontOutlineColor;

	private int _shadowOffsetX;
	private int _shadowOffsetY;
	private int _outlineSize;
	private int _shadowOutlineSize;

	private bool _hasFont;
	private bool _hasFontSize;
	private bool _hasFontColor;
	private bool _hasFontShadowColor;
	private bool _hasFontOutlineColor;
	private bool _hasShadowOffsetX;
	private bool _hasShadowOffsetY;
	private bool _hasOutlineSize;
	private bool _hasShadowOutlineSize;

	public override void _EnterTree()
	{
		base._EnterTree();

		// 這裡抓到的就是 tscn 上原本的 override 值
		_hasFont = HasThemeFontOverride("font");
		if (_hasFont) _font = (Font)Get("theme_override_fonts/font");

		_hasFontSize = HasThemeFontSizeOverride("font_size");
		if (_hasFontSize) _fontSize = (int)Get("theme_override_font_sizes/font_size");

		_hasFontColor = HasThemeColorOverride("font_color");
		if (_hasFontColor) _fontColor = (Color)Get("theme_override_colors/font_color");

		_hasFontShadowColor = HasThemeColorOverride("font_shadow_color");
		if (_hasFontShadowColor) _fontShadowColor = (Color)Get("theme_override_colors/font_shadow_color");

		_hasFontOutlineColor = HasThemeColorOverride("font_outline_color");
		if (_hasFontOutlineColor) _fontOutlineColor = (Color)Get("theme_override_colors/font_outline_color");

		_hasShadowOffsetX = HasThemeConstantOverride("shadow_offset_x");
		if (_hasShadowOffsetX) _shadowOffsetX = (int)Get("theme_override_constants/shadow_offset_x");

		_hasShadowOffsetY = HasThemeConstantOverride("shadow_offset_y");
		if (_hasShadowOffsetY) _shadowOffsetY = (int)Get("theme_override_constants/shadow_offset_y");

		_hasOutlineSize = HasThemeConstantOverride("outline_size");
		if (_hasOutlineSize) _outlineSize = (int)Get("theme_override_constants/outline_size");

		_hasShadowOutlineSize = HasThemeConstantOverride("shadow_outline_size");
		if (_hasShadowOutlineSize) _shadowOutlineSize = (int)Get("theme_override_constants/shadow_outline_size");
	}

	public override void _Ready()
	{
		base._Ready(); // MegaLabel 會在這裡可能覆蓋字型

		RestoreOverridesFromSnapshot();
		Callable.From(RestoreOverridesFromSnapshot).CallDeferred(); // 保險再蓋一次
	}

	public void RestoreOverridesFromSnapshot()
	{
		if (_hasFont && _font != null) AddThemeFontOverride("font", _font);
		if (_hasFontSize) AddThemeFontSizeOverride("font_size", _fontSize);

		if (_hasFontColor) AddThemeColorOverride("font_color", _fontColor);
		if (_hasFontShadowColor) AddThemeColorOverride("font_shadow_color", _fontShadowColor);
		if (_hasFontOutlineColor) AddThemeColorOverride("font_outline_color", _fontOutlineColor);

		if (_hasShadowOffsetX) AddThemeConstantOverride("shadow_offset_x", _shadowOffsetX);
		if (_hasShadowOffsetY) AddThemeConstantOverride("shadow_offset_y", _shadowOffsetY);
		if (_hasOutlineSize) AddThemeConstantOverride("outline_size", _outlineSize);
		if (_hasShadowOutlineSize) AddThemeConstantOverride("shadow_outline_size", _shadowOutlineSize);
	}

	public void RestoreStyleExceptEnergyColors()
	{
		if (_hasFont && _font != null) AddThemeFontOverride("font", _font);
		if (_hasFontSize) AddThemeFontSizeOverride("font_size", _fontSize);

		// 保留官方 0 能量變色，不還原這兩個：
		// font_color
		// font_outline_color

		if (_hasFontShadowColor) AddThemeColorOverride("font_shadow_color", _fontShadowColor);

		if (_hasShadowOffsetX) AddThemeConstantOverride("shadow_offset_x", _shadowOffsetX);
		if (_hasShadowOffsetY) AddThemeConstantOverride("shadow_offset_y", _shadowOffsetY);
		if (_hasOutlineSize) AddThemeConstantOverride("outline_size", _outlineSize);
		if (_hasShadowOutlineSize) AddThemeConstantOverride("shadow_outline_size", _shadowOutlineSize);
	}

	public void RestoreStyleKeepingZeroEnergyColor(bool isZeroEnergy)
	{
		// 原本的 font/font_size/outline_size/shadow 都還原
		RestoreStyleExceptEnergyColors();

		// 只有非 0 能量時，強制改回自己的 outline color
		if (!isZeroEnergy && _hasFontOutlineColor)
			AddThemeColorOverride("font_outline_color", _fontOutlineColor);
	}


}
