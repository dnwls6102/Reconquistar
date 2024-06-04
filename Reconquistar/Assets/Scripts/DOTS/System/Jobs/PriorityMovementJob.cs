using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Collections;
using UnityEngine;

//using UnityEngine;
//using System.Diagnostics;


namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct PriorityMovementJob : IJobEntity
    {
        public float Time;
        [ReadOnly] public MapMakerComponentData MapMaker;
        //public EntityCommandBuffer.ParallelWriter ECBWriter;
        // excute 쿼리에 moving tag 추가 예정
        public void Execute(ref LocalTransform transform, EnabledRefRW<PriorityMovingTag> movingTag, EnabledRefRW<PriorityMoveDoneTag> DoneTag,ref SampleUnitComponentData sampleUnitComponentData)
        {
            //Debug.Log("PMove");
            // MovingTag를 달고 있는 Unit의 transform이 Unit의 목표지점(destIndex)와 같을 경우?
            if (math.all(transform.Position == Int2tofloat3(sampleUnitComponentData.destIndex)))
            {
                //Debug.Log("PMoveCancel:"+transform.Position);
                // Debug.Log("Cancel Moving Tag of "+sampleUnitComponentData.index +sampleUnitComponentData.destIndex);
                sampleUnitComponentData.index = sampleUnitComponentData.destIndex;
                movingTag.ValueRW = false; //Unit의 index 정보를 destIndex로 바꾸고 movingTag 없애기
                DoneTag.ValueRW = true;
            }
            else // 아직 일치하지 않을 경우
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
        public float3 Int2tofloat3(int2 index) //기존 : static
        {
            return new float3(index.x, (float)index.y * MapMaker.width, 0);
        }
    }

}