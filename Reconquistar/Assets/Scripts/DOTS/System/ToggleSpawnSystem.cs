using _1.Scripts.DOTS.Authoring_baker_;
using _1.Scripts.DOTS.Components___Tags;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _1.Scripts.DOTS.System
{
    public partial struct ToggleSpawnSystem : ISystem
    {
        EntityQuery TileQuery;
        [BurstCompile]

        public void Oncreate(ref SystemState state)
        {
            state.RequireForUpdate<SampleSpawnData>();
            TileQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<MapTileAuthoringComponentData>().Build(ref state);
        }

        [BurstCompile]

        public void OnUpdate(ref SystemState state)
        {
            // if (Input.GetMouseButtonDown(0))
            // {
            //     Debug.Log("클릭");
            // }
            //var SampleSpawner = SystemAPI.GetSingleton<WhattoSpawn>();

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }







}
