using Lampkeeper.Light;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lampkeeper.Visual
{
    /// <summary>
    /// 게임플레이 광원(<see cref="LightSource"/>)을 화면의 2D 라이트로 보여 준다.
    /// 반경은 매 프레임 LightSource에서 읽으므로, 보이는 빛의 경계가 항상 빛 발판 판정 경계를 따라간다.
    /// 게임플레이 쪽은 이 컴포넌트를 모른다.
    /// </summary>
    [ExecuteAlways]
    public sealed class LightSourceVisual : MonoBehaviour
    {
        public enum Role
        {
            Lantern,
            Ember,
        }

        // 반경 흔들림이 밝기 흔들림과 다른 패턴이 되도록 노이즈 좌표를 띄운다.
        const float RadiusNoiseOffset = 17.31f;
        const float SeedRange = 100f;

        [SerializeField] LightSource source;
        [SerializeField] VisualPalette palette;
        [SerializeField] LightingProfile profile;
        [SerializeField] Role role = Role.Lantern;

        [Tooltip("밝은 핵 (불씨는 이것 하나만 쓴다)")]
        [SerializeField] Light2D core;

        [Tooltip("은은한 번짐 (등불만)")]
        [SerializeField] Light2D halo;

        float noiseSeed;

        void OnEnable()
        {
            // 광원마다 흔들림 패턴이 달라지도록 노이즈 좌표를 무작위로 띄운다.
            noiseSeed = Random.value * SeedRange;
        }

        // Lantern.Update가 반경을 맞춘 뒤에 반영한다.
        void LateUpdate()
        {
            if (source == null || palette == null || profile == null || core == null)
                return;

            float intensityScale = 1f;
            float radiusScale = 1f;
            if (Application.isPlaying)
            {
                float time = Time.time * profile.flickerSpeed;
                intensityScale += FlickerNoise.Sample(noiseSeed, time) * profile.flickerIntensityAmount;
                radiusScale += FlickerNoise.Sample(noiseSeed + RadiusNoiseOffset, time) * profile.flickerRadiusAmount;
            }

            float radius = source.Radius * radiusScale;
            if (role == Role.Lantern)
                ApplyLantern(radius, intensityScale);
            else
                ApplyEmber(radius, intensityScale);
        }

        void ApplyLantern(float radius, float intensityScale)
        {
            Configure(core, palette.lanternCoreColor, profile.coreIntensity * intensityScale,
                innerRadius: 0f, outerRadius: radius * profile.coreRadiusRatio, profile.coreFalloff, volume: 0f, castsShadows: true);

            if (halo != null)
            {
                Configure(halo, palette.lanternHaloColor, profile.haloIntensity * intensityScale,
                    innerRadius: radius * profile.haloInnerRatio, outerRadius: radius * profile.haloRadiusScale,
                    profile.haloFalloff, profile.haloVolumeIntensity, castsShadows: true);
            }
        }

        void ApplyEmber(float radius, float intensityScale)
        {
            // 불씨는 작아서 그림자를 만들지 않는다 (그림자 라이트 수를 아끼는 목적도 있다).
            Configure(core, palette.emberColor, profile.emberIntensity * intensityScale,
                innerRadius: 0f, outerRadius: radius, profile.emberFalloff, profile.emberVolumeIntensity, castsShadows: false);
        }

        void Configure(Light2D light, Color color, float intensity, float innerRadius, float outerRadius, float falloff, float volume, bool castsShadows)
        {
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightOuterRadius = outerRadius;
            light.pointLightInnerRadius = Mathf.Min(innerRadius, outerRadius);
            light.falloffIntensity = falloff;

            light.volumetricEnabled = volume > 0f;
            light.volumeIntensity = volume;

            light.shadowsEnabled = castsShadows;
            light.shadowIntensity = profile.shadowIntensity;
            light.shadowSoftness = profile.shadowSoftness;
            light.volumetricShadowsEnabled = castsShadows && volume > 0f;
            light.shadowVolumeIntensity = profile.shadowVolumeIntensity;
        }
    }
}
