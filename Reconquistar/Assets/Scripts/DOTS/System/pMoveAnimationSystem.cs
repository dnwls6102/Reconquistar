﻿using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
namespace _1.Scripts.DOTS.System
{
    [BurstCompile]
    [UpdateBefore(typeof(SpriteUVAnimationSystem))]
    public partial struct pMoveAnimationSystem : ISystem
    {
        private struct SystemData : IComponentData
        {
            public EntityQuery PMovableQuery;
        }
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var systemData = new SystemData();
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PriorityMovingTag>()
                .WithAspect<AnimatorAspect>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState);
            var movableQuery = state.GetEntityQuery(queryBuilder);
            movableQuery.AddChangedVersionFilter(ComponentType.ReadOnly<MovingTag>());
            systemData.PMovableQuery = movableQuery;

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

            var animationSwitchJob = new PmovingAnimationjob()
            {
                AnimationSettings = animationSettings,
                Time = time
            };
            state.Dependency = animationSwitchJob.ScheduleParallelByRef(systemData.PMovableQuery, state.Dependency);
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}