using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Characters.HoshoMago.Powers;

public sealed class TheFoolPower : PathCustomPowerModel
{
    private bool _armed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforePlayPhaseStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (_armed || Owner?.Player == null || player != Owner.Player)
        {
            return;
        }
        await PowerCmd.Remove(this);
        bool flag;
        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int cardsPlayed;
            for (cardsPlayed = 0; cardsPlayed < 13; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    break;
                }

                if (CombatManager.Instance.IsPlayerReadyToEndTurn(player))
                {
                    break;
                }

                CardPile pile = PileType.Hand.GetPile(base.Owner.Player);
                CardModel card = pile.Cards.FirstOrDefault((CardModel c) => c.CanPlay());
                if (card == null)
                {
                    break;
                }

                Creature target = GetTarget(card, player.Creature.CombatState);
                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }

            flag = cardsPlayed >= 13;
            if (cardsPlayed == 0)
            {
                return;
            }
        }

        LocString line = (flag ? new LocString("relics", "WHISPERING_EARRING.warning") : new LocString("relics", "WHISPERING_EARRING.approval"));
        TalkCmd.Play(line, base.Owner.Player.Creature, VfxColor.Orange);

        _armed = true;

    }
    private Creature? GetTarget(CardModel card, CombatState combatState)
    {
        Rng combatTargets = base.Owner.Player.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => combatTargets.NextItem(combatState.Allies.Where((Creature c) => c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Player.Creature)),
            TargetType.AnyPlayer => base.Owner.Player.Creature,
            _ => null,
        };
    }
}
