using _1.Scripts.DOTS.Components___Tags;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using Unity.Mathematics;

public class SampleShootingUnitAuthoring : MonoBehaviour
{
    public float movementspeed;
    public int hp;
    public int dmg;
    public int bullet;
    public int maxNum; //최대 총알 갯수
    public GameObject bulletEntity;
    public class SampleShootingUnitAuthoringBaker : Baker<SampleShootingUnitAuthoring>
    {
        public override void Bake(SampleShootingUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SampleUnitComponentData
            {
                movementspeed = authoring.movementspeed,
                hp = authoring.hp,
                dmg = authoring.dmg,
                team = 0,
            });
            AddComponent(entity, new ShootTag
            {
                bullets = authoring.bullet,
                BulletEntity = GetEntity(authoring.bulletEntity, TransformUsageFlags.Dynamic),
                Maxbullets = authoring.maxNum,
            });
            AddComponent(entity, new MovingTag());
            AddComponent(entity, new AttackTag());
            AddComponent(entity, new LazyTag());
            AddComponent(entity, new TargetEntityData());
        }
    }
}

public struct ShootTag : IComponentData
{
    public int bullets; //총알 / 화살 잔량
    public Entity BulletEntity; //총알 엔티티
    public int Maxbullets; //최대 총알 / 화살 잔량
}

//SampleUnitAuthoring, TargetEntityData는 SampleUnitAuthoring.cs에 있음