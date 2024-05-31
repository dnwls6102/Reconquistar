using System.Collections.Generic;
using _1.Scripts.DOTS.Components___Tags;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    public partial struct ShootingJob : IJobEntity
    {
        public EntityCommandBuffer ECB; //RuleSystem에서 가져온 엔티티커맨드버퍼
        //[ReadOnly] public ComponentLookup<SampleUnitComponentData> SampleUnitComponents; //RuleSystem에서 가져온 SampleUnitComponent ComponentLookup

        public void Execute(ref LocalTransform transform, EnabledRefRW<AttackTag> attackTag, ref ShootTag cUnit)
        {
            Entity instance = ECB.Instantiate(cUnit.BulletEntity); //월드 내에 총알 생성
            cUnit.bullets -= 1;
            ECB.SetComponent<LocalTransform>(instance, new LocalTransform
            {
                Position = transform.Position,
                Rotation = transform.Rotation,
                Scale = transform.Scale,
            });
            Debug.Log("JobsDone");
            ECB.DestroyEntity(instance);
        }
    }
}

