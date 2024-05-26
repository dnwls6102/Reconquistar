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
        private struct SystemData : IComponentData
        {
            public EntityQuery AttackQuery;
        }
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var systemData = new SystemData();
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<AttackTag>()
                .WithAspect<AnimatorAspect>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState);
            var movableQuery = state.GetEntityQuery(queryBuilder);
            movableQuery.AddChangedVersionFilter(ComponentType.ReadOnly<MovingTag>());
            systemData.AttackQuery = movableQuery;

            _ = state.EntityManager.AddComponentData(state.SystemHandle, systemData);

            queryBuilder.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var systemData = SystemAPI.GetComponent<SystemData>(state.SystemHandle);
            if (!SystemAPI.TryGetSingleton<AnimationSettings>(out var animationSettings))
                return;
            var time = SystemAPI.Time.ElapsedTime;

            var animationSwitchJob = new AnimationJob
            {
                AnimationSettings = animationSettings,
                Time = time
            };
            state.Dependency = animationSwitchJob.ScheduleParallelByRef(systemData.AttackQuery, state.Dependency);
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }
    }
}