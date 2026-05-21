using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class Zuma : PathCustomCardModel
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<ZumaPower>(),
            HoverTipFactory.FromCard<PaletteGap>()
        ];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ZumaPower>(1), new CardsVar(7)];

        public Zuma() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature || CombatState is not { } combatState)
            {
                return;
            }

            await CommonActions.Apply<ZumaPower>(choiceContext, ownerCreature, this, DynamicVars["ZumaPower"].BaseValue);
            await OrbCmd.AddSlots(Owner, 1);
            List<CardModel> cards = Enumerable.Range(0, DynamicVars.Cards.IntValue)
                .Select(_ => combatState.CreateCard<PaletteGap>(Owner))
                .Cast<CardModel>()
                .ToList();
            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, Owner, CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(results);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
