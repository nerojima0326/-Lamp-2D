using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>
    /// 플레이어의 등불. 던지면 날아가다 벽에 닿거나 최대 거리에서 멈춰 그 자리를 밝히고, 회수하면 손으로 돌아온다.
    /// 상태와 움직임은 <see cref="LanternMotion"/>이 맡는다.
    /// </summary>
    public sealed class Lantern : MonoBehaviour
    {
        const int CastBufferSize = 8;

        [SerializeField] LanternSettings settings;

        [Tooltip("등불을 드는 사람. 들고 있을 때 따라다니고, 회수하면 이쪽으로 돌아온다.")]
        [SerializeField] Transform holder;

        [Tooltip("등불 자체의 광원")]
        [SerializeField] LightSource lightSource;

        [Tooltip("등불이 손을 떠나도 들고 있던 사람 곁에 남는 불씨 광원 (선택)")]
        [SerializeField] LightSource ember;

        readonly RaycastHit2D[] castHits = new RaycastHit2D[CastBufferSize];

        LanternMotion motion;
        LanternCast cast;
        Rigidbody2D holderBody;

        public LanternState State => motion.State;

        /// <summary>들고 있는 사람이 바라보는 방향 (1 = 오른쪽, -1 = 왼쪽).</summary>
        public int HolderFacing { get; set; } = 1;

        Vector2 HoldPosition => (Vector2)holder.position + new Vector2(settings.holdOffset.x * HolderFacing, settings.holdOffset.y);

        void Awake()
        {
            if (settings == null || holder == null || lightSource == null)
            {
                Debug.LogError($"{nameof(Lantern)}: settings, holder, lightSource를 모두 지정해야 합니다.", this);
                enabled = false;
                return;
            }

            holderBody = holder.GetComponent<Rigidbody2D>();
            motion = new LanternMotion(settings);
            cast = CastAgainstWorld;
        }

        /// <summary>들고 있으면 aimInput 방향으로 던지고, 손을 떠나 있으면 회수한다.</summary>
        public void ThrowOrRecall(Vector2 aimInput)
        {
            if (motion.State == LanternState.Held)
                motion.TryThrow(HoldPosition, LanternAim.Resolve(aimInput, HolderFacing, settings.aimDeadzone));
            else
                motion.TryRecall();
        }

        /// <summary>어디에 있든 바로 손으로 가져온다. 들고 있는 사람을 순간이동시킨 뒤 호출한다.</summary>
        public void SnapToHand()
        {
            motion.SnapToHeld(HoldPosition);
            transform.position = motion.Position;
        }

        void Update()
        {
            // 튜닝 값이 플레이 중에도 바로 반영되도록 매 프레임 맞춘다.
            lightSource.Radius = settings.lightRadius;
            lightSource.Intensity = settings.lightIntensity;
            if (ember != null)
            {
                ember.Radius = settings.emberRadius;
                ember.Intensity = settings.emberIntensity;
            }

            motion.Step(Time.deltaTime, HoldPosition, cast);
            transform.position = motion.Position;
        }

        bool CastAgainstWorld(Vector2 origin, Vector2 direction, float distance, out float hitDistance)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(settings.collisionLayers);

            int count = Physics2D.CircleCast(origin, settings.collisionRadius, direction, filter, castHits, distance);
            bool hit = false;
            hitDistance = distance;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D candidate = castHits[i];
                if (holderBody != null && candidate.rigidbody == holderBody)
                    continue;
                if (!hit || candidate.distance < hitDistance)
                {
                    hitDistance = candidate.distance;
                    hit = true;
                }
            }
            return hit;
        }
    }
}
