using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>
    /// 빛이 닿을 때만 밟을 수 있는 발판. 영역의 일부라도 빛 안에 있으면 실체화된다.
    /// 상태 전환은 <see cref="LightPlatformStateMachine"/>이 맡고, 여기서는 콜라이더와 투명도만 반영한다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LightPlatform : MonoBehaviour
    {
        const int OccupantBufferSize = 8;

        [SerializeField] LightPlatformSettings settings;

        readonly Collider2D[] occupants = new Collider2D[OccupantBufferSize];

        BoxCollider2D box;
        SpriteRenderer spriteRenderer;
        Color baseColor;
        LightPlatformStateMachine machine;

        public LightPlatformState State => machine.State;

        void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                baseColor = spriteRenderer.color;

            if (settings == null)
            {
                Debug.LogError($"{nameof(LightPlatform)}: {nameof(LightPlatformSettings)}가 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }
            machine = new LightPlatformStateMachine(settings);
        }

        void Start()
        {
            // 모든 광원이 OnEnable에서 등록된 뒤, 연출 없이 현재 빛 상태로 맞춘다.
            machine.Snap(LightField.Main.IsLit(WorldArea()));
            Apply();
        }

        void FixedUpdate()
        {
            Rect area = WorldArea();
            bool occupied = !box.enabled && IsOccupied(area);
            machine.Step(LightField.Main.IsLit(area), occupied, Time.fixedDeltaTime);
            Apply();
        }

        void Apply()
        {
            box.enabled = machine.CollisionEnabled;
            if (spriteRenderer == null)
                return;

            Color color = baseColor;
            color.a = baseColor.a * Mathf.Lerp(settings.ghostAlpha, 1f, machine.Opacity);
            spriteRenderer.color = color;
        }

        // 콜라이더가 꺼져 있어도 계산되도록 Transform에서 구한다. 회전은 고려하지 않는다.
        Rect WorldArea()
        {
            Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
            size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            Vector2 center = transform.TransformPoint(box.offset);
            return new Rect(center - size * 0.5f, size);
        }

        bool IsOccupied(Rect area)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(settings.occupantLayers);

            int count = Physics2D.OverlapBox(area.center, area.size, 0f, filter, occupants);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = occupants[i].attachedRigidbody;
                if (body != null && body.bodyType != RigidbodyType2D.Static)
                    return true;
            }
            return false;
        }
    }
}
