using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class YouAreWeakEma : ShitoAlisaCardModel
{
    private sealed class WeakEmaCalculatedDamageVar : CalculatedDamageVar
    {
        public WeakEmaCalculatedDamageVar()
            : base(ValueProp.Move)
        {
            WithMultiplier(CalculateFireballStacks);
        }

        private static decimal CalculateFireballStacks(CardModel card, Creature? target)
        {
            if (card.Owner?.Creature == null)
                return 0m;
            return card.Owner.Creature.GetPowerAmount<FireballSwarmPower>();
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            decimal rawDamage = card.DynamicVars.CalculationBase.BaseValue + card.DynamicVars.ExtraDamage.BaseValue * CalculateFireballStacks(card, target);
            if (card.Owner == null)
            {
                PreviewValue = Math.Max(rawDamage, 0m);
                return;
            }
            if (runGlobalHooks)
            {
                CombatState? combatState = card.CombatState ?? card.Owner?.Creature?.CombatState;
                if (combatState == null)
                {
                    PreviewValue = Math.Max(rawDamage, 0m);
                }
                else
                {
                    PreviewValue = Hook.ModifyDamage(
                        card.Owner.RunState,
                        combatState,
                        target,
                        IsFromOsty ? card.Owner.Osty : card.Owner.Creature,
                        rawDamage,
                        Props,
                        card,
                        ModifyDamageHookType.All,
                        previewMode,
                        out IEnumerable<AbstractModel> _);
                }
            }
            else
            {
                PreviewValue = Math.Max(rawDamage, 0m);
            }
        }
    }

    private const int EnergyCost = 1;
    private const CardType TypeValue = CardType.Attack;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0, new CalculationBaseVar(5m), new ExtraDamageVar(3m), new WeakEmaCalculatedDamageVar());
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FireballSwarmPower>()];

    public YouAreWeakEma() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        decimal totalDamage = DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        if (totalDamage <= 0m)
            return;

        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
