using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NikaidoHiro;

public class NikaidoHiroCardPool : CustomCardPoolModel
{
    public override string Title => NikaidoHiro.CharacterId; //This is not a display name.
    public const string CharacterId = "nikaido_hiro";
    public override string BigEnergyIconPath => (CharacterId + "_energy.png").CharacterImgPath(CharacterId);
    public override string TextEnergyIconPath => (CharacterId + "_energy_text.png").CharacterImgPath(CharacterId);


    private static readonly (float H, float S, float V) CardBackTint = CardPoolTintFromCharacterColor.ToCardBackHsv(NikaidoHiro.Color);

    public override float H => CardBackTint.H;

    public override float S => CardBackTint.S;

    public override float V => CardBackTint.V;

    public override Color DeckEntryCardColor => NikaidoHiro.Color;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}