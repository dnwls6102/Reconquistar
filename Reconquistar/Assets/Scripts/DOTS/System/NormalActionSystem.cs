using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using _1.Scripts.DOTS.System.Jobs;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

namespace _1.Scripts.DOTS.System
{
    public partial struct NormalActionSystem : ISystem
    {
        EntityQuery spawnerQuery;
        EntityQuery unitQuery;
        EntityQuery tileQuery;
        ComponentLookup<SampleUnitComponentData> sampleUnitLookup;
        // Start is called before the first frame update
        void OnCreate(ref SystemState state)
        {
            spawnerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartPause>().Build(ref state);
            unitQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SampleUnitComponentData>().Build(ref state);
            tileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
            sampleUnitLookup = state.GetComponentLookup<SampleUnitComponentData>(true);
        }

        // Update is called once per frame
        void OnUpdate(ref SystemState state)
        {
            sampleUnitLookup.Update(ref state);
            if (spawnerQuery.CalculateEntityCount() == 0)
            {
                return;
            }
        }
    }
}
