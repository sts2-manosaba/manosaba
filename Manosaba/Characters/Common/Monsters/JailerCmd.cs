using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Monsters;

public static class JailerCmd
{
    public static Creature? Summon(Player summoner, decimal amount)
    {
        if (summoner.Creature?.CombatState == null || summoner.PlayerCombatState == null)
        {
            return null;
        }

        int hpAmount = Math.Max(0, (int)decimal.Round(amount, MidpointRounding.AwayFromZero));
        if (hpAmount <= 0)
        {
            return summoner.PlayerCombatState.GetPet<Jailer>();
        }

        Creature? jailer = summoner.PlayerCombatState.GetPet<Jailer>();
        if (jailer is { IsAlive: true })
        {
            return jailer;
        }

        bool didSummon = false;

        if (jailer == null)
        {
            MonsterModel monster = ModelDb.GetById<MonsterModel>(ModelDb.Monster<Jailer>().Id).ToMutable();
            jailer = summoner.Creature.CombatState.CreateCreature(monster, CombatSide.Player, null);
            didSummon = true;
        }

        jailer.SetMaxHpInternal(hpAmount);
        jailer.SetCurrentHpInternal(hpAmount);
        summoner.PlayerCombatState.AddPetInternal(jailer);
        didSummon = true;

        NCreature? jailerNode = NCombatRoom.Instance?.GetCreatureNode(jailer);
        if (jailerNode == null)
        {
            NCombatRoom.Instance?.AddCreature(jailer);
            jailerNode = NCombatRoom.Instance?.GetCreatureNode(jailer);
        }

        jailerNode?.TrackBlockStatus(summoner.Creature);
        if (jailerNode != null)
        {
            jailerNode.ToggleIsInteractable(true);
            jailerNode.OstyScaleToSize(jailer.MaxHp, 0.75f);
        }

        if (didSummon && CombatManager.Instance.IsInProgress)
        {
            SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
            CombatManager.Instance.History.Summoned(summoner.Creature.CombatState, hpAmount, summoner);
        }

        return jailer;
    }
}
