using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCocoCardPool : CustomCardPoolModel
{
    public override string Title => SawatariCoco.CharacterId;
    public const string CharacterId = SawatariCoco.CharacterId;

    public override string BigEnergyIconPath => (CharacterId + "_energy.png").CharacterImgPath(CharacterId);
    public override string TextEnergyIconPath => (CharacterId + "_energy_text.png").CharacterImgPath(CharacterId);

    private static readonly (float H, float S, float V) CardBackTint =
        CardPoolTintFromCharacterColor.ToCardBackHsv(SawatariCoco.Color);

    public override float H => CardBackTint.H;
    public override float S => CardBackTint.S;
    public override float V => CardBackTint.V;
    public override Color DeckEntryCardColor => SawatariCoco.Color;
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() => [];
}
