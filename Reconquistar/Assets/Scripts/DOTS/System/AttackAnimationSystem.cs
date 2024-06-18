using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.System
{
    [BurstCompile]
    [UpdateBefore(typeof(SpriteUVAnimationSystem))]
    public partial struct AttackAnimationSystem : ISystem
    {
         EntityQuery attackTagQuery;
         double delay;
        private struct SystemData : IComponentData
        {
            public EntityQuery AttackQuery;
        }
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AttackTag>();
            delay = 0;
            var systemData = new SystemData();
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<AttackTag,AttackDoneTag,NormalActionDoneTag>()
                .WithAspect<AnimatorAspect>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState);
            var movableQuery = state.GetEntityQuery(queryBuilder);
            movableQuery.AddChangedVersionFilter(ComponentType.ReadOnly<AttackTag>());
            systemData.AttackQuery = movableQuery;

            _ = state.EntityManager.AddComponentData(state.SystemHandle, systemData);

            queryBuilder.Dispose();
            attackTagQuery = new EntityQueryBuilder(Allocator.Temp).WithAny<AttackTag>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (attackTagQuery.IsEmpty)
            {
                return;
            }
            var systemData = SystemAPI.GetComponent<SystemData>(state.SystemHandle);
            if (!SystemAPI.TryGetSingleton<AnimationSettings>(out var animationSettings))
                return;
            var time = SystemAPI.Time.ElapsedTime;
            var animationSwitchJob = new AttackAnimationJob()
            {
                AnimationSettings = animationSettings,
                Time = SystemAPI.Time.ElapsedTime
            };
            state.Dependency = animationSwitchJob.ScheduleParallelByRef(systemData.AttackQuery, state.Dependency);
            state.Dependency.Complete();
            delay += SystemAPI.Time.DeltaTime;
            if (delay > 1)
            {
                foreach (var (unit, target, attackTag, entity) in SystemAPI.Query<RefRO<SampleUnitComponentData>, RefRW<TargetEntityData>, EnabledRefRW<AttackTag>>().WithAll<AttackTag>().WithDisabled<AttackDoneTag>().WithEntityAccess())//, EnabledRefRW<AttackDoneTag>, EnabledRefRW<AttackTag>>().WithAll<AttackTag>())
                {
                    //Debug.Log("공격");
                    SystemAPI.GetComponentRW<SampleUnitComponentData>(target.ValueRW.targetEntity).ValueRW.hp -= unit.ValueRO.dmg;
                    //한번만 하게 수정 필요. normalactiondonetag를 여기에 넣는 것을 권장
                }
                delay = 0;
                var animationDonJob = new AttackDoneJob()
                {
                    AnimationSettings = animationSettings,
                    Time = SystemAPI.Time.ElapsedTime
                };
                state.Dependency = animationDonJob.ScheduleParallelByRef(systemData.AttackQuery, state.Dependency);
            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }
    }
}