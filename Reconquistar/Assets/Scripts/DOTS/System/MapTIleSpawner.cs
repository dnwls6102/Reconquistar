using _1.Scripts.DOTS.Authoring_baker_;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _1.Scripts.DOTS.System
{
    public partial struct MapTileSpawner : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapMakerComponentData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //1번만 돌아야 해서
            state.Enabled = false;
            var MapMaker = SystemAPI.GetSingleton<MapMakerComponentData>();

            // 타일 생성
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var tiles = new NativeArray<Entity>(MapMaker.number * MapMaker.number, Allocator.Temp);
            //var spriteTiles = new NativeArray<Entity>(MapMaker.number * MapMaker.number, Allocator.Temp);
            ecb.Instantiate(MapMaker.MapTilePrefab, tiles);
            //ecb.Instantiate(MapMaker.SpriteTilePrefab, spriteTiles);

            // 타일 정렬
            int x = 0;
            int y = 0;

            for (int i = 0; i < tiles.Length; i++)
            {
                ecb.AddComponent(tiles[i], new MapTileAuthoringComponentData
                {
                    index = new int2(x, y),
                    soldier = 0,
                });

                ecb.SetComponent(tiles[i], new LocalTransform()
                {
                    Position = new float3(x, y * MapMaker.width, 0),
                    Scale = 1
                });

                if (x != MapMaker.number - 1)
                {
                    x++;
                }
                else
                {
                    x = 0;
                    y++;
                }

            }

            ecb.Playback(state.EntityManager);

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}