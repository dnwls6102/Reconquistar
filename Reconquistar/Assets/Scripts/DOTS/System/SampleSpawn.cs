﻿using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _1.Scripts.DOTS.System
{
    public partial struct SampleSpawn : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SamplePMoveSpawnData>();
            state.RequireForUpdate<MapTileAuthoringComponentData>();
            state.RequireForUpdate<MapMakerComponentData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var SampleSpawner = SystemAPI.GetSingleton<SampleSpawnData>();
            var SamplePMoveSpawner = SystemAPI.GetSingleton<SamplePMoveSpawnData>();
            var SampleShootingUnitSpawner = SystemAPI.GetSingleton<SampleShootingUnitSpawnData>();
            var Flags = SystemAPI.GetSingleton<DebugFlags>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            int x = 0;
            int y = 0;
            int newteam = 0;
            int n = 0;
            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
            var tileQuery = SystemAPI.QueryBuilder().WithAll<MapTileAuthoringComponentData>().Build();
            NativeArray<MapTileAuthoringComponentData> tiles = tileQuery.ToComponentDataArray<MapTileAuthoringComponentData>(Allocator.Temp);

            if (Flags.sampleUnitDebugFlag == true)
            {
                //x=2 축을 기준으로, 양 옆으로 랜덤한 갯수의 랜덤한 위치의 sampleUnit들 생성

            }
            else if (Flags.samplePMoveUnitDebugFlag == true)
            {
                //x=2 축을 기준으로, (0,0)과 (4,4)에 우선이동 유닛 생성, (0,4), (1,2), (3,2), (4,0)에 sampleUnit생성

            }
            else if (Flags.sampleShootingUnitDebugFlag == true)
            {
                //x=2 축을 기준으로, (1,2)에 원거리 공격 유닛 생성, (3,1),(3,2),(3,3)에 sampleUnit생성
                x = 3;
                y = 1;
                newteam = 1;
                SampleSpawner.number = 3;
                SampleShootingUnitSpawner.number = 2;
                var SampleUnits = new NativeArray<Entity>(SampleSpawner.number, Allocator.Temp);
                var SampleShootingUnits = new NativeArray<Entity>(SampleShootingUnitSpawner.number, Allocator.Temp);
                ecb.Instantiate(SampleSpawner.SampleEntityPrefab, SampleUnits);
                ecb.Instantiate(SampleShootingUnitSpawner.SampleShootingUnitEntityPrefab, SampleShootingUnits);

                var query = SystemAPI.QueryBuilder().WithAll<SampleUnitComponentData>().WithAll<LocalTransform>().WithAspect<SampleUnitAspect>().Build();
                var shootingQuery = SystemAPI.QueryBuilder().WithAll<SampleUnitComponentData>().WithAll<LocalTransform>().WithAll<ShootTag>().Build();
                var queryMask = query.GetEntityQueryMask();
                var shootingQueryMask = shootingQuery.GetEntityQueryMask();

                //SampleUnit 생성
                foreach (var SampleUnit in SampleUnits)
                {
                    ecb.SetComponentForLinkedEntityGroup(SampleUnit, queryMask, new SampleUnitComponentData
                    {
                        index = new int2(x, y),
                        hp = 3,
                        movementspeed = 1f,
                        dmg = 0,
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

                    //mapMaker.number가 초기 세팅값은 100으로 잡혀있어서 오류가 발생하는 모양
                    //이 작업을 하기 전에 mapMaker.number를 5로 바꿔야함
                    //그냥 5로 하는 방법도 있음
                    MapTileAuthoringComponentData currentTile = tiles[x + 5 * y];
                    currentTile.soldier = 1;
                    tiles[x + 5 * y] = currentTile;

                    y++;

                    //SampleShootingUnit 생성


                }
                x = 1;
                y = 1;
                newteam = 0;
                //SampleShootingUnit 생성
                foreach (var SampleSUnit in SampleShootingUnits)
                {
                    ecb.SetComponentForLinkedEntityGroup(SampleSUnit, shootingQueryMask, new SampleUnitComponentData
                    {
                        index = new int2(x, y),
                        hp = 1,
                        movementspeed = 1f,
                        dmg = 1,
                        team = newteam
                    });
                    ecb.SetComponentEnabled<MovingTag>(SampleSUnit, false);
                    ecb.SetComponentEnabled<AttackTag>(SampleSUnit, false);
                    ecb.SetComponentEnabled<LazyTag>(SampleSUnit, false);

                    ecb.SetComponent(SampleSUnit, new LocalTransform()
                    {
                        Position = new float3(x, y * mapMaker.width, 0),
                        Scale = 5
                    });

                    MapTileAuthoringComponentData currentTile = tiles[x + 5 * y];
                    currentTile.soldier = 1;
                    tiles[x + 5 * y] = currentTile;

                    y++;
                }
                ecb.Playback(state.EntityManager);
                tileQuery.CopyFromComponentDataArray(tiles);
            }
            else
            {
                var SampleUnits = new NativeArray<Entity>(SampleSpawner.number, Allocator.Temp);
                var SamplePMoveUnits = new NativeArray<Entity>(SamplePMoveSpawner.number, Allocator.Temp);
                ecb.Instantiate(SampleSpawner.SampleEntityPrefab, SampleUnits);
                ecb.Instantiate(SamplePMoveSpawner.SamplePMoveEntityPrefab, SamplePMoveUnits);

                var query = SystemAPI.QueryBuilder().WithAll<SampleUnitComponentData>().WithAll<LocalTransform>().WithAspect<SampleUnitAspect>().Build();
                var PMovequery = SystemAPI.QueryBuilder().WithAll<SampleUnitComponentData>().WithAll<LocalTransform>().WithAll<PriorityMovingTag>().Build();
                var queryMask = query.GetEntityQueryMask();
                var PMovequeryMask = PMovequery.GetEntityQueryMask();

                //SampleUnit 생성
                foreach (var SampleUnit in SampleUnits)
                {
                    ecb.SetComponentForLinkedEntityGroup(SampleUnit, queryMask, new SampleUnitComponentData
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

                    //새로 생성한 유닛 타일 점거
                    MapTileAuthoringComponentData currentTile = tiles[x + mapMaker.number * y];
                    currentTile.soldier = 1;
                    tiles[x + mapMaker.number * y] = currentTile;

                    if (y < mapMaker.number - 1)
                    {
                        y++;
                    }
                    else
                    {
                        y = 0;
                        if (x < 6)
                        {
                            x++;
                        }
                        else if (x == 6)
                        {
                            x = 99;
                        }
                        else if (x > 6)
                        {
                            x--;
                        }
                    }
                    if (x >= 50)
                    {
                        newteam = 1;
                    }

                }

                x = 7;
                y = 0;
                newteam = 0;

                //자유 이동 유닛 생성 (주: 현재 정상적인 작동 여부를 확인하기 위해 7번 열과 92번 열에 자유 이동 유닛을 생성하는 코드로 작성함)
                foreach (var PMoveUnit in SamplePMoveUnits)
                {
                    ecb.SetComponentForLinkedEntityGroup(PMoveUnit, PMovequeryMask, new SampleUnitComponentData
                    {
                        index = new int2(x, y),
                        hp = 3,
                        movementspeed = 2f,
                        dmg = 0,
                        team = newteam
                    });
                    ecb.SetComponentEnabled<MovingTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<AttackTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<LazyTag>(PMoveUnit, false);

                    ecb.SetComponent(PMoveUnit, new LocalTransform()
                    {
                        Position = new float3(x, y * mapMaker.width, 0),
                        Scale = 5
                    });
                    //새로 생성한 유닛 타일 점거
                    MapTileAuthoringComponentData currentTile = tiles[x + mapMaker.number * y];
                    currentTile.soldier = 1;
                    tiles[x + mapMaker.number * y] = currentTile;

                    if (y < mapMaker.number - 1)
                    {
                        y++;
                    }
                    else
                    {
                        y = 0;
                        x = 92;
                    }
                    if (x >= 50)
                    {
                        newteam = 1;
                    }

                }
                ecb.Playback(state.EntityManager);
                tileQuery.CopyFromComponentDataArray(tiles);
            }


        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}