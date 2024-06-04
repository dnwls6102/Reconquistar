using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Collections;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct DoneTagResetJob : IJobEntity
    {
        //현재 true로 설정되어 있는 모든 DoneTag들을 false로 바꾸기
        public void Execute(EnabledRefRW<PriorityMoveDoneTag> PMoveDoneTag, EnabledRefRW<PriorityAttackDoneTag> PAttackDoneTag, EnabledRefRW<NormalActionDoneTag> NormalActionDoneTag)
        {
            PMoveDoneTag.ValueRW = false;
            PAttackDoneTag.ValueRW = false;
            NormalActionDoneTag.ValueRW = false;
        }
    }
}