using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.TonoHanna.Relics;
using manosaba.Extensions;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.TonoHanna.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.TonoHanna
{
    public class TonoHanna : PlaceholderCharacterModel
    {
        public const string CharacterId = "tono_hanna";

        public static readonly Color Color = new("73BF00");
        public override Color MapDrawingColor => Color;
        public override Color NameColor => Color;
        public override CharacterGender Gender => CharacterGender.Feminine;
        public override int StartingHp => 66;
        public override int StartingGold => 0;

        public override IEnumerable<CardModel> StartingDeck => [
            ModelDb.Card<StrikeTonoHanna>(),
            ModelDb.Card<StrikeTonoHanna>(),
            ModelDb.Card<StrikeTonoHanna>(),
            ModelDb.Card<StrikeTonoHanna>(),
            ModelDb.Card<DefendTonoHanna>(),
            ModelDb.Card<DefendTonoHanna>(),
            ModelDb.Card<DefendTonoHanna>(),
            ModelDb.Card<Sewing>(),
            ModelDb.Card<Thrifty>(),
            ModelDb.Card<TraumaTonoHanna>(),
        ];

        public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<Frugal>()];

        public override CardPoolModel CardPool => ModelDb.CardPool<TonoHannaCardPool>();
        public override RelicPoolModel RelicPool => ModelDb.RelicPool<TonoHannaRelicPool>();
        public override PotionPoolModel PotionPool => ModelDb.PotionPool<TonoHannaPotionPool>();

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
}
