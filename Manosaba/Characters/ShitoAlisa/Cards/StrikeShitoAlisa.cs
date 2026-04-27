using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class StrikeShitoAlisa : ShitoAlisaCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    private new const int EnergyCost = 1;
    private const CardType TypeValue = CardType.Attack;
    private new const CardRarity Rarity = CardRarity.Basic;
    private const TargetType TargetTypeValue = TargetType.AnyEnemy;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0, new DamageVar(6, ValueProp.Move));

    public StrikeShitoAlisa() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
