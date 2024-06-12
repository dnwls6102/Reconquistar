using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace _1.Scripts.DOTS.System
{
    public partial struct NormalActionSystem : ISystem
    {
        EntityQuery priorityMoveDoneQuery;
        EntityQuery reloadingDoneQuery;
        EntityQuery attackDoneQuery;
        EntityQuery spawnerQuery;
        EntityQuery unitQuery;
        EntityQuery tileQuery;
        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;
        void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SampleUnitComponentData>();
            priorityMoveDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<PriorityMoveDoneTag>().Build(ref state);
            reloadingDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<ReloadingDoneTag>().Build(ref state);
            attackDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<AttackDoneTag>().Build(ref state);
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
            unitQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SampleUnitComponentData>().Build(ref state);
            tileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
            sampleUnitLookup = state.GetComponentLookup<SampleUnitComponentData>(true);
        }

        void OnUpdate(ref SystemState state)
        {
            sampleUnitLookup.Update(ref state);
            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            if (priorityMoveDoneQuery.IsEmpty && reloadingDoneQuery.IsEmpty)
            {
                Debug.Log("일반 행동 시작");
                MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
                NativeArray<Entity> sampleUnits = unitQuery.ToEntityArray(Allocator.TempJob);
                NativeArray<int2> moves = new(2, Allocator.Temp);
                NativeArray<Entity> tiles = tileQuery.ToEntityArray(Allocator.TempJob);
                FindNearestJob findNearestJob = new()
                {
                    MapMaker = mapMaker,
                    SampleUnits = sampleUnits,
                    SampleUnitComponents = sampleUnitLookup,
                };
                findNearestJob.ScheduleParallel();
                state.Dependency.Complete();
                sampleUnits.Dispose();

                EntityCommandBuffer ecb = new(Allocator.TempJob);

                //총알 생성
                foreach (var (bullet, transform, unitEntity) in SystemAPI.Query<RefRO<ShootTag>, RefRW<LocalTransform>>().WithAll<AttackTag>().WithEntityAccess())
                {
                    var temp = state.EntityManager.Instantiate(bullet.ValueRO.BulletEntity); //query로 받아온 ShootTag 컴포넌트의 bulletEntity 생성
                    state.EntityManager.SetComponentData(temp, new LocalTransform()
                    {
                        Position = SystemAPI.GetComponent<LocalTransform>(unitEntity).Position,
                        Rotation = quaternion.identity,
                        Scale = SystemAPI.GetComponent<LocalTransform>(unitEntity).Scale,
                    }); //총알 엔티티의 transform을 원거리 유닛의 transform으로 설정

                }
                //공격
                //attackTag의 Flag는 어디서 세워지는 것인지? => FindNearestJob에서 세워짐
                //현재 공격이 이루어지지 않는 이유 : 공격이 단 한번이라도 이루어지지 않는다면 AttackDoneTag는 언제나 비활성화된 상태이니, foreach로 잡히지 않는다. 
                foreach (var (unit, target, attackTag, entity) in SystemAPI.Query<RefRO<SampleUnitComponentData>, RefRW<TargetEntityData>, EnabledRefRW<AttackTag>>().WithAll<AttackTag>().WithDisabled<AttackDoneTag>().WithEntityAccess())//, EnabledRefRW<AttackDoneTag>, EnabledRefRW<AttackTag>>().WithAll<AttackTag>())
                {
                    Debug.Log("공격");
                    SystemAPI.GetComponentRW<SampleUnitComponentData>(target.ValueRW.targetEntity).ValueRW.hp -= unit.ValueRO.dmg;
                    attackTag.ValueRW = false; //attackTag의 비활성화는 정상적으로 진행됨. 빨라서 안보이는거임
                    SystemAPI.SetComponentEnabled<AttackDoneTag>(entity, true);
                    //Debug.Log("attackTagValue : " + attackTag.ValueRW);
                    //doneTag.ValueRW = true; //사거리 밖의 유닛들에 대해서 행동 완료 처리: FindNearestJob, FinrPriorityMoveJob에서 진행
                }

                //Job 할당 자체가 안되는 문제
                //같은 이유로 Job 할당은 되는데, 조건에 충족하는 유닛이 없으니 Execute가 실행되지 않는거임
                /*AttackJob attackJob = new()
                {
                    SampleUnitComponents = sampleUnitLookup,
                    ecb = ecb.AsParallelWriter(),
                };
                attackJob.ScheduleParallel();
                state.Dependency.Complete();*/

                //체력 0인 유닛 파괴

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


                if (attackDoneQuery.IsEmpty)
                {
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
                                        SystemAPI.SetComponentEnabled<NormalActionDoneTag>(entity, false);
                                        break;
                                    }
                                    SystemAPI.SetComponentEnabled<NormalActionDoneTag>(entity, true);

                                }
                            }



                            if (i == 0 && !SystemAPI.IsComponentEnabled<MovingTag>(entity)) // 첫 번째 루프를 도는 중 MovingTag가 붙여지지 않은 entity의 경우 LazyTag 붙이기
                                SystemAPI.SetComponentEnabled<LazyTag>(entity, true);
                            else SystemAPI.SetComponentEnabled<LazyTag>(entity, false); // 그 외의 경우 LazyTag 비활성화
                        }
                    }

                    tiles.Dispose();

                    new MovementJob
                    {
                        Time = (float)SystemAPI.Time.DeltaTime,
                        MapMaker = mapMaker
                    }.ScheduleParallel();
                    state.Dependency.Complete();
                }

            }
        }
    }
}
