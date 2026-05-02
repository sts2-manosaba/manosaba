using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class Linear : PathCustomCardModel
{
    private const string IncreaseKey = "Increase";

    private const int BaseDamage = 13;

    private int _currentDamage = BaseDamage;
    private int _increasedDamage;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, ManosabaKeywords.SwordTechnique];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new IntVar(IncreaseKey, 3m),
    ];

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    public Linear()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, shouldShowInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_dramatic_stab", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        int inc = DynamicVars[IncreaseKey].IntValue;
        BuffFromPlay(inc);
        (DeckVersion as Linear)?.BuffFromPlay(inc);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IncreaseKey].UpgradeValueBy(1m);
    }

    protected override void AfterDowngraded()
    {
        UpdateDamage();
    }

    private void BuffFromPlay(int extraDamage)
    {
        IncreasedDamage += extraDamage;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        CurrentDamage = BaseDamage + IncreasedDamage;
    }
}
