using _1.Scripts.DOTS.System.Jobs;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Components___Tags;


namespace _1.Scripts.DOTS.System
{
    public partial struct RuleSystem : ISystem
    {
        EntityQuery behaviorTagQuery;
        EntityQuery unitQuery;
        EntityQuery tileQuery;
        EntityQuery spawnerQuery;

        Entity spawnerEntity;

        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;
        ComponentLookup<StartPause> startLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            behaviorTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<AttackTag, MovingTag, LazyTag>().Build(ref state);
            unitQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SampleUnitComponentData>().Build(ref state);
            tileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
            sampleUnitLookup = state.GetComponentLookup<SampleUnitComponentData>(true);
            startLookup = state.GetComponentLookup<StartPause>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            sampleUnitLookup.Update(ref state);
            startLookup.Update(ref state);

            //spawnerEntity = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>()).GetSingletonEntity();

            // Debug.Log(spawnerQuery.CalculateEntityCount());
            // NativeArray<Entity> spawner = spawnerQuery.ToEntityArray(Allocator.TempJob);
            // spawnerEntity = spawner[0];

            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            //Attack Tag, Moving Tag, Lazy Tag 중 하나라도 가진 엔티티가 없을 경우
            if (behaviorTagQuery.IsEmpty)
            {
                MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
                //Debug.Log("Find behavior");

                //행동을 결정해야 함(공격할 타겟 찾기 or 이동할 위치 찾기)
                //Debug.Log("FIND JOB START");
                NativeArray<Entity> sampleUnits = unitQuery.ToEntityArray(Allocator.TempJob);

                FindNearestJob findNearestJob = new()
                {
                    MapMaker = mapMaker,
                    SampleUnits = sampleUnits,
                    SampleUnitComponents = sampleUnitLookup,
                };

                findNearestJob.ScheduleParallel();
                state.Dependency.Complete();
                sampleUnits.Dispose();

                //타일 배열
                //인덱스가 (3, 5)인 타일 = tiles[3 + 5 * mapMaker.number] => 세로로 바뀌었으니까 tiles[3 * number + 5] 일듯?
                NativeArray<Entity> tiles = tileQuery.ToEntityArray(Allocator.TempJob);

                //체력 감소
                foreach (var (unit, target) in SystemAPI.Query<RefRO<SampleUnitComponentData>, RefRW<TargetEntityData>>().WithAll<AttackTag>())
                {
                    SystemAPI.GetComponentRW<SampleUnitComponentData>(target.ValueRW.targetEntity).ValueRW.hp -= unit.ValueRO.dmg;
                }

                //체력 0인 유닛 파괴
                EntityCommandBuffer ecb = new(Allocator.Temp);
                foreach (var (unit, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>>().WithEntityAccess())
                {
                    if (unit.ValueRW.hp <= 0)
                    {
                        SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[unit.ValueRO.index.x + unit.ValueRO.index.y * mapMaker.number]).ValueRW.soldier = 0;
                        ecb.DestroyEntity(entity);
                    }
                }
                ecb.Playback(state.EntityManager);

                NativeArray<int2> moves = new(2, Allocator.Temp);
                //Attack을 하지 않은 유닛들이 순차적으로 이동
                for (int i = 0; i < 2; i++)
                {
                    foreach (var (unit, target, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>, RefRO<TargetEntityData>>()
                        .WithDisabled<AttackTag>().WithDisabled<MovingTag>().WithEntityAccess())
                    {
                        if (!state.EntityManager.Exists(target.ValueRO.targetEntity)
                            || !SystemAPI.HasComponent<SampleUnitComponentData>(target.ValueRO.targetEntity))
                            continue;

                        int2 targetIndex = SystemAPI.GetComponentRO<SampleUnitComponentData>(target.ValueRO.targetEntity).ValueRO.index;
                        int dx = targetIndex.x - unit.ValueRO.index.x;
                        int dy = targetIndex.y - unit.ValueRO.index.y;
                        moves[0] = new int2((int)math.sign(dx), 0);
                        moves[1] = new int2(0, (int)math.sign(dy));
                        int2 unitIndex = unit.ValueRO.index;
                        RefRW<MapTileAuthoringComponentData> currentTile = SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[unitIndex.x + unitIndex.y * mapMaker.number]);
                        for (int j = 0; j < moves.Length; j++)
                        {
                            if (moves[j].x != 0 || moves[j].y != 0)
                            {
                                RefRW<MapTileAuthoringComponentData> nextTile = SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[(unitIndex.x + moves[j].x) + (unitIndex.y + moves[j].y) * mapMaker.number]);
                                if (nextTile.ValueRO.soldier == 0)
                                {
                                    unit.ValueRW.destIndex = nextTile.ValueRO.index;
                                    currentTile.ValueRW.soldier = 0;
                                    nextTile.ValueRW.soldier = 1;
                                    SystemAPI.SetComponentEnabled<MovingTag>(entity, true);
                                    break;
                                }
                            }
                        }

                        if (i == 0 && !SystemAPI.IsComponentEnabled<MovingTag>(entity))
                            SystemAPI.SetComponentEnabled<LazyTag>(entity, true);
                        else SystemAPI.SetComponentEnabled<LazyTag>(entity, false);
                    }
                }

                tiles.Dispose();

                //테스트용(현재 Attack Tag를 제거하는 로직이 따로 없기 때문에 여기서 바로 제거)
                foreach (var attackTag in SystemAPI.Query<EnabledRefRW<AttackTag>>())
                {
                    attackTag.ValueRW = false;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}