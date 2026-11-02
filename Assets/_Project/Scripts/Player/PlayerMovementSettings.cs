using UnityEngine;

namespace Lampkeeper.Player
{
    /// <summary>
    /// 플레이어 이동 튜닝 값. PlayerMotor가 매 물리 스텝마다 직접 읽으므로
    /// 플레이 중 인스펙터에서 바꾸면 즉시 반영된다.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Lampkeeper/Player Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [Header("달리기")]
        [Tooltip("최고 달리기 속도 (유닛/초)")]
        [Min(0f)] public float maxRunSpeed = 8.5f;

        [Tooltip("지상: 정지 → 최고 속도까지 걸리는 시간 (초). 0이면 즉시.")]
        [Min(0f)] public float groundAccelerationTime = 0.06f;

        [Tooltip("지상: 최고 속도 → 정지까지 걸리는 시간 (초). 방향 전환에도 쓰인다. 0이면 즉시.")]
        [Min(0f)] public float groundDecelerationTime = 0.04f;

        [Tooltip("공중: 정지 → 최고 속도까지 걸리는 시간 (초)")]
        [Min(0f)] public float airAccelerationTime = 0.1f;

        [Tooltip("공중: 최고 속도 → 정지까지 걸리는 시간 (초)")]
        [Min(0f)] public float airDecelerationTime = 0.16f;

        [Tooltip("가로축: 현재 속도 / 최고 속도 (0~1), 세로축: 가속도 배수. 평평하면 일정한 가속. 0이 되는 구간이 있으면 그 속도에서 멈춘다.")]
        public AnimationCurve accelerationCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("가로축: 현재 속도 / 최고 속도 (0~1), 세로축: 감속도 배수.")]
        public AnimationCurve decelerationCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Header("점프")]
        [Tooltip("점프 버튼을 끝까지 누르고 있을 때의 최고 높이 (유닛)")]
        [Min(0.01f)] public float jumpHeight = 3.2f;

        [Tooltip("점프 버튼을 바로 뗐을 때의 높이 (유닛). 가변 점프의 하한.")]
        [Min(0.01f)] public float minJumpHeight = 0.9f;

        [Tooltip("최고점까지 걸리는 시간 (초). 작을수록 빠르고 묵직한 점프.")]
        [Min(0.01f)] public float timeToApex = 0.38f;

        [Tooltip("최고점 부근(세로 속도 절댓값이 이 값 이하)에서 점프를 누르고 있으면 중력이 줄어든다 (유닛/초)")]
        [Min(0f)] public float apexHangSpeedThreshold = 2f;

        [Tooltip("최고점 부근 중력 배수. 1이면 체공 보정 없음.")]
        [Range(0.1f, 1f)] public float apexHangGravityMultiplier = 0.6f;

        [Header("낙하")]
        [Tooltip("하강 중 중력 배수. 클수록 떨어질 때 묵직하다.")]
        [Min(1f)] public float fallGravityMultiplier = 1.8f;

        [Tooltip("최대 낙하 속도 (유닛/초)")]
        [Min(0f)] public float maxFallSpeed = 20f;

        [Header("관용 입력")]
        [Tooltip("발판을 벗어난 뒤에도 점프를 받아 주는 시간 (초)")]
        [Min(0f)] public float coyoteTime = 0.1f;

        [Tooltip("착지 전에 누른 점프를 기억해 두는 시간 (초)")]
        [Min(0f)] public float jumpBufferTime = 0.1f;

        [Header("접지 판정")]
        [Tooltip("바닥으로 인정할 레이어")]
        public LayerMask groundLayers = ~0;

        [Tooltip("콜라이더를 아래로 캐스트하는 거리 (유닛)")]
        [Min(0f)] public float groundCheckDistance = 0.05f;

        [Tooltip("법선의 y가 이 값 이상인 면만 바닥으로 본다 (0.7 ≈ 45도 경사)")]
        [Range(0f, 1f)] public float minGroundNormalY = 0.7f;

        /// <summary>점프 높이와 최고점 시간에서 유도한 기본 중력 가속도 (양수).</summary>
        public float Gravity => 2f * jumpHeight / (timeToApex * timeToApex);

        /// <summary>jumpHeight까지 오르는 데 필요한 초기 점프 속도.</summary>
        public float JumpVelocity => 2f * jumpHeight / timeToApex;

        /// <summary>점프 버튼을 뗀 뒤 상승 중의 중력 배수. 바로 떼면 정확히 minJumpHeight까지 오른다.</summary>
        public float LowJumpGravityMultiplier => jumpHeight / minJumpHeight;

        void OnValidate()
        {
            minJumpHeight = Mathf.Min(minJumpHeight, jumpHeight);
        }
    }
}
