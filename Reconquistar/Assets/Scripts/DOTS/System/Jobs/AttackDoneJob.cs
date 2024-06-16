using _1.Scripts.DOTS.Components___Tags;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct AttackDoneJob : IJobEntity
    {
        public AnimationSettings AnimationSettings;
        public double Time;
        public void Execute(AnimatorAspect animator, EnabledRefRW<AttackTag> attackTagEnabled,
            EnabledRefRW<NormalActionDoneTag> normalActionDonTag, EnabledRefRW<AttackDoneTag> attackDoneTag)
        {
            if(attackTagEnabled.ValueRO == false)
            {
                return;
            }
            attackTagEnabled.ValueRW = false;
            attackDoneTag.ValueRW = true;
            normalActionDonTag.ValueRW = true;
            animator.SetAnimation(AnimationSettings.IdleHash, Time);
        }
    }
}