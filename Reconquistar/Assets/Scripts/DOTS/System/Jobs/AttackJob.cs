using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Collections;
using UnityEngine;


namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    //AttackTag가 활성화된 인원에 한해서만 공격 작업을 실행해야 하기에 Enabled 무시 태그는 붙이지 않음
    public partial struct AttackJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<SampleUnitComponentData> SampleUnitComponents;
        public EntityCommandBuffer.ParallelWriter ecb;
        public void Execute(RefRO<SampleUnitComponentData> unit, RefRW<TargetEntityData> target, EnabledRefRW<AttackTag> attackTag)
        {
            Debug.Log("공격Job");
            //Job에서 entity 접근을 통해 컴포넌트 데이터 조작을 하는 방법을 찾아야 함
            //GetComponentLookup으로 하기
            //SampleUnitComponents[target.ValueRW.targetEntity].hp = SampleUnitComponents[target.ValueRW.targetEntity].hp - unit.ValueRO.dmg;
            //ecb ParallelWriter로 하기
            //WithEntityAccess로 entity에 대한 접근을 한 후 attackDoneTag를 활성화하는 방법(Execute로 entityAccess가 가능한지부터 확인)
            //아니면 AttackDoneTag가 disabled된 유닛들을 긁어 모으는법도 방법
            //필요한 인수: sortkey, entity, Component
            ecb.SetComponent<SampleUnitComponentData>(1, target.ValueRW.targetEntity, new SampleUnitComponentData()
            {
                order = SampleUnitComponents[target.ValueRO.targetEntity].order,
                index = SampleUnitComponents[target.ValueRO.targetEntity].index,
                destIndex = SampleUnitComponents[target.ValueRO.targetEntity].destIndex,
                movementspeed = SampleUnitComponents[target.ValueRO.targetEntity].movementspeed,
                hp = SampleUnitComponents[target.ValueRW.targetEntity].hp - unit.ValueRO.dmg,
                dmg = SampleUnitComponents[target.ValueRO.targetEntity].dmg,
                team = SampleUnitComponents[target.ValueRO.targetEntity].team,
                dice = SampleUnitComponents[target.ValueRO.targetEntity].dice
            });
            attackTag.ValueRW = false; //행동 Flag를 내리기
            //doneTag.ValueRW = true; //행동 완료 Flag를 올리기
        }
    }
}
