using _1.Scripts.DOTS.Components___Tags;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public AnimationSettings AnimationSettings;
        public double Time;
        
        public void Execute(AnimatorAspect animator, EnabledRefRO<MovingTag> movingTagEnabled)
        {
            animator.SetAnimation(movingTagEnabled.ValueRO ? AnimationSettings.WalkHash : AnimationSettings.IdleHash, Time);
        }
    }
}