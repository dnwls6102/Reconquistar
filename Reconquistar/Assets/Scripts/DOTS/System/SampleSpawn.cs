using System;
using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using DOTS.Authoring_baker_;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _1.Scripts.DOTS.System
{
    public partial struct SampleSpawn : ISystem
    {
        //각 카드의 기초 데이터. 마지막 number에는 곱하기 10을 한 것이 실제 인원수. 
        public static readonly NativeHashMap<int, unit> SpawnData = new NativeHashMap<int, unit>(50, Allocator.Temp) 
        {
            // 카스티야
            {1, new unit(10,2,1,3,2,1,14,1,6,false,false,false, 9)},
            {2,new unit(11,3,1,8,4,0,1,4,3,false,false,false, 8)},
            {3,new unit(11,5,4,4,3,1,2,1,8,false,false,true, 7)},
            {4,new unit(19,6,6,13,6,2,2,1,8,false,false,false, 6)},
            {5,new unit(29,6,4,15,7,4,2,1,8,true,false,true,5)},
            {6, new unit(19,6,0,9,5,3,2,2,8,false,false,false,4)},
            {7,new unit(19,6,1,9,5,3,1,2,10,false,false,false,3)},
            {8,new unit(42,7,1,19,10,8,1,1,20,true,false,false,1)},
            {9,new unit(48,6,3,23,10,8,1,4,3,true,false,false,1)},
            {10,new unit(34,10,6,16,9,7,1,3,6, false,true,false,2)},
            //아라곤
            {11, new unit(11,5,2,3,3,1,1,1,6,false,true,true,9)},
            {12,new unit(10,2,1,3,2,1,14,1,6,false,false,false,8)},
            {13, new unit(11,3,1,8,4,0,1,4,3,false,false,false,7)},
            {14, new unit(29,5,2,12,6,4,1,3,4,true,false,true,6)},
            {15, new unit(19,6,6,13,6,2,2,1,8,false,false,false,5)},
            {16, new unit(19,8,5,12,7,2,1,1,12,false,true,false,4)},
            {17, new unit(32,5,2,14,8,7,2,5,3,false,false,false,3)},
            {18, new unit(20,3,0,15,7,2,13,1,12,false,false,false,4)},
            {19, new unit(31,8,4,11,7,7,1,2,10,false,false,false,2)},
            {20, new unit(48,10,4,23,10,8,1,1,10,true,false,false,1)},
            //포르투갈
            {21, new unit(12,3,5,6,4,1,6,3,4,false,false,false,9)},
            {22, new unit(13,6,5,9,5,1,2,1,8,false,false,true,8)},
            {23, new unit(13,6,1,5,4,2,1,1,6,false,true,true,7)},
            {24, new unit(19,8,6,10,6,2,1,2,5,false,true,false,6)},
            {25, new unit(19,5,1,7,4,3,16,1,8,false,false,false,5)},
            {26, new unit(29,4,0,14,7,4,1,4,3,true,false,false,4)},
            {27, new unit(19,3,1,9,5,3,7,3,6,false,false,false,3)},
            {28, new unit(46,7,1,21,10,8,1,1,20,true,false,false,1)},
            {29, new unit(42,9,5,19,11,8,1,1,12,true,true,false,1.5)},
            {30, new unit(34,6,3,16,8,7,2,5,3,false,false,false,2)},
            //그라나다
            {31, new unit(10,3,1,3,1,1,12,1,12,false,false,false,9)},
            {32, new unit(11,7,4,7,3,0,1,1,10,false,false,false,8)},
            {33, new unit(11,5,4,6,2,1,2,1,8,false,false,false,7)},
            {34, new unit(29,5,-2,9,5,5,12,1,12,true,false,false,6)},
            {35, new unit(29,4,-1,11,6,5,1,3,4,true,false,false,5)},
            {36, new unit(19,6,0,9,5,3,2,2,8,false,false,false,4)},
            {37, new unit(20,5,5,15,8,2,1,1,12,false,true,false,3)},
            {38, new unit(42,5,2,19,10,8,1,4,3,true,false,false,1.5)},
            {39, new unit(30,6,6,7,5,7,6,2,3,false,false,false,1)},
            {40, new unit(32,8,6,14,8,7,1,3,8,false,true,false,2.5)},
            
            //나바라, 최전방에 1카드 씩
            {41, new unit(27,4,-4,17,7,4,1,1,20,true,false,false,1)},
            {42, new unit(16,6,5,11,4,2,2,1,8,false,false,false,3)},
            //아프리카, 최전방에 1카드 씩
            {43, new unit(13,2,-1,6,3,3,7,3,6,false,false,false,1)},
            {44, new unit(13,5,2,7,3,3,2,1,8,false,false,false,3)},
            //마요르카, 최후방에 1카드 씩
            {45, new unit(12,3,0,3,2,3,18,2,3,false,false,false,7)},
            //갈리시아, 최전방에 1카드 씩
            {46, new unit(17,6,0,15,5,3,1,1,10,false,false,false,4)},
            
            {47, new unit()},
            {48, new unit()},
            
            {49, new unit()},
            {50, new unit()},
            
        };
        
        public NativeHashMap<int, int> test;
        public NativeHashMap<int, NativeList<int>> test2;
        
        public NativeList<DeckData> DeckListData;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
           // Debug.Log(string.Format("{0}",SpawnData[1].HasPatk));
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<SamplePMoveSpawnData>();
            state.RequireForUpdate<MapTileAuthoringComponentData>();
            state.RequireForUpdate<MapMakerComponentData>();
            state.RequireForUpdate<DeckLoadingDoneTag>();
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
            //int n = 0;
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
                    ecb.SetComponentEnabled<NormalActionDoneTag>(SampleUnit, false);

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
                    ecb.SetComponentEnabled<NormalActionDoneTag>(SampleSUnit, false);

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
                    ecb.SetComponentEnabled<NormalActionDoneTag>(SampleUnit, false);
                    

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
                        movementspeed = 1f,
                        dmg = 0,
                        team = newteam
                    });
                    ecb.SetComponentEnabled<PriorityMovingTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<MovingTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<AttackTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<LazyTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<PriorityMoveDoneTag>(PMoveUnit, false);
                    ecb.SetComponentEnabled<NormalActionDoneTag>(PMoveUnit, false);

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

            foreach (var (unit, entity) in SystemAPI.Query<RefRW<SampleUnitComponentData>>().WithEntityAccess())
            {
                unit.ValueRW.dice = Random.CreateFromIndex((uint)entity.Index); //DateTime은 Burst와 호환이 안됨. 시작할 때만 쓰는 거라서 후속적인 영향이 있는지 확인하고 수정할거임
                //Debug.Log(unit.ValueRW.dice.NextInt());
            }

            var DeckManagerEntity = SystemAPI.GetSingletonEntity<DeckListBuffer>();
            DynamicBuffer<DeckListBuffer> DeckListBuffer = SystemAPI.GetBuffer<DeckListBuffer>(DeckManagerEntity);
            
            //var DeckEntity=TestDeckListBuffer[0].HashToDeckEntity;
            //DynamicBuffer<CardListBuffer> CardListBuffer = SystemAPI.GetBuffer<CardListBuffer>(DeckEntity);
            //var CardEntity = CardListBuffer[0].HashToCardEntity;
            //DynamicBuffer<SynergyListBuffer> TestSynBuffer = SystemAPI.GetBuffer<SynergyListBuffer>(CardEntity);
            DeckListData = new NativeList<DeckData>(5,Allocator.Temp){
                {new DeckData{HashToCard = new NativeHashMap<int, int>(20,Allocator.Temp){}, HashToSyn = new NativeHashMap<int, NativeList<int>>(20,Allocator.Temp){}}}
            };
            ReadDeckData(DeckListData, DeckListBuffer,ref state);
            Debug.Log(string.Format("{0}",DeckListData[0].HashToSyn[1][1]));
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        public void ReadDeckData (NativeList<DeckData> decklist, DynamicBuffer<DeckListBuffer> TestDeckListBuffer, ref SystemState system){
            int decknumber = 0;
            foreach(var DeckEntity in TestDeckListBuffer){
                decklist.Add(new DeckData{});
                DynamicBuffer<CardListBuffer> CardListBuffer = SystemAPI.GetBuffer<CardListBuffer>(DeckEntity.HashToDeckEntity);
                int cardnumber = 1;
                foreach(var CardEntity in CardListBuffer){
                    DynamicBuffer<SynergyListBuffer> TestSynBuffer = SystemAPI.GetBuffer<SynergyListBuffer>(CardEntity.HashToCardEntity);
                    decklist[decknumber].HashToCard.Add(cardnumber,SystemAPI.GetComponent<CardHeader>(CardEntity.HashToCardEntity).cardNum);
                    NativeList<int> temp = new NativeList<int>(5,Allocator.Persistent);
                    foreach(var Synergy in TestSynBuffer){
                        temp.Add(Synergy.SynNumber);
                    }
                    decklist[decknumber].HashToSyn.Add(cardnumber,temp);
                    cardnumber++;
                }
                decknumber++;
            }
            return;
        }
        
        public struct DeckData : IComponentData{
            public NativeHashMap<int,int> HashToCard;
            public NativeHashMap<int, NativeList<int>> HashToSyn;
        }

    }
    
}