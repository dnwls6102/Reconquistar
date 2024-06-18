using _1.Scripts.DOTS.Components___Tags;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using Unity.Mathematics;

public class SamplePMoveUnitAuthoring : MonoBehaviour
{
    public float movementspeed;
    public int hp;
    public int dmg;
    public int range;
    public class SamplePMoveUnitAuthoringBaker : Baker<SamplePMoveUnitAuthoring>
    {
        public override void Bake(SamplePMoveUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SampleUnitComponentData
            {
                movementspeed = authoring.movementspeed,
                hp = authoring.hp,
                dmg = authoring.dmg,
                team = 0,
                range = authoring.range,
            });
            AddComponent(entity, new PriorityMovingTag());
            AddComponent(entity, new MovingTag());
            AddComponent(entity, new AttackTag());
            AddComponent(entity, new LazyTag());
            AddComponent(entity, new TargetEntityData());
            AddComponent(entity, new PriorityMoveDoneTag());
            AddComponent(entity, new NormalActionDoneTag());
            AddComponent(entity, new AttackDoneTag());
        }
    }
}

//SampleUnitAuthoring, TargetEntityData는 SampleUnitAuthoring.cs에 있음