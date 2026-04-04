using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Commands;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    public abstract class NanokaPuzzleQuestTokenBase : PathCustomCardModel
    {
        private const string VfxScenePath = "res://Manosaba/scenes/hikami_meruru/vfx/nanoka_complete.tscn";
        private const int EnergyCost = -1;
        private const CardType CardTypeValue = CardType.Quest;
        private const CardRarity Rarity = CardRarity.Token;
        private const TargetType TargetTypeValue = TargetType.None;
        private const bool ShouldShowInCardLibrary = false;

        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Innate, CardKeyword.Retain];

        protected NanokaPuzzleQuestTokenBase() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
        {
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            CombatManager? combatManager = CombatManager.Instance;
            CombatState? combatState = CombatState;
            Player? owner = Owner;
            Creature? ownerCreature = owner?.Creature;
            if (combatManager == null || combatState == null || owner == null || ownerCreature == null || combatManager.IsEnding)
            {
                return;
            }

            // Only one token should execute the win flow to avoid duplicate triggers.
            if (this is not NanokaHead)
            {
                return;
            }

            CardPile handPile = PileType.Hand.GetPile(owner);
            IReadOnlyList<CardModel> handCards = handPile?.Cards ?? [];
            CardModel? head = handCards.FirstOrDefault(c => c is NanokaHead);
            CardModel? rightArm = handCards.FirstOrDefault(c => c is NanokaRightArm);
            CardModel? rightLeg = handCards.FirstOrDefault(c => c is NanokaRightLeg);
            CardModel? leftArm = handCards.FirstOrDefault(c => c is NanokaLeftArm);
            CardModel? leftLeg = handCards.FirstOrDefault(c => c is NanokaLeftLeg);

            if (head == null || rightArm == null || rightLeg == null || leftArm == null || leftLeg == null)
            {
                return;
            }

            CardCmd.Preview([head, rightArm, rightLeg, leftArm, leftLeg], 1.5f, CardPreviewStyle.HorizontalLayout);
            await Cmd.Wait(1.55f);

            await CardPileCmd.RemoveFromCombat(head);
            await CardPileCmd.RemoveFromCombat(rightArm);
            await CardPileCmd.RemoveFromCombat(rightLeg);
            await CardPileCmd.RemoveFromCombat(leftArm);
            await CardPileCmd.RemoveFromCombat(leftLeg);

            CardModel complete = combatState.CreateCard<NanokaComplete>(owner);
            CardPileAddResult completeResult = await CardPileCmd.AddGeneratedCardToCombat(
                complete,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Random
            );
            CardCmd.PreviewCardPileAdd(completeResult, 1.2f, CardPreviewStyle.HorizontalLayout);
            await ManosabaVfxCmd.PlaySceneAtCombatCenterAndWait(VfxScenePath, fitCoverViewport: true);
            await CardCmd.AutoPlay(choiceContext, complete, null);
            IReadOnlyList<Creature> hittableEnemies = combatState.GetOpponentsOf(ownerCreature)
                .Where(e => e.IsHittable)
                .ToList();
            if (hittableEnemies.Count == 0)
            {
                return;
            }

            await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(combatState);
        }

        protected override void OnUpgrade()
        {
        }
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaPiece : PathCustomCardModel
    {
        private const int EnergyCost = 0;
        private const CardType CardTypeValue = CardType.Skill;
        private const CardRarity Rarity = CardRarity.Token;
        private const TargetType TargetTypeValue = TargetType.Self;
        private const bool ShouldShowInCardLibrary = false;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public NanokaPiece() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
        {
        }

        public static IEnumerable<NanokaPiece> Create(Player owner, int amount, CombatState combatState)
        {
            List<NanokaPiece> list = [];
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<NanokaPiece>(owner));
            }
            return list;
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
        }
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaHead : NanokaPuzzleQuestTokenBase
    {
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaRightArm : NanokaPuzzleQuestTokenBase
    {
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaRightLeg : NanokaPuzzleQuestTokenBase
    {
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaLeftArm : NanokaPuzzleQuestTokenBase
    {
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaLeftLeg : NanokaPuzzleQuestTokenBase
    {
    }

    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaComplete : PathCustomCardModel
    {
        private const int EnergyCost = 0;
        private const CardType CardTypeValue = CardType.Skill;
        private const CardRarity Rarity = CardRarity.Token;
        private const TargetType TargetTypeValue = TargetType.Self;
        private const bool ShouldShowInCardLibrary = false;

        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        public NanokaComplete() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
        {
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
        }
    }
}
