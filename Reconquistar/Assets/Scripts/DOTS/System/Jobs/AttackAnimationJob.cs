using _1.Scripts.DOTS.Components___Tags;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.System.Jobs
{    
    [BurstCompile]
    public partial struct AttackAnimationJob : IJobEntity
    {
        public AnimationSettings AnimationSettings;
        public double Time;
        public void Execute(AnimatorAspect animator, EnabledRefRW<AttackTag> attackTagEnabled)
        {
            if(attackTagEnabled.ValueRO == true)
            animator.SetAnimation(AnimationSettings.AttackHash, Time);
        } 
    }
    
}