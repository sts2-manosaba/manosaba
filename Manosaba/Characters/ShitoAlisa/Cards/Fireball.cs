using System;
using System.Collections.Generic;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Cards;

/// <summary>衍生牌「火球」：消耗、造成傷害並獲得 1 層火球環繞；升級打全體敵人。</summary>
[Pool(typeof(ShitoAlisaCardPool))]
public sealed class Fireball : ShitoAlisaCardModel
{
    private const int EnergyCost = 0;
    private const CardType TypeValue = CardType.Attack;
    private const CardRarity Rarity = CardRarity.Token;
    private const bool ShouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        WithCombust(0, new DamageVar(5m, ValueProp.Move));

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FireballSwarmPower>()];

    public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    public Fireball() : base(EnergyCost, TypeValue, Rarity, TargetType.AnyEnemy, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState? state = CombatState;
        if (state == null)
            return;

        if (IsUpgraded)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(state)
                .Execute(choiceContext);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        await PowerCmd.Apply<FireballSwarmPower>(Owner.Creature, 1m, Owner.Creature, this);
    }
}
