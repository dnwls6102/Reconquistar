using Unity.Collections;
using Unity.Entities;

namespace Mono.MonoToSystem
{
    public struct Deck : IComponentData
    {
        public NativeList<int> Card;
        public int team;
    }
    
}

/*
 * 덱으로 이루어진 리스트 길이 4 =>NativeList
 * 덱은 카드로 이루어진 리스트[30] => NativeList
 * team =0 1 2
 * Managed Data(OOP)를 데이터 지향적 데이터 구조로 만들 수 있는 곳이 1.Monobehaviour 2. SystemBase(ECS 소속, Isystem이랑 똑같음, Managed Data에 접근할 수 있음 단, BurstCompile 불가)
 * MonoBehaviour에서 하는법: 엔티티를 만든다. 
 */