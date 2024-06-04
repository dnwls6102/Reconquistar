using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// 각 플레이어의 덱 리스트를 가져와서 NativeList에 추가하는 클래스. 
// 덱 리스트를 가져오는 건 Monopoly 씬에서, NativeList에 추가하는 건 Sample 씬에서 진행해야 함.
// 현재 테스트용 클래스이며, 추후에 GameManager에 통합할 수 있음
namespace Mono.MonoToSystem
{
    public class MonoDataToNative : MonoBehaviour
    {
        private Entity deckentity;
        public static NativeList<Deck> test = new NativeList<Deck>(4,Allocator.Temp)
        {
            {new Deck(){Card = new NativeList<CardInfo>(30,Allocator.Temp){}}}
        };
        public void SpawnEntity()
              {
                  var ecb = new EntityCommandBuffer(Allocator.Temp);
                  ecb.Instantiate(deckentity);
                  //DeckcomponentData () 카드 리스트만 줄거에요. 그리고 각 카드의 데이터 구조체 안에는, 해당 카드가 얻은 시너지 효과 리스트, 그리고 몇명의 유닛을 생성할지
                  //엔티티 생성할 때 순서: 1. 다음으로 생성하려는 카드의 정보를 가져온다. 2. 그 카드의 숫자와 맞는 유닛 데이터를 SpawnData에서 가져온다
                  //3. 카드의 정보에서 현재 가진 시너지 효과 리스트를 참조해서 tagComponent를 추가하거나, 유닛 데이터를 추가한다
                  //4. 생성한다(이때, 한줄 넘어가면 다음 줄에 계속)
              }
    }
    
}