using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.TestSupport;

namespace manosaba.Characters.TonoHanna.Relics;

[Pool(typeof(TonoHannaRelicPool))]
public sealed class Frugal : LevelingPathCustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("GoldPerEnergy", 5m)];

    public override Task AfterObtained()
    {
        ApplyRelicLevelEffects();
        return Task.CompletedTask;
    }

    protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
    {
        ApplyRelicLevelEffects();
    }

    private void ApplyRelicLevelEffects()
    {
        DynamicVars["GoldPerEnergy"].BaseValue = 5m + (RelicLevel - 1);
    }

    /// <summary>Called when <see cref="Manosaba.Characters.TonoHanna.Powers.PuppetCollectionSummaryPower"/> increases (a new distinct puppet appears in the collection).</summary>
    public static async Task OnPuppetCollectionIncreasedAsync(Creature owner, decimal positiveDelta)
    {
        if (positiveDelta <= 0m || TestMode.IsOn)
            return;

        Player? player = owner.Player;
        Frugal? frugal = player?.GetRelic<Frugal>();
        if (frugal == null)
            return;

        int perGain = frugal.RelicLevel switch
        {
            >= 4 => 10,
            >= 2 => 5,
            _ => 0
        };

        if (perGain <= 0)
            return;

        decimal majoka = perGain * positiveDelta;
        await PowerCmd.Apply<MajokaPower>(owner, majoka, owner, null);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Creature.Side)
            return;

        if (!Owner.Creature.IsAlive)
            return;

        if (Owner.PlayerCombatState == null)
            return;

        int energyLeft = Owner.PlayerCombatState.Energy;
        if (energyLeft <= 0)
            return;

        int gold = energyLeft * DynamicVars["GoldPerEnergy"].IntValue;
        if (gold <= 0)
            return;

        await PlayerCmd.GainGold(gold, Owner);
    }
}
