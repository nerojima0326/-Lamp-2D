using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>불꽃의 미세한 흔들림에 쓰는 부드러운 노이즈.</summary>
    public static class FlickerNoise
    {
        /// <summary>-1 ~ 1 사이 값. seed와 time이 같으면 항상 같고, time이 조금 변하면 값도 조금만 변한다.</summary>
        public static float Sample(float seed, float time)
            => Mathf.Clamp(Mathf.PerlinNoise(seed, time) * 2f - 1f, -1f, 1f);
    }
}
