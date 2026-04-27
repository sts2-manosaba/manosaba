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
    private const int energyCost = 2;
    private const CardType TypeValue = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetTypeValue = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MeruruInfirmaryPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MeruruInfirmaryPower>(1)];

    public MeruruInfirmary() : base(energyCost, TypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        decimal amount = DynamicVars["MeruruInfirmaryPower"].BaseValue;
        if (!IsUpgraded)
        {
            await PowerCmd.Apply<MeruruInfirmaryPower>(ownerCreature, amount, ownerCreature, this);
            return;
        }

        if (CombatState is not { } combatState)
        {
            return;
        }

        IEnumerable<Creature> teammates = from c in combatState.GetTeammatesOf(ownerCreature)
                                          where c != null && c.IsAlive && c.IsPlayer
                                          select c;
        foreach (Creature teammate in teammates)
        {
            await PowerCmd.Apply<MeruruInfirmaryPower>(teammate, amount, ownerCreature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
