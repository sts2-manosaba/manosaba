using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.JogasakiNoah;

public class JogasakiNoahPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => JogasakiNoah.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}