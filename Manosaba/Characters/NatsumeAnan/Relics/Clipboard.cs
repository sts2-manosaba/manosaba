using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.NatsumeAnan.Relics;

[Pool(typeof(NatsumeAnanRelicPool))]
public sealed class Clipboard : LevelingPathCustomRelicModel
{
    private const int BaseCombatStartKotodamaGain = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("KotodamaEnergy", BaseCombatStartKotodamaGain)];

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
}
