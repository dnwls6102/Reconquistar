using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Collections;

//using UnityEngine;
//using System.Diagnostics;


namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        public float Time;
        [ReadOnly] public MapMakerComponentData MapMaker;
        //public EntityCommandBuffer.ParallelWriter ECBWriter;
        // excute 쿼리에 moving tag 추가 예정
        public void Execute(ref LocalTransform transform, EnabledRefRW<MovingTag> movingTag, ref SampleUnitComponentData sampleUnitComponentData)
        {
            if (math.all(transform.Position == Int2tofloat3(sampleUnitComponentData.destIndex)))
            {
                // Debug.Log("Cancel Moving Tag of "+sampleUnitComponentData.index +sampleUnitComponentData.destIndex);
                sampleUnitComponentData.index = sampleUnitComponentData.destIndex;
                movingTag.ValueRW = false;
            }
            else
            {
                transform.Position = MoveTowards(transform.Position, Int2tofloat3(sampleUnitComponentData.destIndex), Time * sampleUnitComponentData.movementspeed);
                //Debug.Log("Moving entity" + sampleUnitComponentData.index);
                // moving tag 취소
            }
        }

        //MoveTowards의 Unity.Mathematic버전
        public static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
        {
            float deltaX = target.x - current.x;
            float deltaY = target.y - current.y;

            float sqdist = deltaX * deltaX + deltaY * deltaY;

            if (sqdist == 0 || sqdist <= maxDistanceDelta * maxDistanceDelta)
                return target;
            var dist = math.sqrt(sqdist);

            return new float3(current.x + deltaX / dist * maxDistanceDelta,
                current.y + deltaY / dist * maxDistanceDelta, 0
                );
        }
        //인덱스를 float3 형식으로 바꿔주는 코드
        public static float3 Int2tofloat3(int2 index)
        {
            return new float3(index.x, (float)index.y * 1f, 0);
        }
    }

}