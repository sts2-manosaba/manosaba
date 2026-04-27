using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.ShitoAlisa;

public class ShitoAlisaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => ShitoAlisa.Color;

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();
}
