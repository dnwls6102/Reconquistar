using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Mathematics;
using System.Collections.Generic;
using _1.Scripts.DOTS.Components___Tags;
using NSprites;
using UnityEngine;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    public partial struct FindPriorityMoveJob : IJobEntity //자유 이동 태그를 갖고 있는 유닛들의 목적지만 찾는 Job
    {
        [ReadOnly] public NativeArray<Entity> SampleUnits; //RuleSystem에서 가져온 SampleUnitComponents를 갖고 있는 엔티티들 배열
        [ReadOnly] public ComponentLookup<SampleUnitComponentData> SampleUnitComponents; //RuleSystem에서 가져온 SampleUnitComponent ComponentLookup
        [ReadOnly] public MapMakerComponentData MapMaker; //RuleSystem에서 가져온 MapMaker
        //public ComponentLookup<TargetEntityData> TargetEntityComponents; //RuleSystem에서 가져온 TargetEntityLookup
        //public ComponentLookup<Flip> FlipComponents;

        // 전체 유닛 및 유닛 엔티티도 가져와야...
        public void Execute(in SampleUnitComponentData currentUnit, PriorityMovingTag PMoveTag, ref TargetEntityData target, ref Flip flipx)
        {
            //비교 기준 : 자유 이동 유닛들. 비교 후 target 선정을 Execute로 가져온 currentUnit으로 해야함

            bool found = false;
            int2 targetIndex = new(0, 0);
            float dist = math.INFINITY;

            //가장 가까운 적 유닛 찾기
            for (int i = 0; i < SampleUnits.Length; i++)
            {
                if (SampleUnitComponents[SampleUnits[i]].team != currentUnit.team) //전체 유닛들 중 i번째 인덱스를 가진 유닛의 팀과 현재 자유이동 유닛의 팀이 다를경우
                {
                    float newDist = math.distancesq(currentUnit.index, SampleUnitComponents[SampleUnits[i]].index); //해당 유닛과 현재 자유이동 유닛의 거리 측정
                    if (newDist < dist) // dist가 newDist보다 클 경우 (주: 현재는 디버깅을 위해 반드시 작동하게 해야해서 math.INFINITY로 설정한 것)
                    {
                        dist = newDist; // dist = newDist
                        targetIndex = SampleUnitComponents[SampleUnits[i]].index; // targetIndex를 i번째 유닛의 index로 변경
                        // currentUnit의 targetEntity를 SampleUnits[i]으로 변경
                        target.targetEntity = SampleUnits[i];
                        found = true; //flag 설정
                    }

                }
            }
            if (found) //가까운 적, 그러니까 목표 대상을 찾았다면
            {
                if (targetIndex.x - currentUnit.index.x > 0) //targetIndex가 현재 유닛보다 오른쪽에 있는 경우
                {
                    flipx.Value = new int2(-1, 0); // flipx 발동 
                }
                else if (targetIndex.x - currentUnit.index.x < 0) //targetIndex가 현재 유닛보다 왼쪽에 있는 경우
                {
                    flipx.Value = new int2(1, 0); // flipx 취소
                }
            }

            // else
            //     return;
        }
    }
}