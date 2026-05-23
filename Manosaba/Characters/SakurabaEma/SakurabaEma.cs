using Godot;
using manosaba.Characters.SakurabaEma.Cards;
using manosaba.Extensions;
using Manosaba.Characters.Common;
using Manosaba.Characters.Common.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SakurabaEma;

public class SakurabaEma : ManosabaLockedCharacterModel
{
    public const string CharacterId = ManosabaLockedCharacterIds.SakurabaEma;

    protected override string LockedCharacterId => CharacterId;

    public static readonly Color Color = new("E8A0BF");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeSakurabaEma>(),
        ModelDb.Card<StrikeSakurabaEma>(),
        ModelDb.Card<StrikeSakurabaEma>(),
        ModelDb.Card<StrikeSakurabaEma>(),
        ModelDb.Card<DefendSakurabaEma>(),
        ModelDb.Card<DefendSakurabaEma>(),
        ModelDb.Card<DefendSakurabaEma>(),
        ModelDb.Card<DefendSakurabaEma>(),
        ModelDb.Card<TraumaSakurabaEma>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<LockedCharacterStarterRelic>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<SakurabaEmaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SakurabaEmaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SakurabaEmaPotionPool>();

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(CharacterId);
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
