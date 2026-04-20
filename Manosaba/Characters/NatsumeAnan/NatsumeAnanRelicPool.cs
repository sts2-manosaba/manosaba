using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.NatsumeAnan;

public class NatsumeAnanRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => NatsumeAnan.Color;

    public override string BigEnergyIconPath => "natsume_anan_energy.png".CharacterImgPath(NatsumeAnan.CharacterId);
    public override string TextEnergyIconPath => "natsume_anan_energy.png".CharacterImgPath(NatsumeAnan.CharacterId);
}
