using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SakurabaEma;

public class SakurabaEmaCardPool : CustomCardPoolModel
{
    public override string Title => SakurabaEma.CharacterId;
    public const string CharacterId = SakurabaEma.CharacterId;

    public override string BigEnergyIconPath => LockedCharacterStarterRelicPool.FallbackBigEnergyIconPath;
    public override string TextEnergyIconPath => LockedCharacterStarterRelicPool.FallbackTextEnergyIconPath;

    private static readonly (float H, float S, float V) CardBackTint =
        CardPoolTintFromCharacterColor.ToCardBackHsv(SakurabaEma.Color);

    public override float H => CardBackTint.H;
    public override float S => CardBackTint.S;
    public override float V => CardBackTint.V;
    public override Color DeckEntryCardColor => SakurabaEma.Color;
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() => [];
}
