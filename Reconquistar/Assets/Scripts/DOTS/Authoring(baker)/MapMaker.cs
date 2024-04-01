using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.Authoring_baker_
{
    public class MapMakerAuthoring : MonoBehaviour
    {
        public GameObject MapTilePrefab;
        public GameObject SpriteTilePrefab;
        //타일맵 가로 세로 갯수
        public int number;
        //맵 한 칸 넓이
        public float width;
        private class MapMakerBaker : Baker<MapMakerAuthoring>
        {
            public override void Bake(MapMakerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MapMakerComponentData
                {
                    MapTilePrefab = GetEntity(authoring.MapTilePrefab, TransformUsageFlags.None),
                    SpriteTilePrefab = GetEntity(authoring.SpriteTilePrefab, TransformUsageFlags.Dynamic),
                    number = authoring.number,
                    width = authoring.width
                });
            }
        }
    }

    public struct MapMakerComponentData : IComponentData
    {
        public Entity MapTilePrefab;
        public Entity SpriteTilePrefab;
        //타일맵 가로 세로 갯수
        public int number;
        //맵 한 칸 넓이
        public float width;
    }
}