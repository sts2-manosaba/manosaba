using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.Common.Encounters;

public sealed class BasementGuardianEventEncounter : EncounterModel
{
    private bool _ranOutOfTime;

    public override RoomType RoomType => RoomType.Monster;

    // When the guardian escapes (RanOutOfTime), vanilla NCombatUi treats this as no reward payout at all
    // (ProceedWithoutRewards — no gold, cards, potion, or ExtraRewards).
    public override bool ShouldGiveRewards => !RanOutOfTime;

    public bool RanOutOfTime
    {
        get => _ranOutOfTime;
        set
        {
            AssertMutable();
            _ranOutOfTime = value;
        }
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<Monsters.BasementGuardian>(),
    ];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<Monsters.BasementGuardian>().ToMutable(), null),
        ];
    }

    public override Dictionary<string, string> SaveCustomState()
    {
        return new Dictionary<string, string>
        {
            ["RanOutOfTime"] = RanOutOfTime.ToString(),
        };
    }

    public override void LoadCustomState(Dictionary<string, string> state)
    {
        RanOutOfTime = bool.Parse(state["RanOutOfTime"]);
    }
}
