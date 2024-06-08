﻿using Unity.Collections;
using Unity.Entities;

namespace _1.Scripts.DOTS.Components___Tags
{
    //유닛 생성 시 초기화하는 용도. 유닛 생성할 때만 사용되고, 여기 있는 데이터를 각 엔티티에 적용시키고, 그 이후에는 각 엔티티에서 각자의 데이터를 가져온다.
    public struct unit : IComponentData
    {
        public int hp; //체력
        public int toHit; //명중 보정값
        public int toEvade; //최피 보정값
        public int dmg; //대미지 보정값
        public int defence; //방어도
        public int dmgDice; //대미지 주사위의 면(예시.6면체, 8면체, 10면체..)
        public int dmgDiceCount; //대미지 주사위 개수(예시.6면체 주사위 1개/2개...)
        public int order; //사기
        public int range; //사거리
        public bool HasPmove; // 자유 이동 유무
        public bool HasPatk; // 자유 공격 유무
    }
}