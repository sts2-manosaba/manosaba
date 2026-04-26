using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.ShitoAlisa;

public class ShitoAlisaCardPool : CustomCardPoolModel
{
    public override string Title => ShitoAlisa.CharacterId;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 0.08f;
    public override float S => 0.95f;
    public override float V => 0.95f;

    public override Color DeckEntryCardColor => ShitoAlisa.Color;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}
