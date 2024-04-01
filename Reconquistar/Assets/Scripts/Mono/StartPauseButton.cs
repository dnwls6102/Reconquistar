using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using _1.Scripts.DOTS.Authoring_baker_; //SampleSpawnData 컴포넌트를 불러오기 위함
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public class StartPauseButton : MonoBehaviour
{
    private Entity _spawnerEntity;

    private EntityManager _entityManager;



    public void StartPauseBtn()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _spawnerEntity = _entityManager.CreateEntityQuery(typeof(SampleSpawnData)).GetSingletonEntity();

        if (_entityManager.IsComponentEnabled<StartPause>(_spawnerEntity))
        {
            _entityManager.SetComponentEnabled<StartPause>(_spawnerEntity, false);
            Debug.Log(_entityManager.IsComponentEnabled<StartPause>(_spawnerEntity));
        }
        else
        {
            _entityManager.SetComponentEnabled<StartPause>(_spawnerEntity, true);
            Debug.Log(_entityManager.IsComponentEnabled<StartPause>(_spawnerEntity));
        }
    }
}
