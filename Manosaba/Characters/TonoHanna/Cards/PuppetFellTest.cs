using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class PuppetFellTest : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Quest;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PuppetCollectionSummaryPower>()];

    public PuppetFellTest() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = Owner.Creature;
        await PowerCmd.Apply<AlisaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<AnAnPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<CocoPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<EmaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<HannaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<HiroPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<LeiaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<MargoPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<MeruruPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<MiriaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<NanokaPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<NoahPuppetCollectionPower>(creature, 1m, creature, this);
        await PowerCmd.Apply<SherryPuppetCollectionPower>(creature, 1m, creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}
