using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.TsukishiroYuki;

public class TsukishiroYukiRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TsukishiroYuki.Color;

    public override string BigEnergyIconPath => (TsukishiroYuki.CharacterId + "_energy.png").CharacterImgPath(TsukishiroYuki.CharacterId);
    public override string TextEnergyIconPath => (TsukishiroYuki.CharacterId + "_energy_text.png").CharacterImgPath(TsukishiroYuki.CharacterId);
}
