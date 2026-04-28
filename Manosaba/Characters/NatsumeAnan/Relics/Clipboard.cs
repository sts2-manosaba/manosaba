using BaseLib.Utils;
using Manosaba.Characters.Common.Resources;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.NatsumeAnan.Relics;

[Pool(typeof(NatsumeAnanRelicPool))]
public sealed class Clipboard : LevelingPathCustomRelicModel, ICustomEnergySaveCarrier
{
    private const int BaseCombatStartKotodamaGain = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("KotodamaEnergy", BaseCombatStartKotodamaGain)];

    [SavedProperty]
    public int Manosaba_KotodamaEnergy { get; set; }

    public CharacterCustomEnergyDefinition SavedEnergyDefinition => KotodamaEnergy.Instance;

    public int SavedCustomEnergyValue
    {
        get => Manosaba_KotodamaEnergy;
        set => Manosaba_KotodamaEnergy = Math.Max(0, value);
    }

    public Player? SavedCustomEnergyOwner => Owner;

    public override Task AfterObtained()
    {
        SyncKotodamaGainVar();
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        SyncKotodamaGainVar();
        int gain = DynamicVars["KotodamaEnergy"].IntValue;
        if (gain > 0)
        {
            KotodamaEnergy.Gain(Owner, gain);
            Flash();
        }

        return Task.CompletedTask;
    }

    public async Task OnKotodamaSpentByCardPlay(int spentKotodama)
    {
        if (spentKotodama <= 0 || Owner?.Creature == null || !Owner.Creature.IsAlive)
        {
            return;
        }

        int blockPerKotodama = GetBlockPerKotodamaForLevel();
        int blockToGain = spentKotodama * blockPerKotodama;
        if (blockToGain <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, blockToGain, ValueProp.Unpowered, null);
    }

    protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
    {
        _ = oldLevel;
        _ = newLevel;
        SyncKotodamaGainVar();
    }

    private void SyncKotodamaGainVar()
    {
        DynamicVars["KotodamaEnergy"].BaseValue = BaseCombatStartKotodamaGain + (RelicLevel - 1);
    }

    private int GetBlockPerKotodamaForLevel()
    {
        return RelicLevel switch
        {
            <= 1 => 2,
            2 => 3,
            3 => 4,
            4 => 6,
            _ => 9,
        };
    }
}
