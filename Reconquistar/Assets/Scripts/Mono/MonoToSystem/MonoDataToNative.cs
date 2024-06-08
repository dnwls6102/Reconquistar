using System;
using System.Collections.Generic;
using _1.Scripts.DOTS.Authoring_baker_;
using DOTS.Authoring_baker_;
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
        private int k = 0;
        private Entity _spawnerEntity;
        private Entity deckentity;
        private EntityManager _entityManager;
        public Dictionary<int, List<int>> deckList = new Dictionary<int, List<int>>()
        {
            {1, new List<int>(){1,2,3,4}},
            {2, new List<int>(){4,5,6,67,7}}
        };
        public NativeList<int> test; 

        private void Awake()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            SpawnEntity();
        }

        // private void LateUpdate()
        // {
        //     if (Input.GetMouseButtonUp(0))
        //     {
        //     }
        // }

        public void SpawnEntity()
        {
            var DeckManagerEntity =_entityManager.CreateEntity();
            var DeckEntity = _entityManager.CreateEntity();
            var CardEntity = _entityManager.CreateEntity();
            _entityManager.SetName(DeckManagerEntity,"DeckManagerEntity");
            _entityManager.SetName(DeckEntity,"DeckEntity");
            _entityManager.SetName(CardEntity,"CardEntity");
            _entityManager.AddBuffer<DeckListBuffer>(DeckManagerEntity);
            _entityManager.AddBuffer<CardListBuffer>(DeckEntity);
            _entityManager.AddBuffer<SynergyListBuffer>(CardEntity);
            _entityManager.AddComponent<DeckHeader>(DeckEntity);
            _entityManager.AddComponent<CardHeader>(CardEntity);
            DynamicBuffer<DeckListBuffer> TestDeckListBuffer =
                _entityManager.GetBuffer<DeckListBuffer>(DeckManagerEntity);
            DynamicBuffer<CardListBuffer> TestBuffer = _entityManager.GetBuffer<CardListBuffer>(DeckEntity);
            DynamicBuffer<SynergyListBuffer> TestSynBuffer = _entityManager.GetBuffer<SynergyListBuffer>(CardEntity);
            TestDeckListBuffer.Add(new DeckListBuffer(){HashToDeckEntity = DeckEntity});
            TestBuffer.Add(new CardListBuffer() { HashToCardEntity = CardEntity });
            TestSynBuffer.Add(new SynergyListBuffer() { SynNumber = 2 });
            TestSynBuffer.Add(new SynergyListBuffer() { SynNumber = 2 });
            TestSynBuffer.Add(new SynergyListBuffer() { SynNumber = 2 });
            _entityManager.AddComponent<DeckLoadingDoneTag>(DeckManagerEntity);
            _entityManager.SetComponentEnabled<DeckLoadingDoneTag>(DeckManagerEntity,true);
            //TestSynBuffer.Add(new SynergyListBuffer() { SynNumber = 2 });
            //프리팹 사용 없이 생성한 엔티티. sampleData는 하나의 덱 엔티티로, 카드들을 엔티티로 이루어진 dynamic buffer로 관리한다.
            //testHashData는 하나의 카드 엔티티. 이 안에 카드가 가진 시너지를 dynamic buffer로 추가한다.



            /*
            _spawnerEntity = _entityManager.CreateEntityQuery(typeof(DataEntityData)).GetSingletonEntity();
            deckentity = _entityManager.GetComponentData<DataEntityData>(_spawnerEntity).DataEntityPrefab;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.Instantiate(deckentity);
            ecb.Playback(_entityManager);
            ecb.Dispose();

            var ecb2 = new EntityCommandBuffer(Allocator.Temp);
            ecb2.Playback(_entityManager);
            ecb2.Dispose();*/
            //DeckcomponentData () 카드 리스트만 줄거에요. 그리고 각 카드의 데이터 구조체 안에는, 해당 카드가 얻은 시너지 효과 리스트, 그리고 몇명의 유닛을 생성할지
            //엔티티 생성할 때 순서: 1. 다음으로 생성하려는 카드의 정보를 가져온다. 2. 그 카드의 숫자와 맞는 유닛 데이터를 SpawnData에서 가져온다
            //3. 카드의 정보에서 현재 가진 시너지 효과 리스트를 참조해서 tagComponent를 추가하거나, 유닛 데이터를 추가한다
            //4. 생성한다(이때, 한줄 넘어가면 다음 줄에 계속)
        }
    }
    
}