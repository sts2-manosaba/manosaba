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
        public override string CustomIconTexturePath => "nikaido_hiro_map.png".CharacterUiPath();
        public override string CustomCharacterSelectIconPath => "nikaido_hiro_char_select.png".CharacterUiPath();
        public override string CustomMapMarkerPath => "nikaido_hiro_map.png".CharacterUiPath();
        public override string CustomCharacterSelectBg => "NikaidoHiroBg.tscn".CharacterScenePath();
        public override string CustomVisualPath => "NikaidoHiro.tscn".CharacterScenePath();
        public override string CustomIconPath => "NikaidoHiroIcon.tscn".CharacterScenePath();
    }
}
