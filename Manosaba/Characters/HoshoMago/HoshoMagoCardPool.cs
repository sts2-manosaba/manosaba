using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Extensions;

namespace manosaba.Characters.HoshoMago;

public class HoshoMagoCardPool : CustomCardPoolModel
{
    public override string Title => HoshoMago.CharacterId;

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();

    private static readonly (float H, float S, float V) CardBackTint = CardPoolTintFromCharacterColor.ToCardBackHsv(HoshoMago.Color);

    public override float H => CardBackTint.H;

    public override float S => CardBackTint.S;

    public override float V => CardBackTint.V;

    public override Color DeckEntryCardColor => HoshoMago.Color;

    public override bool IsColorless => false;
}
