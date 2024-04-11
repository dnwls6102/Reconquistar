using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _1.Scripts.DOTS.System
{
    public partial struct SampleSpawn : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SampleSpawnData>();
            state.RequireForUpdate<MapTileAuthoringComponentData>();
            state.RequireForUpdate<MapMakerComponentData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var SampleSpawner = SystemAPI.GetSingleton<SampleSpawnData>();

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var SampleUnits = new NativeArray<Entity>(SampleSpawner.number, Allocator.Temp);
            ecb.Instantiate(SampleSpawner.SampleEntityPrefab, SampleUnits);

            var query = SystemAPI.QueryBuilder().WithAll<SampleUnitComponentData>().WithAll<LocalTransform>().WithAspect<SampleUnitAspect>().Build();
            var queryMask = query.GetEntityQueryMask();
            int x = 0;
            int y = 0;
            int newteam = 0;
            int n = 0;
            MapMakerComponentData mapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();
            var tileQuery = SystemAPI.QueryBuilder().WithAll<MapTileAuthoringComponentData>().Build();
            NativeArray<MapTileAuthoringComponentData> tiles = tileQuery.ToComponentDataArray<MapTileAuthoringComponentData>(Allocator.Temp);

            foreach (var SampleUnit in SampleUnits)
            {
                ecb.SetComponentForLinkedEntityGroup(SampleUnit, queryMask, new SampleUnitComponentData
                {
                    index = new int2(x, y),
                    hp = 3,
                    movementspeed = 1.5f,
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
            ecb.Playback(state.EntityManager);
            tileQuery.CopyFromComponentDataArray(tiles);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}