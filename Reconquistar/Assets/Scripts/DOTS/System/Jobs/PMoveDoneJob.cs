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
    public partial struct PMoveDoneJob : IJobEntity
    {
        //자유 이동 태그를 가지고 있는 모든 유닛들의 DoneTag를 세우기
        public void Execute(EnabledRefRW<PriorityMoveDoneTag> PMoveDoneTag)
        {
            PMoveDoneTag.ValueRW = true;
        }
    }
}