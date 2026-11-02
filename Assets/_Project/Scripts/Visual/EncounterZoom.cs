using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>감지 결과의 유지 시간과 줌 보간. 프레임마다 카메라/물리 조회를 하지 않는 순수 로직.</summary>
    public sealed class EncounterZoom
    {
        float remaining;
        float velocity;
        public bool InEncounter { get; private set; }
        public float Size { get; private set; }

        public void Reset(float size)
        {
            Size = size;
            remaining = velocity = 0f;
            InEncounter = false;
        }

        public float Step(bool enemyPresent, float deltaTime, EncounterCameraSettings settings)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            if (enemyPresent) remaining = Mathf.Max(0f, settings.returnDelay);
            else remaining = Mathf.Max(0f, remaining - deltaTime);
            InEncounter = enemyPresent || remaining > 0f;
            float target = InEncounter ? Mathf.Max(settings.soloSize, settings.encounterSize) : settings.soloSize;
            float duration = target > Size ? settings.zoomOutTime : settings.zoomInTime;
            Size = duration <= 0f ? target : Mathf.SmoothDamp(Size, target, ref velocity, duration, Mathf.Infinity, deltaTime);
            return Size;
        }

        public static bool Overlaps(Vector2 cameraCenter, float halfHeight, float aspect, float margin, Bounds enemy)
        {
            Vector2 half = new Vector2(halfHeight * Mathf.Max(0.01f, aspect), halfHeight) + Vector2.one * margin;
            return enemy.max.x >= cameraCenter.x - half.x && enemy.min.x <= cameraCenter.x + half.x
                && enemy.max.y >= cameraCenter.y - half.y && enemy.min.y <= cameraCenter.y + half.y;
        }
    }
}
