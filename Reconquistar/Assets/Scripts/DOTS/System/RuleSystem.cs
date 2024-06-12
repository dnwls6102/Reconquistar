using _1.Scripts.DOTS.System.Jobs;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Components___Tags;
using Random = Unity.Mathematics.Random;


namespace _1.Scripts.DOTS.System
{
    public partial struct RuleSystem : ISystem
    {
        EntityQuery NormalBehaviorTagQuery;
        EntityQuery unitQuery;
        EntityQuery tileQuery;
        EntityQuery spawnerQuery;
        EntityQuery priorityMoveDoneQuery;
        EntityQuery reloadingDoneQuery;
        EntityQuery AttackDoneQuery;
        EntityQuery priorityAttackDoneQuery;
        EntityQuery normalActionDoneQuery;
        //EntityQuery pMovingTagQuery;
        Entity spawnerEntity;
        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;
        ComponentLookup<StartPause> startLookup;
        //Random seed;
         ComponentLookup<PriorityMoveDoneTag> pMoveReset;
         ComponentLookup<NormalActionDoneTag> MoveReset;
        ComponentLookup<PriorityAttackDoneTag> pAtkReset;
        ComponentLookup<ReloadingDoneTag> reloadingDoneReset;
        ComponentLookup<AttackDoneTag> attackReset;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SampleUnitComponentData>();
            //행동 완료 태그가 비활성화된 유닛들을 모으는 쿼리 --> 해당 쿼리가 비어있으면 모든 유닛들의 행동이 완료되었다는 의미
            priorityMoveDoneQuery= new EntityQueryBuilder(Allocator.Temp).WithDisabled<PriorityMoveDoneTag>().Build(ref state);
            normalActionDoneQuery= new EntityQueryBuilder(Allocator.Temp).WithDisabled<NormalActionDoneTag>().Build(ref state);
            priorityAttackDoneQuery= new EntityQueryBuilder(Allocator.Temp).WithDisabled<PriorityAttackDoneTag>().Build(ref state);
            reloadingDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<ReloadingDoneTag>().Build(ref state);
            AttackDoneQuery = new EntityQueryBuilder(Allocator.Temp).WithDisabled<AttackDoneTag>().Build(ref state);
         
           
            NormalBehaviorTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<PriorityMovingTag, AttackTag, MovingTag, LazyTag>().Build(ref state); //턴이 지났는지 확인하는 용도. 여기에 모든 행동 지시 태그를 넣어야 합니다. 그리고 행동 우선 순위가 지날 때마다 확인해주세요.
            unitQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SampleUnitComponentData>().Build(ref state);
            tileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
            sampleUnitLookup = state.GetComponentLookup<SampleUnitComponentData>(true);
            startLookup = state.GetComponentLookup<StartPause>(true);
            pMoveReset = state.GetComponentLookup<PriorityMoveDoneTag>();
            MoveReset = state.GetComponentLookup<NormalActionDoneTag>();
            pAtkReset = state.GetComponentLookup<PriorityAttackDoneTag>();
            reloadingDoneReset = state.GetComponentLookup<ReloadingDoneTag>();
            attackReset = state.GetComponentLookup<AttackDoneTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            sampleUnitLookup.Update(ref state);
            startLookup.Update(ref state);
            pMoveReset.Update(ref state);
            MoveReset.Update(ref state);
            pAtkReset.Update(ref state);
            reloadingDoneReset.Update(ref state);
            attackReset.Update(ref state);
           // seed = new Random((uint)(SystemAPI.Time.DeltaTime*1000));
            //spawnerEntity = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>()).GetSingletonEntity();

            // Debug.Log(spawnerQuery.CalculateEntityCount());
            // NativeArray<Entity> spawner = spawnerQuery.ToEntityArray(Allocator.TempJob);
            // spawnerEntity = spawner[0];

            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();

            NativeArray<Entity> tiles = tileQuery.ToEntityArray(Allocator.TempJob);
            //난수 생성 확인기
            // foreach (var (unit, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>>().WithEntityAccess())
            // {
            //     Debug.Log(unit.ValueRW.dice.NextInt(1, 6));
            // }

            Debug.Log("PMoveQuery : " + priorityMoveDoneQuery.IsEmpty);
            Debug.Log("ReloadingQuery : " + reloadingDoneQuery.IsEmpty);
            Debug.Log("AttackDoneQuery : " + AttackDoneQuery.IsEmpty); //AttackDone Flag가 정상적으로 세워지지 않아 태그 초기화가 이루어지지 않음
            if (priorityMoveDoneQuery.IsEmpty && reloadingDoneQuery.IsEmpty && AttackDoneQuery.IsEmpty) //턴 종료 확인
            {
                Debug.Log("초기화 진행");
                //유닛 몇개 있는지 확인, 유닛이 있다면 사기 체크, 체크 통과하면 초기화 , singleton 엔티티가 각 세력 병력 수 기록, 사기 체크가 필요한 세력이 누구누구인지 job에 전달함.
                //job에서는 각 엔티티를 가져와서 엔티티의 세력 데이터를 사기 체크 필요한 세력 데이터와 비교 후 사기 체크 결정. 통과 시 태그 초기화(행동 완료 태그 비활성화).
                EntityCommandBuffer ecb = new(Allocator.Temp);
                //체력 0인 유닛 파괴
                
                foreach (var (unit, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>>().WithEntityAccess())
                {
                    //사기 체크. 지금은 테스트용으로 비활성화
                    //if (unit.ValueRW.order + unit.ValueRW.dice.NextInt(1, 6) + unit.ValueRW.dice.NextInt(1, 6) < 10) 
                    {
                        SystemAPI.GetComponentRW<MapTileAuthoringComponentData>(tiles[unit.ValueRO.index.x + unit.ValueRO.index.y * mapMaker.number]).ValueRW.soldier = 0;
                        //ecb.DestroyEntity(entity);
                        //Debug.Log("Delete");
                        if (pMoveReset.HasComponent(entity))
                        {
                            pMoveReset.SetComponentEnabled(entity,false);
                        }
                        if (reloadingDoneReset.HasComponent(entity))
                        {
                            reloadingDoneReset.SetComponentEnabled(entity, false);
                        }
                        if (attackReset.HasComponent(entity))
                        {
                            attackReset.SetComponentEnabled(entity, false);
                        }
                        if (MoveReset.HasComponent(entity))
                        {
                            MoveReset.SetComponentEnabled(entity,false);
                        }
                        if (pAtkReset.HasComponent(entity))
                        {
                            pAtkReset.SetComponentEnabled(entity,false);
                        }
                    }
                }
                ecb.Playback(state.EntityManager);
                tiles.Dispose();
                
            }
            
            //모든 유닛의 행동 완료 태그 초기화 job이 완료 됐는지 확인하는 조건문.
            

            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}