using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCocoCardPool : CustomCardPoolModel
{
    public override string Title => SawatariCoco.CharacterId;
    public const string CharacterId = SawatariCoco.CharacterId;

    public override string BigEnergyIconPath => LockedCharacterStarterRelicPool.FallbackBigEnergyIconPath;
    public override string TextEnergyIconPath => LockedCharacterStarterRelicPool.FallbackTextEnergyIconPath;

    private static readonly (float H, float S, float V) CardBackTint =
        CardPoolTintFromCharacterColor.ToCardBackHsv(SawatariCoco.Color);

    public override float H => CardBackTint.H;
    public override float S => CardBackTint.S;
    public override float V => CardBackTint.V;
    public override Color DeckEntryCardColor => SawatariCoco.Color;
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() => [];
}
