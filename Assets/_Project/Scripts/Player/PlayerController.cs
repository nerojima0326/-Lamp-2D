using Lampkeeper.Light;
using UnityEngine;

namespace Lampkeeper.Player
{
    /// <summary>
    /// PlayerMotor를 Rigidbody2D와 입력에 연결한다. 이동 계산은 전부 PlayerMotor가 맡는다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        const int GroundHitBufferSize = 8;

        [SerializeField] PlayerMovementSettings settings;

        [Tooltip("플레이어가 다루는 등불 (선택)")]
        [SerializeField] Lantern lantern;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[GroundHitBufferSize];

        Rigidbody2D body;
        Collider2D bodyCollider;
        PlayerInputReader input;

        public PlayerMotor Motor { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            // 상승/하강 중력이 다르므로 중력은 Rigidbody2D가 아니라 PlayerMotor가 계산한다.
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (settings == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: {nameof(PlayerMovementSettings)}가 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }
            if (bodyCollider == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: 같은 오브젝트에 Collider2D가 필요합니다.", this);
                enabled = false;
                return;
            }

            Motor = new PlayerMotor(settings);
            input = new PlayerInputReader();
        }

        /// <summary>속도를 없애고 지정한 위치로 옮긴다. 들고 있던 등불도 손으로 가져온다.</summary>
        public void Teleport(Vector2 position)
        {
            // 보간 중인 Transform이 이전 위치에서 끌려오지 않도록 둘 다 옮긴다.
            body.position = position;
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            Motor?.ResetDash();
            if (lantern != null)
                lantern.SnapToHand();
        }

        void OnEnable() => input?.Enable();

        void OnDisable()
        {
            input?.Disable();
            Motor?.ResetDash();
        }

        void OnDestroy() => input?.Dispose();

        void Update()
        {
            // 버튼 입력은 렌더 프레임 단위로 들어오므로 여기서 받아 버퍼에 넣고, 점프 자체는 FixedUpdate에서 처리한다.
            if (input.JumpPressedThisFrame)
                Motor.RequestJump();
            if (input.DashPressedThisFrame)
                Motor.RequestDash();

            if (lantern != null)
            {
                lantern.HolderFacing = Motor.Facing;
                if (input.LanternPressedThisFrame)
                    lantern.ThrowOrRecall(input.Move);
            }
        }

        void FixedUpdate()
        {
            var frameInput = new MotorInput(input.Move.x, input.JumpHeld);
            body.linearVelocity = Motor.Step(body.linearVelocity, frameInput, IsTouchingGround(), Time.fixedDeltaTime);
        }

        bool IsTouchingGround()
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(settings.groundLayers);

            int hitCount = bodyCollider.Cast(Vector2.down, filter, groundHits, settings.groundCheckDistance);
            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].normal.y >= settings.minGroundNormalY)
                    return true;
            }
            return false;
        }
    }
}
