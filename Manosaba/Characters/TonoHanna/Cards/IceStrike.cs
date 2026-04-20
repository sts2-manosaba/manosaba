using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards;

[Pool(typeof(TonoHannaCardPool))]
public class IceStrike : PathCustomCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>
    /// Card UI uses <see cref="DamageVar.UpdateCardPreview"/>; bonus from <see cref="PuppetCollectionSummaryPower"/> was only applied in <see cref="OnPlay"/>, so the printed damage was too low in combat.
    /// </summary>
    private sealed class IceStrikeDamageVar : DamageVar
    {
        public IceStrikeDamageVar(decimal damage, ValueProp props)
            : base(damage, props)
        {
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
            if (card.Owner?.Creature == null)
            {
                return;
            }

            int collection = card.Owner.Creature.GetPower<PuppetCollectionSummaryPower>()?.Amount ?? 0;
            if (collection == 0)
            {
                return;
            }

            decimal bonusPer = card.IsUpgraded ? 6m : 5m;
            PreviewValue += collection * bonusPer;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IceStrikeDamageVar(25m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PuppetCollectionSummaryPower>()];

    public IceStrike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int collection = Owner.Creature.GetPower<PuppetCollectionSummaryPower>()?.Amount ?? 0;
        decimal bonusPer = IsUpgraded ? 6m : 5m;
        decimal damage = DynamicVars.Damage.BaseValue + collection * bonusPer;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
