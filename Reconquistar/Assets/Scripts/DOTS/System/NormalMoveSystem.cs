using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace _1.Scripts.DOTS.System
{
    public partial struct NormalMoveSystem : ISystem
    {
        
        EntityQuery priorityMoveDoneQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SampleUnitComponentData>();
            state.RequireForUpdate<MovingTag>();
            priorityMoveDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<PriorityMoveDoneTag>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (priorityMoveDoneQuery.IsEmpty)
            {
                MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
                new MovementJob
                {
                    Time = (float)SystemAPI.Time.DeltaTime,
                    MapMaker = mapMaker
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