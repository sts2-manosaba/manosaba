using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace manosaba.Characters.NikaidoHiro.Relics
{

    [Pool(typeof(NikaidoHiroRelicPool))]
    public sealed class PenOfHiro : PathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature == player.Creature)
            {
                if (base.Owner.Creature.GetPowerAmount<VotePower>() > 0)
                {
                    PlayerCmd.GainEnergy(1, player);
                    PowerCmd.Apply<VotePower>(base.Owner.Creature, -1m, player.Creature, null);
                }
            }
            return Task.CompletedTask;
        }
    }
}
