using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public sealed class LetHeartBurn : ShitoAlisaCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromCard<DyingMessage>(true),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(0);

    public LetHeartBurn() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (CombatState is not { } combatState || Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        int hpLoss = Math.Max(0, ownerCreature.CurrentHp - 1);
        if (ownerCreature.CurrentHp > 1)
        {
            await CreatureCmd.SetCurrentHp(ownerCreature, 1);
        }

        if (hpLoss > 0)
        {
            foreach (Creature enemy in combatState.GetOpponentsOf(ownerCreature).Where(e => e.IsAlive && e.IsHittable && e.CanReceivePowers).ToList())
            {
                await CommonActions.Apply<BurnPower>(choiceContext, enemy, this, hpLoss);
            }
        }

        CardModel dyingMessage = combatState.CreateCard(ModelDb.Card<DyingMessage>(), Owner);
        dyingMessage.UpgradeInternal();
        dyingMessage.FinalizeUpgradeInternal();
        await CardPileCmd.AddGeneratedCardToCombat(dyingMessage, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
