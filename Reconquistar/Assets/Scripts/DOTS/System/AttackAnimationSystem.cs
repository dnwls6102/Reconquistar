using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
                .WithAll<AttackTag>()
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
                Time = time
            };
            state.Dependency = animationSwitchJob.ScheduleParallelByRef(systemData.AttackQuery, state.Dependency);
            delay += SystemAPI.Time.DeltaTime;
            if (delay > 1)
            {
                delay = 0;
                var animationDonJob = new AttackDoneJob()
                {
                    AnimationSettings = animationSettings,
                    Time = time
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