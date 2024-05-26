using _1.Scripts.DOTS.Components___Tags;
using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct AttackDoneJob : IJobEntity
    {
        public void Execute(EnabledRefRW<AttackTag> attackTagEnabled,
            EnabledRefRW<NormalActionDoneTag> normalActionDonTag)
        {
            attackTagEnabled.ValueRW = false;
            normalActionDonTag.ValueRW = true;
        }
    }
}