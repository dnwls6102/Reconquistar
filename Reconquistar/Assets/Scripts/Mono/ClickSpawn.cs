using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using _1.Scripts.DOTS.Authoring_baker_; //SampleSpawnData 컴포넌트를 불러오기 위함
using _1.Scripts.DOTS.Components___Tags;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public class ClickSpawn : MonoBehaviour
{
    public float distance = 0.1f;

    private Entity _targetEntity;
    private Camera MainCamera;
    private EntityManager _entityManager;
    private Entity _spawnerEntity;
    private Entity _sampleUnitEntity;
    private int _spawnNum;
    // private int _toggleValue;

    private void Awake()
    {
        MainCamera = Camera.main;
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePosition;
            mousePosition = MainCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 10f));

            _targetEntity = SelectEntity(mousePosition);
            //Debug.Log($"{_targetEntity.ToString()}");

            if (_targetEntity != Entity.Null)
            {
                Spawn();
            }
        }
    }

    private Entity SelectEntity(Vector3 mousePosition)
    {
        //if (MainCamera.Instance == null) return Entity.Null;
        //var mainCam = MainCamera.Instance.Camera;
        //Debug.Log(mousePosition);
        // var ray = MainCamera.ScreenToWorldPoint(mousePosition);//var ray = MainCamera.ScreenPointToRay(mousePosition);
        //Debug.Log($"{ray.ToString()}");
        Entity closestTile = Entity.Null;

        // RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, 50f);
        // Debug.DrawRay(mousePosition, transform.forward * 10, Color.red, 0.3f);
        // if (!hit)
        // {
        //     Debug.Log("분기 발동");
        //     return Entity.Null;
        // }
        // else
        //     Debug.Log("정상 클릭");


        var query = _entityManager.CreateEntityQuery(typeof(MapTileAuthoringComponentData));

        var minSqrDist = distance * distance;
        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {

            var entityPos = _entityManager.GetComponentData<MapTileAuthoringComponentData>(entity).index;

            var dist = (mousePosition - new Vector3(entityPos.x, entityPos.y, mousePosition.z)).sqrMagnitude;
            if (dist < minSqrDist)
            {
                //Debug.Log(entity);
                closestTile = entity;
                break;
            }
        }

        return closestTile;
    }

    private void Spawn()
    {
        MapTileAuthoringComponentData clickedTile = _entityManager.GetComponentData<MapTileAuthoringComponentData>(_targetEntity);
        Debug.Log($"{_targetEntity.ToString()}");
        if (clickedTile.soldier != 0) return;
        MapMakerComponentData mapMaker = _entityManager.CreateEntityQuery(typeof(MapMakerComponentData)).GetSingleton<MapMakerComponentData>();

        _spawnerEntity = _entityManager.CreateEntityQuery(typeof(SampleSpawnData)).GetSingletonEntity();
        _sampleUnitEntity = _entityManager.GetComponentData<SampleSpawnData>(_spawnerEntity).SampleEntityPrefab;
        _spawnNum = _entityManager.GetComponentData<SampleSpawnData>(_spawnerEntity).number;
        //_toggleValue = _entityManager.GetComponentData<WhattoSpawn>(_spawnerEntity).ToggleValue;

        /*
        if (_toggleValue == 0)
        {
            return;
        }
        */

        var SampleUnits = new NativeArray<Entity>(_spawnNum, Allocator.Temp);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.Instantiate(_sampleUnitEntity, SampleUnits);
        var tileQuery = _entityManager.CreateEntityQuery(typeof(MapTileAuthoringComponentData));
        var tiles = tileQuery.ToEntityArray(Allocator.Temp);

        int x = clickedTile.index.x;
        int y = 0;
        int newteam = 0;
        foreach (var SampleUnit in SampleUnits)
        {
            // MapTileAuthoringComponentData currentTile = _entityManager.GetComponentData<MapTileAuthoringComponentData>(tiles[y + x * mapMaker.number]);
            // if (currentTile.soldier == 0)
            // {
            ecb.SetComponent(SampleUnit, new SampleUnitComponentData
            {
                index = new int2(x, y),
                hp = 3,
                movementspeed = 1f,
                dmg = 1,
                team = newteam
            });
            ecb.SetComponentEnabled<MovingTag>(SampleUnit, false);
            ecb.SetComponentEnabled<AttackTag>(SampleUnit, false);
            ecb.SetComponentEnabled<LazyTag>(SampleUnit, false);

            ecb.SetComponent(SampleUnit, new LocalTransform()
            {
                Position = new float3(x, y * mapMaker.width, 0),
                Scale = 5
            });

            //     currentTile.soldier = 1;
            //     _entityManager.SetComponentData(tiles[x + y * mapMaker.number], currentTile);
            // }
            // else ecb.DestroyEntity(SampleUnit);

            if (y != mapMaker.number) //원본 : if (y < mapMaker.number - 1)
            {
                y++;
            }
            else
            {
                break;
            }
            if (x >= 50)
            {
                newteam = 1;
            }
        }
        ecb.Playback(_entityManager);
    }
}
