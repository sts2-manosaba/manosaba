using Godot;
using manosaba.Characters.SawatariCoco.Cards;
using manosaba.Characters.ShitoAlisa;
using manosaba.Extensions;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCoco : ManosabaLockedCharacterModel
{
    public const string CharacterId = ManosabaLockedCharacterIds.SawatariCoco;

    protected override string LockedCharacterId => CharacterId;

    public static readonly Color Color = new("D4A574");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<TraumaSawatariCoco>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<LegIrons>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<SawatariCocoCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SawatariCocoRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SawatariCocoPotionPool>();

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(CharacterId);
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
