using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Commands;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class TheWayOut : PathCustomCardModel
{
    private const string VfxScenePath = "res://Manosaba/scenes/saeki_miria/vfx/the_way_out.tscn";
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public TheWayOut()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        CardPile? pile = Pile;
        if (CombatState == null || pile == null)
        {
            return;
        }

        if ((pile.Type == PileType.Discard || pile.Type == PileType.Hand || pile.Type == PileType.Draw || pile.Type == PileType.Exhaust)
            && side == CombatSide.Player)
        {
            await ManosabaVfxCmd.PlaySceneAtCombatCenterAndWait(VfxScenePath, fitCoverViewport: true, spriteNodeNames: ["StillA","StillB"]);
            await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(CombatState);
        }
    }
}
