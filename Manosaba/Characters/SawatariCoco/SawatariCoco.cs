using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.SawatariCoco.Cards;
using manosaba.Characters.SawatariCoco.Helpers;
using manosaba.Characters.SawatariCoco.Relics;
using manosaba.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCoco : PlaceholderCharacterModel
{
    public const string CharacterId = "sawatari_coco";

    public static readonly Color Color = new("FE743A");

    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 60;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<StrikeSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<DefendSawatariCoco>(),
        ModelDb.Card<TraumaSawatariCoco>(),
        ModelDb.Card<PhoneLiveStream>(),
        ModelDb.Card<ForMyOshi>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<LiveStreamingEquipment>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<SawatariCocoCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SawatariCocoRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SawatariCocoPotionPool>();

    public override string? CustomIconTexturePath => null;
    public override string? CustomIconOutlineTexturePath => null;
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
    public override string CustomAttackSfx => SawatariCocoSfx.Instance.CharacterAttack ?? null!;
    public override string CustomCastSfx => SawatariCocoSfx.Instance.CharacterCast ?? null!;
    public override string CustomDeathSfx => SawatariCocoSfx.Instance.CharacterDeath ?? null!;
}
