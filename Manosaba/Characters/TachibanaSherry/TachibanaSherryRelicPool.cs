using BaseLib.Abstracts;
using manosaba.Extensions;
using Godot;

namespace manosaba.Characters.TachibanaSherry;

public class TachibanaSherryRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TachibanaSherry.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}