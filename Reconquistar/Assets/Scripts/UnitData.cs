using Unity.Entities;

namespace UnitData
{
    public class UnitData
    {
        public int Card { get; private set; }
        public unit Number { get; private set; }
        public UnitData(int card, unit number)
        {
            Card = card;
            Number = number;
        }
    }
    public struct unit : IComponentData
    {
        public int hp;
        public int toHit;
        public int toEvade;
        public int dmg;
        public int defence;

    }
}