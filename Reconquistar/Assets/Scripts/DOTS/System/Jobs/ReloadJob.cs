using System.Collections.Generic;
using _1.Scripts.DOTS.Components___Tags;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.System.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct ReloadJob : IJobEntity
    {
        public void Execute(ref ShootTag cUnit, EnabledRefRW<ReloadingDoneTag> doneTag)
        {
            if (cUnit.bullets == 0 || cUnit.bullets < 0) //현재 원거리 공격 유닛의 총알 갯수가 0개 이하일 경우
            {
                Debug.Log("Reloading");
                cUnit.bullets = cUnit.Maxbullets;
                doneTag.ValueRW = true;
            }
        }
    }
}
