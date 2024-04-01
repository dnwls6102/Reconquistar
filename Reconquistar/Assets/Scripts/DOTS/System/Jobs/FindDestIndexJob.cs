/*
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Mathematics;
using System.Collections.Generic;
using _1.Scripts.DOTS.Components___Tags;



namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct FindDestIndexJob : IJobEntity
    {
        
        [ReadOnly] public NativeArray<SampleUnitComponentData> SampleUnits;
        [ReadOnly] public MapMakerComponentData MapMaker;

        public void Execute(ref SampleUnitComponentData sampleUnit, EnabledRefRW<MovingTag> movingTag, EnabledRefRW<LazyTag> lazyTag)
        {
            // 유닛들이 담긴 배열을 복사하고 복사한 배열 속 유닛들을 현재 유닛에서 가까운 순으로 정렬
            NativeArray<SampleUnitComponentData> sampleUnitsInOrder = new(SampleUnits.Length, Allocator.Temp);
            SampleUnits.CopyTo(sampleUnitsInOrder);
            sampleUnitsInOrder.Sort(new SampleUnitIdexComp{ OriginIndex = sampleUnit.index });
            
            sampleUnit.destIndex = new int2(sampleUnit.index);
            for(int i=0; i<sampleUnitsInOrder.Length; i++){
                if(sampleUnitsInOrder[i].team != sampleUnit.team){
                    int dx = (int)math.sign(sampleUnitsInOrder[i].index.x - sampleUnit.index.x);
                    int dy =  (int)math.sign(sampleUnitsInOrder[i].index.y - sampleUnit.index.y);
                    sampleUnit.destIndex += new int2(dx, dy);
                    break;
                }
            }
            
            //목적지를 찾았을 경우 moving tag 활성, 움직일 곳이 없으면 Lazy
            if(!sampleUnit.index.Equals(sampleUnit.destIndex)){
                movingTag.ValueRW = true;
                if(lazyTag.ValueRO) lazyTag.ValueRW = false;
            }
            else
                lazyTag.ValueRW = !lazyTag.ValueRW;
        }
    }
}
*/