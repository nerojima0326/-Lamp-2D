using Lampkeeper.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Visual
{
    // PlayerSpriteAnimation이 프레임과 방향을 갱신한 뒤 복사한다.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerDashTrail : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] SpriteRenderer source;
        [SerializeField] DashTrailSettings settings;

        GameObject poolRoot;
        SpriteRenderer[] ghosts;
        float[] ages;
        int next;
        float untilNext;
        bool wasDashing;

        void Awake()
        {
            if (settings == null || source == null || player == null) return;
            poolRoot = new GameObject("[Visual] Dash Afterimages");
            SceneManager.MoveGameObjectToScene(poolRoot, gameObject.scene);
            int count = Mathf.Max(1, settings.poolSize);
            ghosts = new SpriteRenderer[count];
            ages = new float[count];
            for (int i = 0; i < count; i++)
            {
                var ghost = new GameObject($"Afterimage {i}").AddComponent<SpriteRenderer>();
                ghost.transform.SetParent(poolRoot.transform, false);
                ghost.sharedMaterial = settings.material != null ? settings.material : source.sharedMaterial;
                ghost.enabled = false;
                ghosts[i] = ghost;
            }
        }

        void LateUpdate()
        {
            if (ghosts == null || settings == null || source == null || player == null) return;
            float lifetime = Mathf.Max(0.01f, settings.lifetime);
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (!ghosts[i].enabled) continue;
                ages[i] += Time.deltaTime;
                float remaining = 1f - Mathf.Clamp01(ages[i] / lifetime);
                Color color = settings.tint;
                color.a *= remaining * remaining;
                ghosts[i].color = color;
                if (remaining <= 0f) ghosts[i].enabled = false;
            }

            bool dashing = player.Motor != null && player.Motor.IsDashing;
            if (dashing && source.enabled && source.sprite != null)
            {
                untilNext -= Time.deltaTime;
                if (!wasDashing || untilNext <= 0f)
                {
                    Emit();
                    untilNext = Mathf.Max(0.01f, settings.spawnInterval);
                }
            }
            wasDashing = dashing;
        }

        void Emit()
        {
            SpriteRenderer ghost = ghosts[next];
            ghost.sprite = source.sprite;
            ghost.flipX = source.flipX;
            ghost.flipY = source.flipY;
            ghost.sortingLayerID = source.sortingLayerID;
            ghost.sortingOrder = source.sortingOrder + settings.sortingOrderOffset;
            ghost.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            ghost.transform.localScale = source.transform.lossyScale;
            ghost.color = settings.tint;
            ghost.enabled = true;
            ages[next] = 0f;
            next = (next + 1) % ghosts.Length;
        }

        void OnDisable()
        {
            if (ghosts != null)
                foreach (var ghost in ghosts)
                    if (ghost != null) ghost.enabled = false;
            wasDashing = false;
            untilNext = 0f;
        }

        void OnDestroy()
        {
            if (poolRoot != null) Destroy(poolRoot);
        }
    }
}
