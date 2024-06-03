using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace _1.Scripts.DOTS.System
{
    public partial struct PriorityMovementSystem : ISystem
    {
        EntityQuery priorityMoveDoneWithAnyQuery;
        EntityQuery priorityAttackDoneWithAnyQuery;
        EntityQuery normalActionDoneWithAnyQuery;
        EntityQuery priorityMovingTagQuery;
        EntityQuery spawnerQuery;
        EntityQuery tileQuery;
        EntityQuery unitQuery;
        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SampleUnitComponentData>();
            //DoneWithAnyQuery : 활성화된 행동 완료 태그들을 수집 --> 해당 쿼리들이 비어있다면 행동 완료한 유닛들이 없다는 의미 == 초기화가 잘 진행되었다는 의미
            priorityMoveDoneWithAnyQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMoveDoneTag>().Build(ref state);
            priorityAttackDoneWithAnyQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityAttackDoneTag>().Build(ref state);
            normalActionDoneWithAnyQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<NormalActionDoneTag>().Build(ref state);
            priorityMovingTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMovingTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
            tileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
            unitQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SampleUnitComponentData>().Build(ref state);
            sampleUnitLookup = state.GetComponentLookup<SampleUnitComponentData>(true);
        }

        // Update is called once per frame
        public void OnUpdate(ref SystemState state)
        {
            sampleUnitLookup.Update(ref state);
            //GameManager역할을 하는 SampleSpawner가 생성되지 않으면 Update구문을 실행시키지 않음
            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
            NativeArray<Entity> tiles = tileQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<Entity> sampleUnits = unitQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<int2> moves = new(2, Allocator.Temp);

            //모든 유닛들의 행동 완료 태그 초기화 작업이 잘 이루어졌는가?
           // if (priorityMoveDoneWithAnyQuery.IsEmpty && priorityAttackDoneWithAnyQuery.IsEmpty && normalActionDoneWithAnyQuery.IsEmpty)
            {
                Debug.Log("조건 통과");
                Debug.Log("" + priorityMovingTagQuery.IsEmpty);
                if(!priorityMovingTagQuery.IsEmpty)
                {
                    Debug.Log("자유 이동 분기 작동");
                    //new FindPriorityMoveJob 할당
                    FindPriorityMoveJob findPMoveJob = new() //자유 이동 태그 유닛을 위한 findNearestJob
                    {
                        MapMaker = mapMaker,
                        SampleUnits = sampleUnits,
                        SampleUnitComponents = sampleUnitLookup,
                    };
                    findPMoveJob.ScheduleParallel();
                    state.Dependency.Complete();
                }
                //기존의 타일 soldier값 수정하는 부분을 job으로 돌리기?
                for (int i = 0; i < 2; i++)
                {
                    //자유 이동 태그를 갖고 있으면서, 현재 MovingTag가 비활성화된 유닛들
                    foreach (var (unit, target, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>, RefRO<TargetEntityData>>()
                                 .WithDisabled<PriorityMovingTag>().WithEntityAccess())
                    {

                        if (!state.EntityManager.Exists(target.ValueRO.targetEntity) ||
                            !SystemAPI.HasComponent<SampleUnitComponentData>(target.ValueRO.targetEntity))
                        {
                            Debug.Log("11");
                            SystemAPI.SetComponentEnabled<PriorityMoveDoneTag>(entity, true);
                            continue;
                        } //이 유닛의 target 엔티티가 없는 경우 또는 이 유닛의 target 엔티티가 SampleUnit이 아닌 경우

                        int2 targetIndex = SystemAPI.GetComponentRO<SampleUnitComponentData>(target.ValueRO.targetEntity).ValueRO.index; //이 유닛의 target 엔티티의 위치 정보
                        int dx = targetIndex.x - unit.ValueRO.index.x;
                        int dy = targetIndex.y - unit.ValueRO.index.y; //target엔티티의 Index - 현재 이 유닛의 index
                        moves[0] = new int2((int)math.sign(dx), 0); //dx가 음수일 경우 -1, 그 외 1
                        moves[1] = new int2(0, (int)math.sign(dy)); //dy가 음수일 경우 -1, 그 외 1
                        int2 unitIndex = unit.ValueRO.index; //현재 unit의 index

                        //현재 유닛이 점거하고 있는 타일의 index정보를 받아옴
                        RefRW<MapTileAuthoringComponentData> currentTile = SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[unitIndex.x + unitIndex.y * mapMaker.number]);
                        for (int j = 0; j < moves.Length; j++)
                        {
                            if (moves[j].x != 0 || moves[j].y != 0)
                            {
                                RefRW<MapTileAuthoringComponentData> nextTile = SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[(unitIndex.x + moves[j].x) + (unitIndex.y + moves[j].y) * mapMaker.number]);
                                if (nextTile.ValueRO.soldier == 0) // 만약 이 유닛이 이동하려는 다음 맵 타일에 유닛이 없을 경우
                                {
                                    unit.ValueRW.destIndex = nextTile.ValueRO.index; // 이 유닛의 destIndex를 nextTile의 Index로 설정
                                    currentTile.ValueRW.soldier = 0; // 현재 점거중인 타일의 soldier값을 0으로 설정하여 뒤에 있는 유닛이 자유롭게 이동하게끔 설정
                                    nextTile.ValueRW.soldier = 1; // 이동하려는 다음 맵 타일의 soldier값을 1로 설정
                                    SystemAPI.SetComponentEnabled<PriorityMovingTag>(entity, true); // PriorityMovingTag를 붙여 MovementJob이 일어나게끔 함
                                    SystemAPI.SetComponentEnabled<PriorityMoveDoneTag>(entity, false);
                                    break;
                                }
                                {
                                    //Debug.Log("11");
                                    SystemAPI.SetComponentEnabled<PriorityMoveDoneTag>(entity, true);
                                }
                            }
                        }
                    }
                }
                new PriorityMovementJob()
                {
                    Time = (float)SystemAPI.Time.DeltaTime,
                    MapMaker = mapMaker
                    //ECBWriter = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                }.ScheduleParallel();
                state.Dependency.Complete();
            }
        }
    }
}
