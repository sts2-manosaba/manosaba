using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Characters.JogasakiNoah.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public sealed class SekirinyakudouPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner?.Player is not { } player || player.PlayerCombatState?.OrbQueue == null)
        {
            return;
        }

        foreach (BloodOrb bloodOrb in player.PlayerCombatState.OrbQueue.Orbs.OfType<BloodOrb>().ToList())
        {
            bloodOrb.AddLayers(1m);
        }

        List<SekketsusoujitsuHelper.BloodOrbEvokeResult> evokedBloodOrbs = [];
        for (int i = 0; i < 2; i++)
        {
            SekketsusoujitsuHelper.BloodOrbEvokeResult? result = await SekketsusoujitsuHelper.EvokeHighestPassiveBloodOrb(choiceContext, player);
            if (result == null)
            {
                break;
            }

            evokedBloodOrbs.Add(result.Value);
        }

        foreach (SekketsusoujitsuHelper.BloodOrbEvokeResult evokedBloodOrb in evokedBloodOrbs)
        {
            BloodOrb generatedBloodOrb = (BloodOrb)ModelDb.Orb<BloodOrb>().ToMutable();
            generatedBloodOrb.AddLayers(evokedBloodOrb.Layers - generatedBloodOrb.Layers);
            await OrbCmd.Channel(choiceContext, generatedBloodOrb, player);
        }

        await Cmd.Wait(0.1f);
    }
}
