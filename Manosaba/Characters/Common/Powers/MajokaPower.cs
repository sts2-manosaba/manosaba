using BaseLib.Extensions;
using Manosaba.Characters.HikamiMeruru.Cards;
using Manosaba.Characters.JogasakiNoahCard.Cards;
using Manosaba.Characters.NikaidoHiro.Cards;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Characters.TonoHanna.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers
{
    public class MajokaPower : PathCustomPowerModel
    {
        // Damage scaling follows a saturating curve (Hill function) that approaches 4x total damage.
        private const decimal MaxTotalDamageMultiplier = 4m;
        // Tuned anchors:
        // - ~2.75x at 200 stacks
        // - ~3.5x at 400 stacks
        private const decimal CurveHalfSaturationStacks = 166.6m;
        private const decimal CurveExponent = 1.8365m;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public static Dictionary<string, Type> mahouCardsMap = new()
        {
            { "nikaido_hiro",  typeof(DeathLoop)},
            { "hikami_meruru", typeof(GrandHeal)},
            { "jogasaki_noah", typeof(LiquidManipulation)},
            { "tachibana_sherry", typeof(SuperPunch)},
            { "saeki_miria", typeof(MemorySharing)},
            { "tono_hanna", typeof(SkyIsland)},
        };

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (base.Owner != dealer)
            {
                return 1m;
            }

            if (props.HasFlag(ValueProp.Unpowered))
            {
                return 1m;
            }

            return GetMajokaDamageMultiplier(base.Amount);
        }

        private static decimal GetMajokaDamageMultiplier(decimal stacks)
        {
            if (stacks <= 0m)
            {
                return 1m;
            }

            // ratio = s^p / (s^p + k^p), where:
            // - s = stacks
            // - p = CurveExponent
            // - k = CurveHalfSaturationStacks
            // This yields a smooth monotonic curve that asymptotically approaches the cap.
            double sPow = Math.Pow((double)stacks, (double)CurveExponent);
            double kPow = Math.Pow((double)CurveHalfSaturationStacks, (double)CurveExponent);
            double ratio = sPow / (sPow + kPow);
            decimal maxBonus = MaxTotalDamageMultiplier - 1m;
            return 1m + maxBonus * (decimal)ratio;
        }

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {

            int toApplyMI = base.Amount / 100 - base.Owner.GetPowerAmount<MurderousImpulsePower>();
            await PowerCmd.Apply<MurderousImpulsePower>(base.Owner.Player.Creature, toApplyMI, base.Owner.Player.Creature, null);
            await CheckAndGiveMahouCards();
        }

        private async Task CheckAndGiveMahouCards()
        {
            if (base.Amount >= 100 && Owner.Player != null)
            {
                String characterId = Owner.Player.Character.Id.ToString().RemovePrefix().ToLowerInvariant();
                if (!mahouCardsMap.TryGetValue(characterId, out Type targetType))
                    return;
                if (Owner.Player.Deck.Cards.Count(c => c.GetType() == targetType) < 1)
                {
                    CardModel cardType = ModelDb.GetById<CardModel>(ModelDb.GetId(targetType));
                    CardModel cardToDeck = Owner.Player.RunState.CreateCard(cardType, Owner.Player);
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardToDeck, PileType.Deck), 1.2f, CardPreviewStyle.GridLayout);
                    CardModel cardToHand = Owner.CombatState.CreateCard(cardType, Owner.Player);
                    await CardPileCmd.AddGeneratedCardToCombat(cardToHand, PileType.Hand, true);
                }
            }
        }

    }
}
