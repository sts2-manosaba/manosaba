using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.NatsumeAnan.Cards;
using manosaba.Characters.NatsumeAnan.Relics;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NatsumeAnan;

public class NatsumeAnan : PlaceholderCharacterModel
{
    public const string CharacterId = "natsume_anan";

    static NatsumeAnan()
    {
        KotodamaEnergy.Register();
    }

    public static readonly Color Color = new("6A5ACD");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 50;
    public override int StartingGold => 99;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeNatsumeAnan>(),
        ModelDb.Card<StrikeNatsumeAnan>(),
        ModelDb.Card<DefendNatsumeAnan>(),
        ModelDb.Card<DefendNatsumeAnan>(),
        ModelDb.Card<TraumaNatsumeAnan>(),
        ModelDb.Card<FlashOfInspiration>(),
        ModelDb.Card<Instigate>(),
        ModelDb.Card<Instigate>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<Clipboard>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<NatsumeAnanCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<NatsumeAnanRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<NatsumeAnanPotionPool>();

    public override string CustomIconTexturePath => (CharacterId + "_map.png").CharacterImgPath(CharacterId);
    public override string CustomCharacterSelectIconPath => (CharacterId + "_char_select.png").CharacterImgPath(CharacterId);
    public override string CustomMapMarkerPath => (CharacterId + "_map.png").CharacterImgPath(CharacterId);
    public override string CustomCharacterSelectBg => (CharacterId + "_bg.tscn").CharacterScenePath(CharacterId);
    public override string CustomVisualPath => (CharacterId + ".tscn").CharacterScenePath(CharacterId);
    public override string CustomIconPath => (CharacterId + "_icon.tscn").CharacterScenePath(CharacterId);
    public override string CustomArmPointingTexturePath => (CharacterId + "_arm_pointing.png").CharacterImgPath(CharacterId);
    public override string CustomRestSiteAnimPath => (CharacterId + "_rest_site.tscn").CharacterScenePath(CharacterId);
    public override string CustomMerchantAnimPath => (CharacterId + "_merchant.tscn").CharacterScenePath(CharacterId);
    public override string CustomArmRockTexturePath => (CharacterId + "_arm_pointing.png").CharacterImgPath(CharacterId);
    public override string CustomArmPaperTexturePath => (CharacterId + "_arm_pointing.png").CharacterImgPath(CharacterId);
    public override string CustomArmScissorsTexturePath => (CharacterId + "_arm_pointing.png").CharacterImgPath(CharacterId);
    public override string CustomEnergyCounterPath => (CharacterId + "_energy_counter.tscn").CharacterScenePath(CharacterId);

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(CharacterId);
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
