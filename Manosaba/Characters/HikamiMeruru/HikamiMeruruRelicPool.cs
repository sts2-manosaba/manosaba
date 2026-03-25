using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.HikamiMeruru;

public class HikamiMeruruRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => HikamiMeruru.Color;

    public override string BigEnergyIconPath => "hikami_meruru_energy.png".CharacterImgPath(HikamiMeruru.CharacterId);
    public override string TextEnergyIconPath => "hikami_meruru_energy_text.png".CharacterImgPath(HikamiMeruru.CharacterId);
}