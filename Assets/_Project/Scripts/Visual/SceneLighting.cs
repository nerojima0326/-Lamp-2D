using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lampkeeper.Visual
{
    /// <summary>씬 전체의 바탕 분위기: 전역 빛의 색·세기와 카메라 배경색을 팔레트에 맞춘다.</summary>
    [ExecuteAlways]
    public sealed class SceneLighting : MonoBehaviour
    {
        [SerializeField] VisualPalette palette;
        [SerializeField] LightingProfile profile;
        [SerializeField] Light2D globalLight;
        [SerializeField] Camera targetCamera;

        void LateUpdate()
        {
            if (palette == null || profile == null)
                return;

            if (globalLight != null)
            {
                globalLight.color = palette.ambientLightColor;
                globalLight.intensity = profile.globalIntensity;
            }
            if (targetCamera != null)
                targetCamera.backgroundColor = palette.voidColor;
        }
    }
}
