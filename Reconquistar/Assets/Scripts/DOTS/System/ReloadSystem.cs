using _1.Scripts.DOTS.System.Jobs;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Components___Tags;

public partial struct ReloadSystem : ISystem
{
    EntityQuery spawnerQuery;
    EntityQuery priorityMoveDoneQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SampleSpawnData>();
        spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
        priorityMoveDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<PriorityMoveDoneTag>().Build(ref state);
    }


    public void OnUpdate(ref SystemState state)
    {
        if (spawnerQuery.CalculateEntityCount() == 0)
        {
            return;
        }
        //이전 행동(자유이동)이 완료 되었는가?
        if (priorityMoveDoneQuery.IsEmpty)
        {
            Debug.Log("2");
            ReloadJob reloadJob = new();
            reloadJob.ScheduleParallel();
            state.Dependency.Complete();
        }
    }
}
