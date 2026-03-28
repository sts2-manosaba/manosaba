using BaseLib.Abstracts;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace Manosaba.Characters.Common.Monsters
{
    public sealed class SakurabaEmaDog : MonsterModel, ICustomModel
    {
        public override int MinInitialHp => 1;
        public override int MaxInitialHp => 1;
        public override bool IsHealthBarVisible => true;
        public override string? HurtSfx => "event:/Manosaba/sfx/sakuraba_ema_dog_hurt.ogg";

        protected override string VisualsPath => "sakuraba_ema_dog.tscn".MonsterScenePath();
        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            MoveState moveState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
            moveState.FollowUpState = moveState;
            return new MonsterMoveStateMachine([moveState], moveState);
        }
    }
}
