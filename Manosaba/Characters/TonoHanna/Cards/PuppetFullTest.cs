using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(QuestCardPool))]
public class PuppetFullTest : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Quest;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PuppetCollectionSummaryPower>()];

    public PuppetFullTest() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = Owner.Creature;
        await CommonActions.Apply<AlisaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<AnAnPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<CocoPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<EmaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<HannaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<HiroPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<LeiaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<MargoPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<MeruruPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<MiriaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<NanokaPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<NoahPuppetCollectionPower>(choiceContext, creature, this, 1m);
        await CommonActions.Apply<SherryPuppetCollectionPower>(choiceContext, creature, this, 1m);
    }

    protected override void OnUpgrade()
    {
    }
}
