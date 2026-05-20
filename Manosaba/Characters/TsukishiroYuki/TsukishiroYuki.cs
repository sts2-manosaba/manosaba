using Godot;
using manosaba.Characters.TsukishiroYuki.Cards;
using manosaba.Characters.ShitoAlisa;
using manosaba.Extensions;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.TsukishiroYuki;

public class TsukishiroYuki : ManosabaLockedCharacterModel
{
    public const string CharacterId = ManosabaLockedCharacterIds.TsukishiroYuki;

    protected override string LockedCharacterId => CharacterId;

    public static readonly Color Color = new("B8D4E8");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeTsukishiroYuki>(),
        ModelDb.Card<StrikeTsukishiroYuki>(),
        ModelDb.Card<StrikeTsukishiroYuki>(),
        ModelDb.Card<StrikeTsukishiroYuki>(),
        ModelDb.Card<DefendTsukishiroYuki>(),
        ModelDb.Card<DefendTsukishiroYuki>(),
        ModelDb.Card<DefendTsukishiroYuki>(),
        ModelDb.Card<DefendTsukishiroYuki>(),
        ModelDb.Card<TraumaTsukishiroYuki>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<LegIrons>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<TsukishiroYukiCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TsukishiroYukiRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TsukishiroYukiPotionPool>();

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(CharacterId);
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
