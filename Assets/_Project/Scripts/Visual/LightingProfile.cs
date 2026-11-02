using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>
    /// 빛 연출 튜닝 값 (강도, 반경 비율, 그림자, 흔들림). 색은 <see cref="VisualPalette"/>에 있다.
    /// 반경 자체는 게임플레이 값(LightSource.Radius)을 따르고, 여기서는 그 비율만 정한다.
    /// </summary>
    [CreateAssetMenu(fileName = "LightingProfile", menuName = "Lampkeeper/Visual/Lighting Profile")]
    public sealed class LightingProfile : ScriptableObject
    {
        [Header("전역 빛")]
        [Tooltip("완전한 검정이 아니라 실루엣이 희미하게 보일 정도")]
        [Range(0f, 1f)] public float globalIntensity = 0.12f;

        [Header("등불 - 밝은 핵")]
        [Tooltip("핵 반경 = 빛 반경 × 이 비율")]
        [Range(0f, 1f)] public float coreRadiusRatio = 0.4f;

        [Min(0f)] public float coreIntensity = 1.5f;

        [Tooltip("감쇠 강도. 높을수록 중심에 몰리고 가장자리가 빨리 어두워진다.")]
        [Range(0f, 1f)] public float coreFalloff = 0.6f;

        [Header("등불 - 은은한 번짐")]
        [Tooltip("번짐 반경 = 빛 반경 × 이 값. 1이면 번짐의 끝이 빛 발판 판정 경계와 같다. 1보다 크면 판정보다 넓게 보여 오해를 준다.")]
        [Range(0.5f, 1.5f)] public float haloRadiusScale = 1f;

        [Tooltip("번짐의 최대 밝기 구간 = 빛 반경 × 이 비율")]
        [Range(0f, 1f)] public float haloInnerRatio = 0.3f;

        [Min(0f)] public float haloIntensity = 0.8f;

        [Tooltip("낮을수록 판정 경계 근처까지 밝게 남는다")]
        [Range(0f, 1f)] public float haloFalloff = 0.35f;

        [Tooltip("배경이 없는 공기 중에도 보이는 빛의 양")]
        [Range(0f, 1f)] public float haloVolumeIntensity = 0.18f;

        [Header("불씨")]
        [Min(0f)] public float emberIntensity = 0.6f;

        [Range(0f, 1f)] public float emberFalloff = 0.5f;

        [Range(0f, 1f)] public float emberVolumeIntensity = 0.06f;

        [Header("그림자")]
        [Range(0f, 1f)] public float shadowIntensity = 0.9f;

        [Range(0f, 1f)] public float shadowSoftness = 0.3f;

        [Tooltip("공기 중의 빛이 벽에 가려지는 정도 (벽 뒤로 드리우는 빛줄기 그림자)")]
        [Range(0f, 1f)] public float shadowVolumeIntensity = 0.85f;

        [Tooltip("지형 그림자 모양을 블록 안쪽으로 줄이는 양 (유닛). 크게 하면 벽 모서리로 빛이 샌다.")]
        [Min(0f)] public float terrainShadowTrim = 0f;

        [Header("흔들림")]
        [Min(0f)] public float flickerSpeed = 2.2f;

        [Tooltip("밝기가 흔들리는 비율 (0.08 = ±8%)")]
        [Range(0f, 0.5f)] public float flickerIntensityAmount = 0.08f;

        [Tooltip("반경이 흔들리는 비율. 판정 경계와 어긋나지 않게 작게 둔다.")]
        [Range(0f, 0.2f)] public float flickerRadiusAmount = 0.02f;
    }
}
