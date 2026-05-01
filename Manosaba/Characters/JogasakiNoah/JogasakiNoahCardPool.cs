using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.JogasakiNoah;

public class JogasakiNoahCardPool : CustomCardPoolModel
{
    public override string Title => JogasakiNoah.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();


    private static readonly (float H, float S, float V) CardBackTint = CardPoolTintFromCharacterColor.ToCardBackHsv(JogasakiNoah.Color);

    public override float H => CardBackTint.H;

    public override float S => CardBackTint.S;

    public override float V => CardBackTint.V;

    public override Color DeckEntryCardColor => JogasakiNoah.Color;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}