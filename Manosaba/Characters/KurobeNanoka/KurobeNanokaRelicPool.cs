using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.KurobeNanoka;

public class KurobeNanokaRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => KurobeNanoka.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}

