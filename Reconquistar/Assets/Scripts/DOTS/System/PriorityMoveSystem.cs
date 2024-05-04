using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.System
{
    public partial struct PriorityMoveSystem : ISystem
    {
        EntityQuery PMovingTagQuery; //자유 이동 태그 활성화된 유닛들 쿼리 
        EntityQuery unitQuery;
        EntityQuery spawnerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //
            state.RequireForUpdate<MapMakerComponentData>();
            PMovingTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMovingTag>().Build(ref state);
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
            //var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            if (!PMovingTagQuery.IsEmpty)
            {
                // MovementJob을 재활용할지, 별도의 자유 이동 Job을 만들지
                // MovementJob을 재활용할 경우 PriorityMoveSystem은 필요없음
                //Debug.Log("자유 이동");
                new MovementJob
                {
                    Time = (float)SystemAPI.Time.DeltaTime,
                    MapMaker = mapMaker
                    //ECBWriter = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                }.ScheduleParallel();
                state.Dependency.Complete();
            }

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}