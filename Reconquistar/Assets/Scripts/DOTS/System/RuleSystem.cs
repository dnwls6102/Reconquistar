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
        EntityQuery priorityMovingTagQuery;
        EntityQuery priorityAttackTagQuery;
        EntityQuery ShootingUnitQuery;

        Entity spawnerEntity;

        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;
        ComponentLookup<StartPause> startLookup;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            // 자유 이동 태그를 갖는 유닛들을 긁어 모으는 쿼리
            priorityMovingTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMovingTag>().Build(ref state);
            // 자유 공격 태그를 갖는 유닛들을 긁어 모으는 쿼리
            priorityAttackTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityAttackTag>().Build(ref state);
            // 원거리 유닛들을 긁어 모으는 쿼리
            ShootingUnitQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<ShootTag>().Build(ref state);
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

            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
            //타일 배열
            //인덱스가 (3, 5)인 타일 = tiles[3 + 5 * mapMaker.number]
            NativeArray<Entity> tiles = tileQuery.ToEntityArray(Allocator.TempJob);
            //목표 설정을 위해 모든 유닛들을 긁어온 Entity 배열
            NativeArray<Entity> sampleUnits = unitQuery.ToEntityArray(Allocator.TempJob);
            //이동 판정을 위한 moves array
            NativeArray<int2> moves = new(2, Allocator.Temp);

            // (한 팀의 전투 병력이 절반 이하로 떨어질 경우) 사기 스탯 체크 후 도주 판단

            // 1번 우선순위 : 자유 이동 태그 유닛의 이동
            if (!priorityMovingTagQuery.IsEmpty) // 자유 이동 태그를 가진 유닛이 한 명이라도 있을 경우
            {
                NativeArray<Entity> pMoveUnits = priorityMovingTagQuery.ToEntityArray(Allocator.TempJob);
                //new FindPriorityMoveJob 할당
                FindPriorityMoveJob findPMoveJob = new() //자유 이동 태그 유닛을 위한 findNearestJob
                {
                    MapMaker = mapMaker,
                    SampleUnits = sampleUnits,
                    SampleUnitComponents = sampleUnitLookup,
                    //TargetEntityComponents = targetEntityLookup,
                    //FlipComponents = flipLookup, //컴포넌트룩업은 엔티티의 샘플유닛 컴포넌트 인스턴스를 뽑아오기 위함이니 재활용
                };
                findPMoveJob.ScheduleParallel();
                state.Dependency.Complete();
                pMoveUnits.Dispose();

                //Debug.Log("자유 이동 분기 작동");

                for (int i = 0; i < 2; i++)
                {
                    //자유 이동 태그를 갖고 있으면서, 현재 MovingTag가 비활성화된 유닛들
                    foreach (var (unit, target, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>, RefRO<TargetEntityData>>()
                        .WithAll<PriorityMovingTag>().WithDisabled<MovingTag>().WithEntityAccess())
                    {

                        if (!state.EntityManager.Exists(target.ValueRO.targetEntity) //이 유닛의 target 엔티티가 없는 경우
                            || !SystemAPI.HasComponent<SampleUnitComponentData>(target.ValueRO.targetEntity)) //이 유닛의 target 엔티티가 SampleUnit이 아닌 경우
                            continue;

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
                                    SystemAPI.SetComponentEnabled<MovingTag>(entity, true); // MovingTag를 붙여 MovementJob이 일어나게끔 함
                                    break;
                                }
                            }
                        }

                        if (i == 0 && !SystemAPI.IsComponentEnabled<MovingTag>(entity)) // 첫 번째 루프를 도는 중 MovingTag가 붙여지지 않은 entity의 경우 LazyTag 붙이기
                            SystemAPI.SetComponentEnabled<LazyTag>(entity, true);
                        else SystemAPI.SetComponentEnabled<LazyTag>(entity, false); // 그 외의 경우 LazyTag 비활성화
                    }
                }
            }

            //2번 우선순위: 사격 무기의 재장전
            //사격 무기를 갖고 있는 유닛들에 별도의 태그 및 총알 갯수를 저장하는 변수를 부여하여
            //총알 갯수가 0개이면 재장전하는 식으로
            //System에서 한번에 처리하지 말고 Job으로 할당
            if (!ShootingUnitQuery.IsEmpty)
            {
                ReloadJob reloadJob = new();
                reloadJob.ScheduleParallel();
            }

            //3번 우선순위: (치료 등 기타 특수능력의) 능력 범위 안에 목표가 있을 시 능력 사용
            Debug.Log("Step 3");
            //Attack Tag, Moving Tag, Lazy Tag 중 하나라도 활성화된 엔티티가 없을 경우
            if (behaviorTagQuery.IsEmpty)
            {
                //Debug.Log("Find behavior");

                //행동을 결정해야 함(공격할 타겟 찾기 -> 이동할 위치 찾기)
                //Debug.Log("FIND JOB START");

                FindNearestJob findNearestJob = new()
                {
                    MapMaker = mapMaker,
                    SampleUnits = sampleUnits,
                    SampleUnitComponents = sampleUnitLookup,
                };

                findNearestJob.ScheduleParallel();
                state.Dependency.Complete();
                sampleUnits.Dispose();

                //체력 감소 == 전투 (일반행동 우선순위 4)
                Debug.Log("Step 4");
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
                        Debug.Log("Delete");
                    }
                }
                ecb.Playback(state.EntityManager);

                //Attack을 하지 않은 유닛들이 순차적으로 이동(일반행동 우선순위 5)
                Debug.Log("Step 5");
                for (int i = 0; i < 2; i++)
                {
                    //AttackTag가 비활성화되고 MovingTag가 비활성화된 유닛들
                    foreach (var (unit, target, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>, RefRO<TargetEntityData>>()
                        .WithDisabled<AttackTag>().WithDisabled<MovingTag>().WithEntityAccess())
                    {

                        if (!state.EntityManager.Exists(target.ValueRO.targetEntity) //이 유닛의 target 엔티티가 없는 경우
                            || !SystemAPI.HasComponent<SampleUnitComponentData>(target.ValueRO.targetEntity)) //이 유닛의 target 엔티티가 SampleUnit이 아닌 경우
                            continue;

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
                                    SystemAPI.SetComponentEnabled<MovingTag>(entity, true); // MovingTag를 붙여 MovementJob이 일어나게끔 함
                                    break;
                                }
                            }
                        }

                        if (i == 0 && !SystemAPI.IsComponentEnabled<MovingTag>(entity)) // 첫 번째 루프를 도는 중 MovingTag가 붙여지지 않은 entity의 경우 LazyTag 붙이기
                            SystemAPI.SetComponentEnabled<LazyTag>(entity, true);
                        else SystemAPI.SetComponentEnabled<LazyTag>(entity, false); // 그 외의 경우 LazyTag 비활성화
                    }
                }

                tiles.Dispose();

                //테스트용(현재 Attack Tag를 제거하는 로직이 따로 없기 때문에 여기서 바로 제거)
                foreach (var attackTag in SystemAPI.Query<EnabledRefRW<AttackTag>>())
                {
                    attackTag.ValueRW = false;
                }

                //6번 우선순위 : 상기한 모든 행동을 할 수 없는 경우 lazy tag를 부여


                //자유 공격 태그 유닛들의 공격
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}