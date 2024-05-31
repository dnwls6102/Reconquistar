using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.Authoring_baker_
{
    public class MapMakerAuthoring : MonoBehaviour
    {
        public GameObject MapTilePrefab;
        public GameObject SpriteTilePrefab; //인덱스 꼬임 방지를 위한 SpriteTile
        //타일맵 가로 세로 갯수
        public int number;
        //맵 한 칸 넓이
        public float width;
        // public bool _isDebug; //맵 크기를 5 * 5로 제한하는 토글
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
                    width = authoring.width,
                    // debugFlag = authoring._isDebug,
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
        // public bool debugFlag; // 맵 타일 전체 크기를 5*5로 제한
    }
}