using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>한 광원이 밝히는 원형 영역. 판정은 모두 이 값으로 한다.</summary>
    public readonly struct LightCircle
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly float Intensity;

        public LightCircle(Vector2 center, float radius, float intensity)
        {
            Center = center;
            Radius = radius;
            Intensity = intensity;
        }

        public bool Contains(Vector2 point) => Radius > 0f && (point - Center).sqrMagnitude <= Radius * Radius;

        /// <summary>영역의 어느 한 부분이라도 빛 안에 들어오는가.</summary>
        public bool Overlaps(Rect area)
        {
            var closest = new Vector2(
                Mathf.Clamp(Center.x, area.xMin, area.xMax),
                Mathf.Clamp(Center.y, area.yMin, area.yMax));
            return Contains(closest);
        }

        /// <summary>노출도: 중심에서 Intensity, 가장자리로 갈수록 선형으로 줄어 반경 밖에서는 0.</summary>
        public float ExposureAt(Vector2 point)
        {
            if (Radius <= 0f)
                return 0f;
            float distance = Vector2.Distance(point, Center);
            return distance >= Radius ? 0f : Intensity * (1f - distance / Radius);
        }
    }
}
