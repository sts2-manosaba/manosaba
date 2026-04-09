using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.HasumiLeia;

public class HasumiLeiaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => HasumiLeia.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
