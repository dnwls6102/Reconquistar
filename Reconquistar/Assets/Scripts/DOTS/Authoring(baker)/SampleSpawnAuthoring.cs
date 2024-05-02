using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.Authoring_baker_
{
    public class SampleSpawnAuthoring : MonoBehaviour
    {
        public GameObject sampleUnit;
        public GameObject samplePMoveUnit;
        public int number;
        public int pMoveNum;
        public int ToggleValue = 0;
        public int startFlag = 0;
        public class SampleSpawnAuthoringBaker : Baker<SampleSpawnAuthoring>
        {
            public override void Bake(SampleSpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SampleSpawnData()
                {
                    SampleEntityPrefab = GetEntity(authoring.sampleUnit, TransformUsageFlags.Dynamic),
                    number = authoring.number,
                });
                AddComponent(entity, new SamplePMoveSpawnData()
                {
                    SamplePMoveEntityPrefab = GetEntity(authoring.samplePMoveUnit, TransformUsageFlags.Dynamic),
                    PMoveNumber = authoring.pMoveNum,
                });
                AddComponent(entity, new WhattoSpawn()
                {
                    ToggleValue = authoring.ToggleValue
                });
                AddComponent(entity, new StartPause()
                {
                    startFlag = authoring.startFlag
                });
            }
        }
    }

    public struct SampleSpawnData : IComponentData
    {
        public Entity SampleEntityPrefab;
        //타일맵 가로 세로 갯수
        public int number;
    }

    public struct SamplePMoveSpawnData : IComponentData
    {
        public Entity SamplePMoveEntityPrefab;
        public int PMoveNumber;
    }

    public struct WhattoSpawn : IComponentData //Mono로부터 ToggleValue를 받아내기 위한 컴포넌트
    {
        public int ToggleValue; // 어떤 유닛을 스폰할지 판단하는 변수
    }

    public struct StartPause : IComponentData, IEnableableComponent
    {
        public int startFlag;
    }
}