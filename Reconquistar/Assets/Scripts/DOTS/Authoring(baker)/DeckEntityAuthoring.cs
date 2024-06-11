using _1.Scripts.DOTS.Authoring_baker_;
using Mono.MonoToSystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring_baker_
{
    public class DeckEntityAuthoringAuthoring : MonoBehaviour
    {
        private class DeckEntityAuthoringBaker : Baker<DeckEntityAuthoringAuthoring>
        {
            public override void Bake(DeckEntityAuthoringAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                //AddComponent(entity, new DeckListData());
                DynamicBuffer<CardListBuffer> testDatas = AddBuffer<CardListBuffer>(entity);
                
            }
        }
    }


    [InternalBufferCapacity(20)]
    public struct CardListBuffer : IBufferElementData
    {
        public Entity HashToCardEntity;
    }

    [InternalBufferCapacity(10)]
    public struct SynergyListBuffer : IBufferElementData
    {
        public int SynNumber;
    } 
    
    [InternalBufferCapacity(4)]
    public struct DeckListBuffer : IBufferElementData
    {
        public Entity HashToDeckEntity;
    }
    public struct DeckHeader : IComponentData
    {
        public int team;
    }

    public struct CardHeader : IComponentData
    {
        public int cardNum;
    }

    public struct DeckLoadingDoneTag : IComponentData, IEnableableComponent
    {
    }
}