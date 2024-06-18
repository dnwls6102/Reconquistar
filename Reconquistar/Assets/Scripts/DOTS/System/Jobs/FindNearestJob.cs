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
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct FindNearestJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> SampleUnits; //RuleSystem에서 가져온 SampleUnits
        [ReadOnly] public ComponentLookup<SampleUnitComponentData> SampleUnitComponents; //RuleSystem에서 가져온 ComponentLookup
        [ReadOnly] public MapMakerComponentData MapMaker; //RuleSystem에서 가져온 MapMaker

        // AttackTag가 활성화 되어있는(주: WithOptions로 Enabled여부를 파악하지 않고 있음) 유닛, Target엔티티 , Flip
        // 목표를 찾지 못했을 경우 DoneTag의 Flag를 세우기
        public void Execute(in SampleUnitComponentData currentUnit, EnabledRefRW<AttackTag> attackTag, EnabledRefRW<AttackDoneTag> doneTag, ref TargetEntityData target, ref Flip flipx)
        {
            bool found = false;
            int2 targetIndex = new(0, 0);
            float dist = math.INFINITY;

            //가장 가까운 적 유닛 찾기
            for (int i = 0; i < SampleUnits.Length; i++)
            {

                if (SampleUnitComponents[SampleUnits[i]].team != currentUnit.team) //전체 유닛들 중 i번째 인덱스를 가진 유닛의 팀과 현재 유닛의 팀이 다를경우
                {
                    float newDist = math.distancesq(currentUnit.index, SampleUnitComponents[SampleUnits[i]].index); //해당 유닛과 현재 유닛의 거리 측정
                    if (newDist < dist) // dist가 newDist보다 클 경우 (주: 현재는 디버깅을 위해 반드시 작동하게 해야해서 math.INFINITY로 설정한 것)
                    {
                        dist = newDist; // dist = newDist
                        targetIndex = SampleUnitComponents[SampleUnits[i]].index; // targetIndex를 해당 유닛의 index로 변경
                        target.targetEntity = SampleUnits[i]; // target의 TargetEntity정보를 해당 유닛으로 교체
                        found = true; //flag 설정
                    }
                }
            }
            if (found) //가까운 적, 그러니까 목표 대상을 찾았다면
            {
                if (targetIndex.x - currentUnit.index.x > 0) //targetIndex가 현재 유닛보다 오른쪽에 있는 경우
                {
                    flipx.Value = new int2(1, 0); // flipx 발동
                }
                else if (targetIndex.x - currentUnit.index.x < 0) //targetIndex가 현재 유닛보다 왼쪽에 있는 경우
                {
                    flipx.Value = new int2(-1, 0); // flipx 취소
                }
                //찾은 타겟이 범위 안에 있을 시 Attack Tag 활성화
                if (math.abs(currentUnit.index.x - targetIndex.x) + math.abs(currentUnit.index.y - targetIndex.y) <= currentUnit.range)
                {
                  //  Debug.Log("done");
                  if (attackTag.ValueRW == false)
                  {
                      attackTag.ValueRW = true;
                  }
                }
                else
                {
                    doneTag.ValueRW = true;
                }
            }
            
        }
    }
}