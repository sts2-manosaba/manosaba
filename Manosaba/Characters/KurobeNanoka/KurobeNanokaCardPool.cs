using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.KurobeNanoka;

public class KurobeNanokaCardPool : CustomCardPoolModel
{
    public override string Title => KurobeNanoka.CharacterId;

    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();

    public override float H => 1f;
    public override float S => 0.8f;
    public override float V => 0.7f;

    public override Color DeckEntryCardColor => KurobeNanoka.Color;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}

