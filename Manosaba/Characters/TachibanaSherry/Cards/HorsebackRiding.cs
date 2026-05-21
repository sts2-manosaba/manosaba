using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using HannaCharacter = manosaba.Characters.TonoHanna.TonoHanna;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class HorsebackRiding : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HorsebackRidingPower>(),
        HoverTipFactory.FromPower<SoarPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HorsebackRidingPower>(1m)];

    public HorsebackRiding() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || CombatState is not { } combatState)
        {
            return;
        }

        await CommonActions.Apply<HorsebackRidingPower>(choiceContext, ownerCreature, this, DynamicVars[nameof(HorsebackRidingPower)].BaseValue);

        foreach (Creature teammate in combatState.GetTeammatesOf(ownerCreature).Where(c => c.IsAlive && c.CanReceivePowers))
        {
            if (teammate.Player?.Character is HannaCharacter)
            {
                await CommonActions.Apply<HannaPuppetPower>(choiceContext, teammate, this, 1m);
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
