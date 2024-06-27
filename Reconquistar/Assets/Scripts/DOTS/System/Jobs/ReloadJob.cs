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
        public void Execute(ref ShootTag cUnit, EnabledRefRW<ReloadingDoneTag> doneTag, EnabledRefRW<NormalActionDoneTag> normaldonTag)
        {
            if (cUnit.bullets == 0 || cUnit.bullets < 0) //현재 원거리 공격 유닛의 총알 갯수가 0개 이하일 경우
            {
                Debug.Log("Reloading");
                cUnit.bullets = cUnit.Maxbullets;
                doneTag.ValueRW = true;
                //normaldonTag.ValueRW = true; 개념상 재장전도 일반 행동 중의 일부지만, 일반 행동 완료 태그는 일반 이동 후에 붙어야함
            }
        }
    }
}
