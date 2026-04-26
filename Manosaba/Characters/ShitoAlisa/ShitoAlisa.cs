using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.ShitoAlisa.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.ShitoAlisa;

public class ShitoAlisa : PlaceholderCharacterModel
{
    public const string CharacterId = "shito_alisa";

    public static readonly Color Color = new("F1260F");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 66;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeShitoAlisa>(),
        ModelDb.Card<StrikeShitoAlisa>(),
        ModelDb.Card<StrikeShitoAlisa>(),
        ModelDb.Card<StrikeShitoAlisa>(),
        ModelDb.Card<DefendShitoAlisa>(),
        ModelDb.Card<DefendShitoAlisa>(),
        ModelDb.Card<DefendShitoAlisa>(),
        ModelDb.Card<TraumaShitoAlisa>(),
        ModelDb.Card<EmberSpark>(),
        ModelDb.Card<Lighter>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<LegIrons>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<ShitoAlisaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ShitoAlisaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ShitoAlisaPotionPool>();

    public override string CustomIconTexturePath => (CharacterId + "_map.png").CharacterImgPath(CharacterId);
    public override string CustomCharacterSelectIconPath => (CharacterId + "_char_select.png").CharacterImgPath(CharacterId);
    public override string CustomMapMarkerPath => (CharacterId + "_map.png").CharacterImgPath(CharacterId);
    public override string CustomCharacterSelectBg => (CharacterId + "_bg.tscn").CharacterScenePath(CharacterId);
    public override string CustomVisualPath => (CharacterId + ".tscn").CharacterScenePath(CharacterId);
    public override string CustomIconPath => (CharacterId + "_icon.tscn").CharacterScenePath(CharacterId);
    public override string CustomArmPointingTexturePath => (CharacterId + "_arm_pointing.png").CharacterImgPath(CharacterId);
    public override string CustomRestSiteAnimPath => (CharacterId + "_rest_site.tscn").CharacterScenePath(CharacterId);
    public override string CustomMerchantAnimPath => (CharacterId + "_merchant.tscn").CharacterScenePath(CharacterId);
    public override string CustomArmRockTexturePath => (CharacterId + "_arm_rock.png").CharacterImgPath(CharacterId);
    public override string CustomArmPaperTexturePath => (CharacterId + "_arm_paper.png").CharacterImgPath(CharacterId);
    public override string CustomArmScissorsTexturePath => (CharacterId + "_arm_scissors.png").CharacterImgPath(CharacterId);
    public override string CustomEnergyCounterPath => (CharacterId + "_energy_counter.tscn").CharacterScenePath(CharacterId);

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(CharacterId);

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
