using BaseLib.Utils;
using manosaba.Characters.SakurabaEma;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.SakurabaEma.Cards;

[Pool(typeof(SakurabaEmaCardPool))]
public sealed class StrikeSakurabaEma : PathCustomCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public StrikeSakurabaEma() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, shouldShowInCardLibrary: false) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

[Pool(typeof(SakurabaEmaCardPool))]
public sealed class DefendSakurabaEma : PathCustomCardModel
{
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    public DefendSakurabaEma() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, shouldShowInCardLibrary: false) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

[Pool(typeof(SakurabaEmaCardPool))]
public sealed class TraumaSakurabaEma : PathCustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(10m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public TraumaSakurabaEma() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self, shouldShowInCardLibrary: false) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.Apply<MajokaPower>(choiceContext, Owner.Creature, this, DynamicVars["MajokaPower"].BaseValue);
}
