using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Entities;
using _1.Scripts.DOTS.Authoring_baker_; //SampleSpawnData 컴포넌트를 불러오기 위함

public class ToggleManager : MonoBehaviour
{
    public int toggleValue = 0;
    public List<Toggle> toggleButtons = new List<Toggle>();
    private Entity _spawnerEntity; // World의 Spawner를 담기 위한 변수

    private EntityManager _entityManager;
    private void Start()
    {
        foreach (Toggle toggle in toggleButtons)
        {
            toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle); });
        }
    }

    void ToggleValueChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            toggleValue = toggleButtons.IndexOf(changedToggle) + 1;

            foreach (Toggle toggle in toggleButtons)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
        }
        else
        {
            bool anyTogglesOn = false;
            foreach (Toggle toggle in toggleButtons)
            {
                if (toggle.isOn)
                {
                    anyTogglesOn = true;
                    break;
                }
            }

            if (!anyTogglesOn)
            {
                toggleValue = 0;
            }
        }
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager; //Mono로 EntityManager를 가져옴
        //Spawner 엔티티를 가져옴.
        _spawnerEntity = _entityManager.CreateEntityQuery(typeof(WhattoSpawn)).GetSingletonEntity();
        //SampleSpawner의 ToggleValue값을 ToggleManager의 업데이트된 toggleValue로 설정
        _entityManager.SetComponentData<WhattoSpawn>(_spawnerEntity, new WhattoSpawn { ToggleValue = toggleValue });
        //디버그용 
        var test = _entityManager.GetComponentData<WhattoSpawn>(_spawnerEntity).ToggleValue;
        Debug.Log($"{test.ToString()}");

    }
}
