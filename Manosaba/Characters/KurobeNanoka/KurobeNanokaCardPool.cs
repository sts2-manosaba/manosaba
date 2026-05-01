using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.KurobeNanoka;

public class KurobeNanokaCardPool : CustomCardPoolModel
{
    public override string Title => KurobeNanoka.CharacterId;

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();

    private static readonly (float H, float S, float V) CardBackTint = CardPoolTintFromCharacterColor.ToCardBackHsv(KurobeNanoka.Color);

    public override float H => CardBackTint.H;

    public override float S => CardBackTint.S;

    public override float V => CardBackTint.V;

    public override Color DeckEntryCardColor => KurobeNanoka.Color;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}

