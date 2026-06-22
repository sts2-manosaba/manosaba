using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.HasumiLeia.Relics;
using manosaba.Extensions;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.HasumiLeia.Cards;
using manosaba.Characters.HasumiLeia.Helpers;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.HasumiLeia;

public class HasumiLeia : PlaceholderCharacterModel
{
	public const string CharacterId = "hasumi_leia";

	public static readonly Color Color = new("feb459");
	public override Color MapDrawingColor => Color;
	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Feminine;
	public override int StartingHp => 80;

	public override IEnumerable<CardModel> StartingDeck =>
	[
		ModelDb.Card<StrikeHasumiLeia>(),
		ModelDb.Card<StrikeHasumiLeia>(),
		ModelDb.Card<StrikeHasumiLeia>(),
		ModelDb.Card<Voiding>(),
		ModelDb.Card<DefendHasumiLeia>(),
		ModelDb.Card<DefendHasumiLeia>(),
		ModelDb.Card<DefendHasumiLeia>(),
		ModelDb.Card<DefendHasumiLeia>(),
		ModelDb.Card<TraumaHasumiLeia>(),
		ModelDb.Card<Lunge>(),
	];

	public override IReadOnlyList<RelicModel> StartingRelics =>
	[
		ModelDb.Relic<Rapier>()
	];

	public override CardPoolModel CardPool => ModelDb.CardPool<HasumiLeiaCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<HasumiLeiaRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<HasumiLeiaPotionPool>();

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

	public override string CustomAttackSfx => HasumiLeiaSfx.Instance.CharacterAttack ?? null!;
	public override string CustomCastSfx => HasumiLeiaSfx.Instance.CharacterCast ?? null!;
	public override string CustomDeathSfx => HasumiLeiaSfx.Instance.CharacterDeath ?? null!;

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}
