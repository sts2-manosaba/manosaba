using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class Buttstroke : PathCustomCardModel
{
    private const string BluntHitSfx = "event:/sfx/characters/ironclinger/ironclinger_attack";
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Attack;
    private const CardRarity Rarity = CardRarity.Common;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m),
        new CardsVar(1),
    ];

    public Buttstroke() : base(EnergyCost, Type, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitFx(vfx: "vfx/vfx_attack_blunt", sfx: BluntHitSfx)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await PowerCmd.Apply<WeakPower>(target, DynamicVars.Weak.BaseValue, ownerCreature, this);
        await PowerCmd.Apply<VulnerablePower>(target, DynamicVars.Vulnerable.BaseValue, ownerCreature, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
