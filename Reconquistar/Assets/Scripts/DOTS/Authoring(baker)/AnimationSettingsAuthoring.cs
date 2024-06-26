using _1.Scripts.DOTS.Components___Tags;
using Unity.Entities;
using UnityEngine;

namespace _1.Scripts.DOTS.Authoring_baker_
{
    public class AnimationSettingsAuthoring : MonoBehaviour
    {
        private class AnimationSettingsBaker : Baker<AnimationSettingsAuthoring>
        {
            public override void Bake(AnimationSettingsAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new AnimationSettings
                {
                    IdleHash = Animator.StringToHash("idle"),
                    WalkHash = Animator.StringToHash("walk"),
                    AttackHash = Animator.StringToHash("attack")
                });
            }
        }
    }
}