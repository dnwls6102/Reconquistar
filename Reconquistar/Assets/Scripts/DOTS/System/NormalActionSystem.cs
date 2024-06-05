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

                EntityCommandBuffer ecb = new(Allocator.Temp);

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
                foreach (var (unit, target) in SystemAPI.Query<RefRO<SampleUnitComponentData>, RefRW<TargetEntityData>>().WithAll<AttackTag>())
                {
                    SystemAPI.GetComponentRW<SampleUnitComponentData>(target.ValueRW.targetEntity).ValueRW.hp -= unit.ValueRO.dmg;
                }

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

            }
        }
    }
}
