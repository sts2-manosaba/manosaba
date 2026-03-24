using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.NikaidoHiro.Relics;
using manosaba.Extensions;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.NikaidoHiro.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NikaidoHiro
{
    public class NikaidoHiro : PlaceholderCharacterModel
    {
        public const string CharacterId = "nikaido_hiro";

        public static readonly Color Color = new("B72222");
        public override Color NameColor => Color;
        public override CharacterGender Gender => CharacterGender.Feminine;
        public override int StartingHp => 60;

        public override IEnumerable<CardModel> StartingDeck => [
            ModelDb.Card<StrikeNikaidoHiro>(),
            ModelDb.Card<StrikeNikaidoHiro>(),
            ModelDb.Card<StrikeNikaidoHiro>(),
            ModelDb.Card<StrikeNikaidoHiro>(),
            ModelDb.Card<DefendNikaidoHiro>(),
            ModelDb.Card<DefendNikaidoHiro>(),
            ModelDb.Card<DefendNikaidoHiro>(),
            ModelDb.Card<DefendNikaidoHiro>(),
            ModelDb.Card<EmaNikaidoHiro>(),
            ModelDb.Card<EmaNikaidoHiro>()
        ];

        public override IReadOnlyList<RelicModel> StartingRelics =>
        [
            ModelDb.Relic<PenOfHiro>()
        ];

        public override CardPoolModel CardPool => ModelDb.CardPool<NikaidoHiroCardPool>();
        public override RelicPoolModel RelicPool => ModelDb.RelicPool<NikaidoHiroRelicPool>();
        public override PotionPoolModel PotionPool => ModelDb.PotionPool<NikaidoHiroPotionPool>();

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
    }
}
