using System;
using UnityEngine;

namespace Lampkeeper.Visual
{
    [CreateAssetMenu(fileName = "LanternFlameSettings", menuName = "Lampkeeper/Visual/Lantern Flame")]
    public sealed class LanternFlameSettings : ScriptableObject
    {
        public Sprite[] frames = Array.Empty<Sprite>();
        [Min(0.1f)] public float framesPerSecond = 6f;
        [Tooltip("투명 여백을 포함한 셀 높이. 등불 자체는 이보다 작게 보인다.")]
        [Min(0.01f)] public float cellHeight = 0.65f;
    }
}
