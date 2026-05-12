using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.JogasakiNoah.Relics;
using manosaba.Extensions;
using Manosaba.Characters.JogasakiNoah.Cards;
using manosaba.Characters.JogasakiNoah.Helpers;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.JogasakiNoah
{
    public class JogasakiNoah : PlaceholderCharacterModel
    {
        public const string CharacterId = "jogasaki_noah";

        public static readonly Color Color = new("2EB0B1");
        public override Color MapDrawingColor => Color;
        public override Color NameColor => Color;
        public override CharacterGender Gender => CharacterGender.Feminine;
        public override int StartingHp => 70;

        public override IEnumerable<CardModel> StartingDeck => [
            ModelDb.Card<StrikeJogasakiNoah>(),
            ModelDb.Card<StrikeJogasakiNoah>(),
            ModelDb.Card<StrikeJogasakiNoah>(),
            ModelDb.Card<DefendJogasakiNoah>(),
            ModelDb.Card<DefendJogasakiNoah>(),
            ModelDb.Card<DefendJogasakiNoah>(),
            ModelDb.Card<TraumaJogasakiNoah>(),
            ModelDb.Card<Palette>(),
            ModelDb.Card<Palette>(),
            ModelDb.Card<ColorMix>()
        ];

        public override IReadOnlyList<RelicModel> StartingRelics =>
        [
            ModelDb.Relic<SprayCanOfNoah>()
        ];

        public override CardPoolModel CardPool => ModelDb.CardPool<JogasakiNoahCardPool>();
        public override RelicPoolModel RelicPool => ModelDb.RelicPool<JogasakiNoahRelicPool>();
        public override PotionPoolModel PotionPool => ModelDb.PotionPool<JogasakiNoahPotionPool>();

        /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
            override all the other methods that define those assets. 
            These are just some of the simplest assets, given some placeholders to differentiate your character with. 
            You don't have to, but you're suggested to rename these images. */
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

        public override string CustomAttackSfx => JogasakiNoahSfx.Instance.CharacterAttack ?? null!;
        public override string CustomCastSfx => JogasakiNoahSfx.Instance.CharacterCast ?? null!;
        public override string CustomDeathSfx => JogasakiNoahSfx.Instance.CharacterDeath ?? null!;

        public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
    }
}
