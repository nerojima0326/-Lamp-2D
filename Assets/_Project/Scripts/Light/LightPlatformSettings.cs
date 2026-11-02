using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>빛 발판 튜닝 값. 플레이 중 바꾸면 바로 반영된다.</summary>
    [CreateAssetMenu(fileName = "LightPlatformSettings", menuName = "Lampkeeper/Light Platform Settings")]
    public sealed class LightPlatformSettings : ScriptableObject
    {
        [Tooltip("빛을 벗어난 뒤 사라질 때까지의 시간 (초). 이 동안은 계속 밟을 수 있다.")]
        [Min(0f)] public float dissolveTime = 0.2f;

        [Tooltip("다시 빛이 닿았을 때 완전히 나타나기까지의 시간 (초)")]
        [Min(0f)] public float formTime = 0.15f;

        [Tooltip("사라진 발판이 남기는 희미한 흔적의 불투명도. 0이면 완전히 보이지 않는다.")]
        [Range(0f, 1f)] public float ghostAlpha = 0.12f;

        [Tooltip("이 레이어의 움직이는 몸이 겹쳐 있으면 실체화(충돌 켜기)를 미룬다")]
        public LayerMask occupantLayers = ~0;
    }
}
