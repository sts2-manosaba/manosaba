using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.HikamiMeruru.Relics;
using manosaba.Characters.SaekiMiria.Relics;
using manosaba.Extensions;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.HikamiMeruru.Cards;
using Manosaba.Characters.JogasakiNoah.Cards;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SaekiMiria
{
    public class SaekiMiria : PlaceholderCharacterModel
    {
        public const string CharacterId = "saeki_miria";

        public static readonly Color Color = new("FFD700");
        public override Color MapDrawingColor => Color;
        public override Color NameColor => Color;
        public override CharacterGender Gender => CharacterGender.Feminine;
        public override int StartingHp => 80;

        public override IEnumerable<CardModel> StartingDeck => [
            ModelDb.Card<StrikeSaekiMiria>(),
            ModelDb.Card<StrikeSaekiMiria>(),
            ModelDb.Card<StrikeSaekiMiria>(),
            ModelDb.Card<DefendSaekiMiria>(),
            ModelDb.Card<DefendSaekiMiria>(),
            ModelDb.Card<DefendSaekiMiria>(),
            ModelDb.Card<DefendSaekiMiria>(),
            ModelDb.Card<Clumsy>(),
            ModelDb.Card<TraumaSaekiMiria>(),
            ModelDb.Card<Exchange>(),
        ];

        public override IReadOnlyList<RelicModel> StartingRelics =>
        [
            ModelDb.Relic<CabinetKey>()
        ];

        public override CardPoolModel CardPool => ModelDb.CardPool<SaekiMiriaCardPool>();
        public override RelicPoolModel RelicPool => ModelDb.RelicPool<SaekiMiriaRelicPool>();
        public override PotionPoolModel PotionPool => ModelDb.PotionPool<SaekiMiriaPotionPool>();

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

        public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
    }
}
