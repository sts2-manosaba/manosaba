using BaseLib.Utils;
using manosaba.Characters.SawatariCoco.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SawatariCoco.Powers;

/// <summary>网购：每回合开始时从购物牌中选择一张加入抽牌堆；打出购物牌时花费金币。</summary>
public sealed class OnlineShoppingPower : PathCustomPowerModel
{
    private bool _upgraded;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public void SetUpgraded(bool upgraded)
    {
        _upgraded = upgraded;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || player != Owner.Player || Owner.CombatState is not { } combatState)
        {
            return;
        }

        List<CardModel> options = SawatariCocoHelper.ShoppingCardTypes
            .Select(type =>
            {
                CardModel? canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(type));
                return canonical != null ? combatState.CreateCard(canonical, player) : null;
            })
            .OfType<CardModel>()
            .ToList();

        if (options.Count == 0)
        {
            return;
        }

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: true);
        if (selected == null)
        {
            return;
        }

        if (_upgraded)
        {
            CardCmd.Upgrade(selected);
        }

        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Draw, player);
    }
}
