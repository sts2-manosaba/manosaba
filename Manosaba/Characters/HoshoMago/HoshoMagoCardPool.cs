using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;
using Manosaba.Extensions;

namespace manosaba.Characters.HoshoMago;

public class HoshoMagoCardPool : CustomCardPoolModel
{
    public override string Title => HoshoMago.CharacterId;
    public const string CharacterId = "hosho_mago";
    public override string BigEnergyIconPath => (CharacterId + "_energy.png").CharacterImgPath(CharacterId);
    public override string TextEnergyIconPath => (CharacterId + "_energy_text.png").CharacterImgPath(CharacterId);

    private static readonly (float H, float S, float V) CardBackTint = CardPoolTintFromCharacterColor.ToCardBackHsv(HoshoMago.Color);

    public override float H => CardBackTint.H;

    public override float S => CardBackTint.S;

    public override float V => CardBackTint.V;

    public override Color DeckEntryCardColor => HoshoMago.Color;

    public override bool IsColorless => false;
}
