using BaseLib.Utils;
using Manosaba.Characters.Common.Resources;
using Manosaba.Extensions;
using manosaba.Characters.NatsumeAnan.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.NatsumeAnan.Relics;

[Pool(typeof(NatsumeAnanRelicPool))]
public sealed class Clipboard : LevelingPathCustomRelicModel, ICustomEnergySaveCarrier
{
    private const int BlockPerKotodamaSpent = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override int MaxRelicLevel => 5;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromPower<SicklyPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("KotodamaEnergy", 1m),
        new BlockVar(BlockPerKotodamaSpent, ValueProp.Unpowered),
        new PowerVar<SicklyPower>(1m),
    ];

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

    public override async Task BeforeCombatStart()
    {
        SyncKotodamaGainVar();

        if (Owner?.Creature == null)
        {
            return;
        }

        await CommonActions.Apply<SicklyPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, null, DynamicVars["SicklyPower"].BaseValue);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player != Owner)
        {
            return Task.CompletedTask;
        }

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

        int blockToGain = spentKotodama * DynamicVars.Block.IntValue;
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
        DynamicVars["KotodamaEnergy"].BaseValue = RelicLevel >= 4 ? 3m : RelicLevel >= 2 ? 2m : 1m;
    }
}
