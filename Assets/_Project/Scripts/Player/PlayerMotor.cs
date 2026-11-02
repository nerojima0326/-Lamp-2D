using System;
using UnityEngine;

namespace Lampkeeper.Player
{
    /// <summary>
    /// 플레이어 이동의 순수 로직. 현재 속도·입력·접지 여부를 받아 다음 속도를 돌려준다.
    /// Rigidbody2D와 입력 장치에 의존하지 않으므로 EditMode 테스트로 검증한다.
    /// </summary>
    /// <remarks>
    /// 매 스텝 입력으로 받는 속도는 물리 엔진이 충돌을 해결한 뒤의 값이어야 한다.
    /// 그래야 벽·천장에 막힌 속도가 다음 스텝에 그대로 반영된다.
    /// </remarks>
    public sealed class PlayerMotor
    {
        // 바닥 캐스트에 닿아도 위로 움직이는 중이면(점프 직후) 접지로 보지 않는다.
        const float GroundedMaxVerticalSpeed = 0.01f;

        readonly PlayerMovementSettings settings;

        float coyoteTimer;
        float jumpBufferTimer;
        float lastOutputVelocityY;

        public PlayerMotor(PlayerMovementSettings settings)
        {
            this.settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public event Action Jumped;

        /// <summary>착지 순간 호출된다. 인자는 착지 직전의 낙하 속도 (양수).</summary>
        public event Action<float> Landed;

        public bool IsGrounded { get; private set; }

        /// <summary>플레이어가 시작한 점프 궤적 위에 있는가. 가변 점프와 최고점 보정은 이때만 적용된다.</summary>
        public bool IsJumping { get; private set; }

        /// <summary>마지막으로 입력한 좌우 방향 (1 = 오른쪽, -1 = 왼쪽). 입력이 없으면 유지된다.</summary>
        public int Facing { get; private set; } = 1;

        /// <summary>점프 입력을 버퍼에 넣는다. 버튼이 눌린 렌더 프레임(Update)에서 호출한다.</summary>
        public void RequestJump() => jumpBufferTimer = settings.jumpBufferTime;

        public Vector2 Step(Vector2 velocity, MotorInput input, bool groundDetected, float deltaTime)
        {
            UpdateContactState(velocity.y, groundDetected, deltaTime);

            if (input.MoveX != 0f)
                Facing = input.MoveX > 0f ? 1 : -1;
            velocity.x = StepHorizontal(velocity.x, input.MoveX, deltaTime);

            bool jumped = TryConsumeJump();
            if (jumped)
                velocity.y = settings.JumpVelocity;
            velocity.y = StepVertical(velocity.y, input.JumpHeld, jumped, deltaTime);

            jumpBufferTimer -= deltaTime;
            lastOutputVelocityY = velocity.y;
            return velocity;
        }

        void UpdateContactState(float velocityY, bool groundDetected, float deltaTime)
        {
            // 직전 스텝에 내보낸 상승 속도가 물리 처리 뒤 사라졌다면 천장에 부딪힌 것이다. 점프를 끊는다.
            if (IsJumping && lastOutputVelocityY > 0f && velocityY <= 0f)
                IsJumping = false;

            bool grounded = groundDetected && velocityY <= GroundedMaxVerticalSpeed;
            if (grounded)
            {
                if (!IsGrounded)
                    Landed?.Invoke(Mathf.Max(0f, -lastOutputVelocityY));
                IsJumping = false;
                coyoteTimer = settings.coyoteTime;
            }
            else
            {
                coyoteTimer -= deltaTime;
            }
            IsGrounded = grounded;
        }

        float StepHorizontal(float velocityX, float moveX, float deltaTime)
        {
            float maxSpeed = settings.maxRunSpeed;
            float targetX = Mathf.Clamp(moveX, -1f, 1f) * maxSpeed;

            // 같은 방향으로 더 빨라질 때만 가속 값을 쓰고, 멈추거나 방향을 틀 때는 감속 값을 쓴다.
            bool speedingUp = targetX * velocityX >= 0f && Mathf.Abs(targetX) > Mathf.Abs(velocityX);
            float duration = speedingUp
                ? (IsGrounded ? settings.groundAccelerationTime : settings.airAccelerationTime)
                : (IsGrounded ? settings.groundDecelerationTime : settings.airDecelerationTime);
            if (duration <= 0f || maxSpeed <= 0f)
                return targetX;

            AnimationCurve curve = speedingUp ? settings.accelerationCurve : settings.decelerationCurve;
            float speedRatio = Mathf.Clamp01(Mathf.Abs(velocityX) / maxSpeed);
            float rate = maxSpeed / duration * curve.Evaluate(speedRatio);
            return Mathf.MoveTowards(velocityX, targetX, rate * deltaTime);
        }

        bool TryConsumeJump()
        {
            if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
                return false;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            IsGrounded = false;
            IsJumping = true;
            Jumped?.Invoke();
            return true;
        }

        float StepVertical(float velocityY, bool jumpHeld, bool jumpedThisStep, float deltaTime)
        {
            float gravity = settings.Gravity * GravityMultiplier(velocityY, jumpHeld);
            // 점프한 스텝은 중력을 반 스텝만 적용한다(중점 적분).
            // 그러면 고정 타임스텝 크기와 상관없이 설정한 점프 높이에 정확히 도달한다.
            float gravityStep = jumpedThisStep ? deltaTime * 0.5f : deltaTime;
            velocityY -= gravity * gravityStep;
            return Mathf.Max(velocityY, -settings.maxFallSpeed);
        }

        float GravityMultiplier(float velocityY, bool jumpHeld)
        {
            if (IsJumping)
            {
                if (!jumpHeld && velocityY > 0f)
                    return settings.LowJumpGravityMultiplier;
                if (jumpHeld && Mathf.Abs(velocityY) < settings.apexHangSpeedThreshold)
                    return settings.apexHangGravityMultiplier;
            }
            return velocityY > 0f ? 1f : settings.fallGravityMultiplier;
        }
    }
}
