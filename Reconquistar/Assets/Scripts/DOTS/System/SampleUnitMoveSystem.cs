using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.System
{
    public partial struct SampleUnitMoveSystem : ISystem
    {
        EntityQuery MovingTagQuery;
        EntityQuery PriorityMovingTagQuery;
        EntityQuery unitQuery;
        EntityQuery spawnerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //
            state.RequireForUpdate<MapMakerComponentData>();
            MovingTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<MovingTag>().Build(ref state);
            PriorityMovingTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMovingTag>().Build(ref state);
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
            if (!PriorityMovingTagQuery.IsEmpty && MovingTagQuery.IsEmpty)
            {
                //Debug.Log("Moving");
                new PriorityMovementJob()
                {
                    Time = (float)SystemAPI.Time.DeltaTime,
                    MapMaker = mapMaker
                    //ECBWriter = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                }.ScheduleParallel();
                state.Dependency.Complete();
            }
            if (!MovingTagQuery.IsEmpty)
            {
                //Debug.Log("Moving");
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