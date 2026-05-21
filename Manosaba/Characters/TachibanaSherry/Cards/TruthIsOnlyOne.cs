using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class TruthIsOnlyOne : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<CluePower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<CluePower>(4m)];

    public TruthIsOnlyOne() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        await PowerCmd.Apply<CluePower>(choiceContext, ownerCreature, DynamicVars["CluePower"].BaseValue, ownerCreature, this);

        decimal block = ownerCreature.GetPowerAmount<CluePower>();
        if (block > 0m)
        {
            await CreatureCmd.GainBlock(ownerCreature, block, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CluePower"].UpgradeValueBy(2m);
    }
}
