using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace _1.Scripts.DOTS.Authoring_baker_
{
    public class MapTileAuthoring : MonoBehaviour
    {
        private class MapTileAuthoringBaker : Baker<MapTileAuthoring>
        {
            public override void Bake(MapTileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }

    public struct MapTileAuthoringComponentData : IComponentData
    { 
        public int2 index;
        public int soldier;
        // 0이면 없음 1이면 빨강팀 2면 파랑팀 있음
    }
}