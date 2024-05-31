using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _1.Scrpits.DOTS.Authoring_baker_
{
    public class BulletAuthoring : MonoBehaviour
    {
        public float vel = 1.1f;
        private class BulletAuthoringBaker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Bullet
                {
                    velocity = authoring.vel,
                });
            }
        }
    }

    public struct Bullet : IComponentData
    {
        public float velocity;
    }
}
