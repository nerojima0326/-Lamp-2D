using UnityEngine;

namespace Lampkeeper.Visual
{
    [DisallowMultipleComponent]
    public sealed class LanternFlameAnimation : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] LanternFlameSettings settings;
        float elapsed;

        void OnEnable() => elapsed = 0f;

        void LateUpdate()
        {
            if (settings == null || spriteRenderer == null || settings.frames == null || settings.frames.Length == 0) return;
            elapsed += Time.deltaTime;
            float fps = Mathf.Max(0.1f, settings.framesPerSecond);
            float cycle = settings.frames.Length / fps;
            elapsed %= cycle;
            int index = Mathf.Min(settings.frames.Length - 1, Mathf.FloorToInt(elapsed * fps));
            Sprite frame = settings.frames[index];
            if (frame == null) return;
            spriteRenderer.sprite = frame;
            transform.localScale = Vector3.one * (settings.cellHeight / frame.bounds.size.y);
        }
    }
}
