using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCocoPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => SawatariCoco.Color;

    public override string BigEnergyIconPath => "sawatari_coco_energy.png".CharacterImgPath(SawatariCoco.CharacterId);
    public override string TextEnergyIconPath => "sawatari_coco_energy.png".CharacterImgPath(SawatariCoco.CharacterId);
}
