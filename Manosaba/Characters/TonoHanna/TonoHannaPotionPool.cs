using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.TonoHanna;

public class TonoHannaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => TonoHanna.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
