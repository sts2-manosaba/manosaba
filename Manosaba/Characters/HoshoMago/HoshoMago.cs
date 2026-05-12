using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.HoshoMago.Relics;
using manosaba.Extensions;
using Manosaba.Characters.HoshoMago.Cards;
using manosaba.Characters.HoshoMago.Helpers;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.HoshoMago;

public class HoshoMago : PlaceholderCharacterModel
{
    public const string CharacterId = "hosho_mago";
    private const string PlaceholderCharacterId = HoshoMago.CharacterId;

    public static readonly Color Color = new("BC7DEF");
    public override Color MapDrawingColor => Color;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 65;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeHoshoMago>(),
        ModelDb.Card<StrikeHoshoMago>(),
        ModelDb.Card<StrikeHoshoMago>(),
        ModelDb.Card<DefendHoshoMago>(),
        ModelDb.Card<DefendHoshoMago>(),
        ModelDb.Card<DefendHoshoMago>(),
        ModelDb.Card<TraumaHoshoMago>(),
        ModelDb.Card<DreamInterpretation>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<TarotDeck>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<HoshoMagoCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HoshoMagoRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HoshoMagoPotionPool>();

    public override string CustomIconTexturePath => (PlaceholderCharacterId + "_map.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomCharacterSelectIconPath => (PlaceholderCharacterId + "_char_select.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomMapMarkerPath => (PlaceholderCharacterId + "_map.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomCharacterSelectBg => (PlaceholderCharacterId + "_bg.tscn").CharacterScenePath(PlaceholderCharacterId);
    public override string CustomVisualPath => (PlaceholderCharacterId + ".tscn").CharacterScenePath(PlaceholderCharacterId);
    public override string CustomIconPath => (PlaceholderCharacterId + "_icon.tscn").CharacterScenePath(PlaceholderCharacterId);
    public override string CustomArmPointingTexturePath => (PlaceholderCharacterId + "_arm_pointing.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomRestSiteAnimPath => (PlaceholderCharacterId + "_rest_site.tscn").CharacterScenePath(PlaceholderCharacterId);
    public override string CustomMerchantAnimPath => (PlaceholderCharacterId + "_merchant.tscn").CharacterScenePath(PlaceholderCharacterId);
    public override string CustomArmRockTexturePath => (PlaceholderCharacterId + "_arm_rock.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomArmPaperTexturePath => (PlaceholderCharacterId + "_arm_paper.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomArmScissorsTexturePath => (PlaceholderCharacterId + "_arm_scissors.png").CharacterImgPath(PlaceholderCharacterId);
    public override string CustomEnergyCounterPath => (PlaceholderCharacterId + "_energy_counter.tscn").CharacterScenePath(PlaceholderCharacterId);

    public override string CharacterSelectSfx => ManosabaCharacterSfx.CharacterSelectEvent(PlaceholderCharacterId);

    public override string CustomAttackSfx => HoshoMagoSfx.Instance.CharacterAttack ?? null!;
    public override string CustomCastSfx => HoshoMagoSfx.Instance.CharacterCast ?? null!;
    public override string CustomDeathSfx => HoshoMagoSfx.Instance.CharacterDeath ?? null!;

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
