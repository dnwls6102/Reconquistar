using Unity.Entities;

namespace _1.Scripts.DOTS.Components___Tags
{
    public struct AnimationSettings : IComponentData
    {
        public int IdleHash;
        public int WalkHash;
        public int AttackHash;
    }
}