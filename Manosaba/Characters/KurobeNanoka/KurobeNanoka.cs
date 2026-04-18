using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.KurobeNanoka.Relics;
using manosaba.Extensions;
using Manosaba.Characters.HasumiLeia.Cards;
using Manosaba.Characters.KurobeNanoka.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.KurobeNanoka;

public class KurobeNanoka : PlaceholderCharacterModel
{
    public const string CharacterId = "kurobe_nanoka";

    public static readonly Color Color = new("85919a");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 60;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeKurobeNanoka>(),
        ModelDb.Card<StrikeKurobeNanoka>(),
        ModelDb.Card<StrikeKurobeNanoka>(),
        ModelDb.Card<DefendKurobeNanoka>(),
        ModelDb.Card<DefendKurobeNanoka>(),
        ModelDb.Card<DefendKurobeNanoka>(),
        ModelDb.Card<DefendKurobeNanoka>(),
        ModelDb.Card<TraumaKurobeNanoka>(),
        ModelDb.Card<GunShot>(),
        ModelDb.Card<GunShot>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<Ribbon>(),
        ModelDb.Relic<MagicalGun>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<KurobeNanokaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<KurobeNanokaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<KurobeNanokaPotionPool>();

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
