using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>등불 튜닝 값. Lantern이 매 프레임 읽으므로 플레이 중 바꾸면 바로 반영된다.</summary>
    [CreateAssetMenu(fileName = "LanternSettings", menuName = "Lampkeeper/Lantern Settings")]
    public sealed class LanternSettings : ScriptableObject
    {
        [Header("빛")]
        [Tooltip("등불이 밝히는 반경 (유닛). 이 안의 빛 발판이 실체화된다.")]
        [Min(0f)] public float lightRadius = 3.5f;

        [Tooltip("등불 빛의 세기. 그림자 계열 적의 노출도 판정에 쓰인다.")]
        [Min(0f)] public float lightIntensity = 1f;

        [Tooltip("등불이 손을 떠났을 때 플레이어 주변에 남는 불씨 반경 (유닛). 발밑까지 닿지 않으면 발판이 무너진다.")]
        [Min(0f)] public float emberRadius = 0.6f;

        [Tooltip("불씨 빛의 세기")]
        [Min(0f)] public float emberIntensity = 0.3f;

        [Header("들고 있기")]
        [Tooltip("플레이어 중심에서 등불을 드는 위치. x는 바라보는 방향으로 뒤집힌다.")]
        public Vector2 holdOffset = new Vector2(0.4f, -0.32f);

        [Header("던지기")]
        [Tooltip("던진 등불이 날아가는 속도 (유닛/초)")]
        [Min(0.01f)] public float throwSpeed = 24f;

        [Tooltip("벽에 부딪히지 않으면 이 거리에서 멈춰 공중에 고정된다 (유닛)")]
        [Min(0f)] public float throwDistance = 7f;

        [Tooltip("조준 입력이 이 크기보다 작으면 바라보는 방향으로 던진다")]
        [Range(0f, 1f)] public float aimDeadzone = 0.35f;

        [Tooltip("날아가는 등불의 충돌 반경 (유닛)")]
        [Min(0f)] public float collisionRadius = 0.2f;

        [Tooltip("던진 등불이 부딪혀 멈추는 레이어. 등불을 든 사람의 콜라이더는 항상 무시한다.")]
        public LayerMask collisionLayers = ~0;

        [Header("회수")]
        [Tooltip("돌아오는 속도 (유닛/초). 벽을 통과한다.")]
        [Min(0.01f)] public float returnSpeed = 30f;

        [Tooltip("손까지 이 거리 안으로 오면 잡은 것으로 본다 (유닛)")]
        [Min(0f)] public float catchDistance = 0.5f;
    }
}
