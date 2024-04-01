using Unity.Entities;

namespace _1.Scripts.DOTS.Components___Tags
{
    public readonly partial struct SampleUnitAspect : IAspect
    {
        public readonly EnabledRefRO<MovingTag> movingTag;
        public readonly EnabledRefRO<AttackTag> AttackTag;
        public readonly EnabledRefRO<LazyTag> LazyTag;
    }
}