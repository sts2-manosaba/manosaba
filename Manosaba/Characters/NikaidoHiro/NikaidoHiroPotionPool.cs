using BaseLib.Abstracts;
using manosaba.Extensions;
using Godot;

namespace manosaba.Characters.NikaidoHiro;

public class NikaidoHiroPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => NikaidoHiro.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}