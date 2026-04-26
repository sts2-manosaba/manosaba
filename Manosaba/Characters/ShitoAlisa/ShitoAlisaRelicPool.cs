using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.ShitoAlisa;

public class ShitoAlisaRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => ShitoAlisa.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
