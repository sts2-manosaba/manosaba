using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class MeruruInfirmary : PathCustomCardModel
{
    private const int EnergyCost = 2;
    private const CardType TypeValue = CardType.Power;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MeruruInfirmaryPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MeruruInfirmaryPower>(1)];

    public MeruruInfirmary() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = DynamicVars["MeruruInfirmaryPower"].BaseValue;
        if (!IsUpgraded)
        {
            await PowerCmd.Apply<MeruruInfirmaryPower>(Owner.Creature, amount, Owner.Creature, this);
            return;
        }

        IEnumerable<Creature> teammates = from c in CombatState.GetTeammatesOf(Owner.Creature)
                                          where c != null && c.IsAlive && c.IsPlayer
                                          select c;
        foreach (Creature teammate in teammates)
        {
            await PowerCmd.Apply<MeruruInfirmaryPower>(teammate, amount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
