using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.Common.Powers;

public abstract class ManosabaTemporaryDexterityPower : PathCustomPowerModel
{
    private bool _shouldIgnoreNextInstance;

    public override PowerType Type => IsPositive ? PowerType.Buff : PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public abstract AbstractModel OriginModel { get; }

    public PowerModel InternallyAppliedPower => ModelDb.Power<DexterityPower>();

    protected virtual bool IsPositive => true;

    private int Sign => IsPositive ? 1 : -1;

    public override LocString Title
    {
        get
        {
            AbstractModel originModel = OriginModel;
            if (originModel is CardModel cardModel)
                return cardModel.TitleLocString;

            if (originModel is PotionModel potionModel)
                return potionModel.Title;

            if (originModel is RelicModel relicModel)
                return relicModel.Title;

            if (originModel is OrbModel orbModel)
                return orbModel.Title;

            throw new InvalidOperationException();
        }
    }

    public override LocString Description => new("powers", IsPositive ? "TEMPORARY_DEXTERITY_POWER.description" : "TEMPORARY_DEXTERITY_DOWN.description");

    protected override string SmartDescriptionLocKey => IsPositive ? "TEMPORARY_DEXTERITY_POWER.smartDescription" : "TEMPORARY_DEXTERITY_DOWN.smartDescription";

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> tips = [];
            IEnumerable<IHoverTip> originTips = OriginModel switch
            {
                CardModel card => [HoverTipFactory.FromCard(card)],
                PotionModel potion => [HoverTipFactory.FromPotion(potion)],
                RelicModel relic => HoverTipFactory.FromRelic(relic),
                OrbModel orb => [orb.DumbHoverTip],
                _ => throw new InvalidOperationException()
            };

            tips.AddRange(originTips);
            tips.Add(HoverTipFactory.FromPower<DexterityPower>());
            return tips;
        }
    }

    public void IgnoreNextInstance()
    {
        _shouldIgnoreNextInstance = true;
    }

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<DexterityPower>(target, Sign * amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount == Amount)
            return;

        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<DexterityPower>(Owner, Sign * amount, applier, cardSource, silent: true);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<DexterityPower>(Owner, -Sign * Amount, Owner, null);
    }
}
