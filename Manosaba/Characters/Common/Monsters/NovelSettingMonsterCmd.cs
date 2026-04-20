using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using Manosaba.Characters.JogasakiNoah.Powers;

namespace Manosaba.Characters.Common.Monsters;

public static class NovelSettingMonsterCmd
{
    public static async Task<SummonResult> Summon<TMonster>(PlayerChoiceContext choiceContext, Player summoner, AbstractModel? source)
        where TMonster : MonsterModel
    {
        CombatState combatState = summoner.Creature.CombatState;
        decimal amount = Hook.ModifySummonAmount(combatState, summoner, 1m, source);
        Creature summoned = combatState.Allies.FirstOrDefault((Creature c) => c.Monster is TMonster && c.PetOwner == summoner);
        if (amount == 0m)
        {
            return new SummonResult(summoned, 0m);
        }

        if (CombatManager.Instance.IsInProgress)
        {
            SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
        }

        if (summoned != null && summoned.IsAlive)
        {
            await CreatureCmd.GainMaxHp(summoned, amount);
        }
        else
        {
            bool isReviving = summoned != null;
            if (isReviving)
            {
                if (summoned.IsAlive)
                {
                    throw new InvalidOperationException("We shouldn't make it here if summoned monster is still alive!");
                }

                summoner.PlayerCombatState.AddPetInternal(summoned);
            }
            else
            {
                summoned = await PlayerCmd.AddPet<TMonster>(summoner);
                NCreature summonedNode = NCombatRoom.Instance?.GetCreatureNode(summoned);
                summonedNode?.TrackBlockStatus(summoner.Creature);
            }

            await CreatureCmd.SetMaxHp(summoned, amount);
            await CreatureCmd.Heal(summoned, amount, isReviving);
        }

        Creature perspective = summoned.PetOwner?.Creature ?? summoner.Creature;
        List<Creature> petTargets = summoned.CombatState.GetOpponentsOf(perspective)
            .Where(c => c != null && c.IsAlive)
            .ToList();
        if (petTargets.Count > 0 && PetEnemyAiPower.TryPrepareForNextTurnWithTargets(summoned, petTargets, rollNewMove: true))
        {
            PetEnemyAiPower.TryAdvanceToValidMove(summoned, petTargets);
        }
        await PowerCmd.Apply<PetEnemyAiPower>(summoned, 1m, summoner.Creature, null);

        if (TestMode.IsOff)
        {
            NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(summoned);
            nCreature.OstyScaleToSize(summoned.MaxHp, 0.75f);
            nCreature.ToggleIsInteractable(true);
        }

        CombatManager.Instance.History.Summoned(combatState, (int)amount, summoner);
        await Hook.AfterSummon(combatState, choiceContext, summoner, amount);
        return new SummonResult(summoned, amount);
    }
}
