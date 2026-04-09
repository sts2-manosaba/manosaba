using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;

namespace manosaba.Characters.HoshoMago;

public class HoshoMagoCardPool : CustomCardPoolModel
{
    public override string Title => HoshoMago.CharacterId;

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();

    public override float H => 1f;
    public override float S => 0.8f;
    public override float V => 0.7f;

    public override Color DeckEntryCardColor => HoshoMago.Color;

    public override bool IsColorless => false;
}
