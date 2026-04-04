using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.SaekiMiria;

public class SaekiMiriaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => SaekiMiria.Color;


    public override string BigEnergyIconPath => "saeki_miria_energy.png".CharacterImgPath(SaekiMiria.CharacterId);
    public override string TextEnergyIconPath => "saeki_miria_energy.png".CharacterImgPath(SaekiMiria.CharacterId);
}