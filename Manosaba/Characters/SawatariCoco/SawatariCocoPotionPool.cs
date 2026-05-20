using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCocoPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => SawatariCoco.Color;

    public override string BigEnergyIconPath => (SawatariCoco.CharacterId + "_energy.png").CharacterImgPath(SawatariCoco.CharacterId);
    public override string TextEnergyIconPath => (SawatariCoco.CharacterId + "_energy_text.png").CharacterImgPath(SawatariCoco.CharacterId);
}
