using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class StarburstStream : PathCustomCardModel
{
    protected override bool HasEnergyCostX => true;

    public const string EnergyRequirementVar = "EnergyRequirement";

    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const int HitCount = 16;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(0m, ValueProp.Move),
        new RepeatVar(HitCount),
        new DynamicVar(EnergyRequirementVar, 3m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.SwordTechnique, ManosabaKeywords.TwoSwords];

    protected override bool IsPlayable
    {
        get
        {
            if (Owner?.Creature == null || Owner.PlayerCombatState == null)
            {
                return false;
            }

            if (Owner.Creature.GetPowerAmount<SecondSwordPower>() <= 0m)
            {
                return false;
            }

            int requiredEnergy = DynamicVars[EnergyRequirementVar].IntValue;
            return Owner.PlayerCombatState.Energy >= requiredEnergy;
        }
    }

    public StarburstStream()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        // Exhaust the hand (excluding this card, which is no longer in hand when played).
        List<CardModel> cardsToExhaust = PileType.Hand.GetPile(Owner).Cards.ToList();
        int swordTechniqueExhaustedCount = cardsToExhaust.Count(c => c.CanonicalKeywords.Contains(ManosabaKeywords.SwordTechnique));
        foreach (CardModel card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        int x = ResolveEnergyXValue();
        DynamicVars.Damage.BaseValue = x + swordTechniqueExhaustedCount;

        decimal beforeAllHp = target.CurrentHp;
        decimal beforeAllBlock = target.Block;

        // Single damage per hit for now (avoids interactive issues).
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(HitCount)
            .WithHitFx("vfx/vfx_big_slash")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        decimal afterAllHp = target.CurrentHp;
        decimal afterAllBlock = target.Block;
        decimal totalDamageDealt = Math.Max(0m, (beforeAllHp - afterAllHp) + (beforeAllBlock - afterAllBlock));

        if (IsUpgraded && Owner?.Creature is { } dealer && target.IsAlive)
        {
            decimal extraDamage = decimal.Ceiling(totalDamageDealt * 0.3m);
            if (extraDamage > 0m)
            {
                // Small pause + big-hit VFX before the 17th (upgrade) hit.
                await Cmd.CustomScaledWait(0.5f, 0.5f);
                VfxCmd.PlayOnCreature(target, "vfx/vfx_big_slash");
                await CreatureCmd.Damage(choiceContext, target, extraDamage, ValueProp.Unpowered, dealer, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
