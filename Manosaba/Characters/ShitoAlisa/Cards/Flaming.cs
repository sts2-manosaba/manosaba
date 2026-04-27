using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.ShitoAlisa.Cards;

[Pool(typeof(ShitoAlisaCardPool))]
public class Flaming : ShitoAlisaCardModel
{
    private new const int EnergyCost = 3;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.AllEnemies;
    private new const bool ShouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Combust];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(3, new PowerVar<BurnPower>(4m));
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<FireballSwarmPower>()];

    public Flaming() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        decimal stacks = DynamicVars["BurnPower"].BaseValue;
        foreach (Creature e in CombatState.GetOpponentsOf(Owner.Creature).Where(e => e.IsAlive && e.IsHittable))
            await PowerCmd.Apply<BurnPower>(e, stacks, Owner.Creature, this);
        await PowerCmd.Apply<FireballSwarmPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPower"].UpgradeValueBy(2m);
    }
}
