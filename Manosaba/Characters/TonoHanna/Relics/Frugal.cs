using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace manosaba.Characters.TonoHanna.Relics;

[Pool(typeof(TonoHannaRelicPool))]
public sealed class Frugal : LevelingPathCustomRelicModel
{
    private const int MaxUnspentEnergyForGoldPerCombat = 5;
    private const decimal BaseGoldPerEnergy = 10m;
    private const int Lv4GoldPerBonusEnergy = 100;

    /// <summary>Unused energy points already converted to gold this combat (capped at <see cref="MaxUnspentEnergyForGoldPerCombat"/>).</summary>
    private int _energyChargesConvertedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("GoldPerEnergy", BaseGoldPerEnergy)];

    public override Task AfterObtained() => Task.CompletedTask;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (RelicLevel < 4 || Owner?.Creature == null)
        {
            return;
        }

        if (side != Owner.Creature.Side || combatState.RoundNumber > 1)
        {
            return;
        }

        if (Owner.PlayerCombatState == null)
        {
            return;
        }

        int bonusEnergy = Owner.Gold / Lv4GoldPerBonusEnergy;
        if (bonusEnergy <= 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(bonusEnergy, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        _energyChargesConvertedThisCombat = 0;
        return Task.CompletedTask;
    }

    protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
    {
        if (newLevel > oldLevel)
        {
            TaskHelper.RunSafely(OnRelicLevelIncreasedAsync(oldLevel, newLevel));
        }
    }

    private async Task OnRelicLevelIncreasedAsync(int oldLevel, int newLevel)
    {
        if (Owner?.Deck == null || Owner.RunState == null)
        {
            return;
        }

        int levelsGained = newLevel - oldLevel;
        for (int i = 0; i < levelsGained; i++)
        {
            await TryAddRandomPuppetCardAsync();
        }
    }

    private async Task TryAddRandomPuppetCardAsync()
    {
        if (Owner?.Deck == null || Owner.RunState == null)
        {
            return;
        }

        List<CardModel> puppetPool = ModelDb.CardPool<TonoHannaCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
            .ToList();

        if (puppetPool.Count <= 0)
        {
            return;
        }

        CardModel canonical = Owner.RunState.Rng.Niche.NextItem(puppetPool);
        CardModel card = Owner.RunState.CreateCard(canonical, Owner);
        CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
        CardCmd.PreviewCardPileAdd(result, 1.2f, CardPreviewStyle.GridLayout);
    }

    /// <summary>Called when <see cref="Manosaba.Characters.TonoHanna.Powers.PuppetCollectionSummaryPower"/> increases (a new distinct puppet appears in the collection).</summary>
    public static async Task OnPuppetCollectionIncreasedAsync(Creature owner, decimal positiveDelta)
    {
        if (positiveDelta <= 0m || TestMode.IsOn)
        {
            return;
        }

        Player? player = owner.Player;
        Frugal? frugal = player?.GetRelic<Frugal>();
        if (frugal == null || frugal.RelicLevel < 2)
        {
            return;
        }

        decimal majoka = 10m * positiveDelta;
        await PowerCmd.Apply<MajokaPower>(owner, majoka, owner, null);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Creature.Side)
        {
            return;
        }

        if (!Owner.Creature.IsAlive)
        {
            return;
        }

        if (Owner.PlayerCombatState == null)
        {
            return;
        }

        int energyLeft = Owner.PlayerCombatState.Energy;
        if (energyLeft <= 0)
        {
            return;
        }

        int capRemaining = MaxUnspentEnergyForGoldPerCombat - _energyChargesConvertedThisCombat;
        if (capRemaining <= 0)
        {
            return;
        }

        int toConvert = Math.Min(energyLeft, capRemaining);
        _energyChargesConvertedThisCombat += toConvert;

        int gold = toConvert * DynamicVars["GoldPerEnergy"].IntValue;
        if (gold <= 0)
        {
            return;
        }

        await PlayerCmd.GainGold(gold, Owner);
    }
}
