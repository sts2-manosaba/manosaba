using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Cards;

/// <summary>DEV: grants orbiting fireball stacks for layout testing. Remove from <see cref="ShitoAlisa.StartingDeck"/> when done.</summary>
[Pool(typeof(ShitoAlisaCardPool))]
public sealed class FireballSwarmTest : ShitoAlisaCardModel
{
    private new const int EnergyCost = 0;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Quest;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public FireballSwarmTest() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = Owner.Creature;
        await PowerCmd.Apply<FireballSwarmPower>(creature, 3m, creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}
