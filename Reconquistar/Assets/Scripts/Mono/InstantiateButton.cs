using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using _1.Scripts.DOTS.Authoring_baker_; //SampleSpawnData 컴포넌트를 불러오기 위함
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public class InstantiateButton : MonoBehaviour
{
    private Entity _spawnerEntity;

    private EntityManager _entityManager;


    public void InstantiateEntity()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager; //Mono로 EntityManager를 가져옴
        //Spawner 엔티티를 가져옴.
        _spawnerEntity = _entityManager.CreateEntityQuery(typeof(SampleSpawnData)).GetSingletonEntity();
        var _sampleUnitEntity = _entityManager.GetComponentData<SampleSpawnData>(_spawnerEntity).SampleEntityPrefab;
        Debug.Log($"{_spawnerEntity.ToString()}");
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.Instantiate(_sampleUnitEntity);

        ecb.SetComponent(_sampleUnitEntity, new SampleUnitComponentData
        {
            index = new int2(0, 0),
            hp = 3,
            movementspeed = 0.2f,
            dmg = 1,
            team = 0
        });
        ecb.SetComponent(_sampleUnitEntity, new LocalTransform()
        {
            Position = new float3(0, 0, 0),
            Scale = 10
        });
        ecb.Playback(_entityManager);
        Debug.Log("엔티티 생성완료");
    }

}
