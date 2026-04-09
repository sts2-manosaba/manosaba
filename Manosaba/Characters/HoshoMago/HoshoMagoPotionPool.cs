using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.HoshoMago;

public class HoshoMagoPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => HoshoMago.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
