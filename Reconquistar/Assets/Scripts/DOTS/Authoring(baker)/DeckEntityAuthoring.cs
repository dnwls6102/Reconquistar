using _1.Scripts.DOTS.Authoring_baker_;
using Mono.MonoToSystem;
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
                AddComponent(entity, new Deck());
            }
        }
    }
    
}