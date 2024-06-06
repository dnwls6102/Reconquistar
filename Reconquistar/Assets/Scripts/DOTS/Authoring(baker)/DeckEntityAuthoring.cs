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
                DynamicBuffer<testData> testDatas = AddBuffer<testData>(entity);
                
            }
        }
    }

[InternalBufferCapacity(20)]
    public struct testData : IBufferElementData
    {
        public Entity HashToCardEntity;
    }

    public struct testSyn : IBufferElementData
    {
        public int SynNumber;
    }
}