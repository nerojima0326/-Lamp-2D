using Lampkeeper.Player;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class PlayerMotorTests
    {
        // Unity 기본 고정 타임스텝
        const float Dt = 0.02f;
        const int MaxSimulationSteps = 1000;
        // 이산 적분 오차 허용 범위 (높이 대비 비율)
        const float HeightTolerance = 0.03f;
        const float Epsilon = 1e-4f;
        const float TimingWindow = 0.1f;

        static readonly MotorInput NoInput = new MotorInput(0f, false);
        static readonly MotorInput JumpHeld = new MotorInput(0f, true);

        PlayerMovementSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            settings.coyoteTime = TimingWindow;
            settings.jumpBufferTime = TimingWindow;
            settings.accelerationCurve = AnimationCurve.Constant(0f, 1f, 1f);
            settings.decelerationCurve = AnimationCurve.Constant(0f, 1f, 1f);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        // 창(window) 안쪽: 창의 절반만큼 지난 시점
        static int StepsWithin(float window) => Mathf.FloorToInt(window * 0.5f / Dt);

        // 창 바깥: 창이 확실히 끝난 시점
        static int StepsBeyond(float window) => Mathf.CeilToInt(window / Dt) + 1;

        // ---------- 점프 높이 ----------

        [Test]
        public void HeldJump_ReachesJumpHeight()
        {
            settings.apexHangGravityMultiplier = 1f; // 최고점 보정 없이 순수 포물선만 확인
            var sim = new FlatGroundSimulation(settings);

            sim.Motor.RequestJump();
            sim.StepUntilLanded(jumpHeld: true);

            Assert.That(sim.Apex, Is.EqualTo(settings.jumpHeight).Within(settings.jumpHeight * HeightTolerance));
        }

        [Test]
        public void TappedJump_ReachesMinJumpHeight()
        {
            var sim = new FlatGroundSimulation(settings);

            sim.Motor.RequestJump();
            sim.StepUntilLanded(jumpHeld: false);

            Assert.That(sim.Apex, Is.EqualTo(settings.minJumpHeight).Within(settings.minJumpHeight * HeightTolerance));
        }

        [Test]
        public void ReleasingJumpMidway_ApexIsBetweenMinAndMax()
        {
            var sim = new FlatGroundSimulation(settings);
            int holdSteps = Mathf.RoundToInt(settings.timeToApex * 0.3f / Dt);

            sim.Motor.RequestJump();
            for (int i = 0; i < holdSteps; i++)
                sim.Step(jumpHeld: true);
            sim.StepUntilLanded(jumpHeld: false);

            Assert.That(sim.Apex, Is.GreaterThan(settings.minJumpHeight * (1f + HeightTolerance)));
            Assert.That(sim.Apex, Is.LessThan(settings.jumpHeight * (1f - HeightTolerance)));
        }

        [Test]
        public void ApexHang_ExtendsAirTime()
        {
            settings.apexHangGravityMultiplier = 1f;
            var withoutHang = new FlatGroundSimulation(settings);
            withoutHang.Motor.RequestJump();
            int stepsWithoutHang = withoutHang.StepUntilLanded(jumpHeld: true);

            settings.apexHangGravityMultiplier = 0.5f;
            var withHang = new FlatGroundSimulation(settings);
            withHang.Motor.RequestJump();
            int stepsWithHang = withHang.StepUntilLanded(jumpHeld: true);

            Assert.That(stepsWithHang, Is.GreaterThan(stepsWithoutHang));
        }

        // ---------- 코요테 타임 / 점프 버퍼 ----------

        [Test]
        public void CoyoteTime_JumpShortlyAfterLeavingGround_Jumps()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = motor.Step(Vector2.zero, NoInput, groundDetected: true, Dt);
            for (int i = 0; i < StepsWithin(settings.coyoteTime); i++)
                velocity = motor.Step(velocity, NoInput, groundDetected: false, Dt);

            motor.RequestJump();
            motor.Step(velocity, NoInput, groundDetected: false, Dt);

            Assert.That(motor.IsJumping, Is.True);
        }

        [Test]
        public void CoyoteTime_JumpAfterWindow_DoesNotJump()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = motor.Step(Vector2.zero, NoInput, groundDetected: true, Dt);
            for (int i = 0; i < StepsBeyond(settings.coyoteTime); i++)
                velocity = motor.Step(velocity, NoInput, groundDetected: false, Dt);

            motor.RequestJump();
            motor.Step(velocity, NoInput, groundDetected: false, Dt);

            Assert.That(motor.IsJumping, Is.False);
        }

        [Test]
        public void JumpBuffer_PressedShortlyBeforeLanding_JumpsOnLanding()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = motor.Step(Vector2.zero, NoInput, groundDetected: false, Dt);

            motor.RequestJump();
            for (int i = 0; i < StepsWithin(settings.jumpBufferTime); i++)
                velocity = motor.Step(velocity, NoInput, groundDetected: false, Dt);
            Assert.That(motor.IsJumping, Is.False, "공중에서 바로 점프하면 안 된다");

            velocity.y = 0f; // 착지: 물리 엔진이 낙하 속도를 없앤다
            motor.Step(velocity, NoInput, groundDetected: true, Dt);

            Assert.That(motor.IsJumping, Is.True);
        }

        [Test]
        public void JumpBuffer_PressedTooEarly_DoesNotJumpOnLanding()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = motor.Step(Vector2.zero, NoInput, groundDetected: false, Dt);

            motor.RequestJump();
            for (int i = 0; i < StepsBeyond(settings.jumpBufferTime); i++)
                velocity = motor.Step(velocity, NoInput, groundDetected: false, Dt);

            velocity.y = 0f;
            motor.Step(velocity, NoInput, groundDetected: true, Dt);

            Assert.That(motor.IsJumping, Is.False);
        }

        [Test]
        public void Jump_WhileRisingNearGround_CannotJumpAgain()
        {
            var motor = new PlayerMotor(settings);
            int jumpCount = 0;
            motor.Jumped += () => jumpCount++;

            motor.RequestJump();
            Vector2 velocity = motor.Step(Vector2.zero, JumpHeld, groundDetected: true, Dt);
            // 점프 직후 몇 스텝은 바닥 캐스트가 아직 바닥에 닿는다. 상승 중이므로 접지로 보면 안 된다.
            for (int i = 0; i < StepsWithin(settings.coyoteTime); i++)
                velocity = motor.Step(velocity, JumpHeld, groundDetected: true, Dt);

            motor.RequestJump();
            motor.Step(velocity, JumpHeld, groundDetected: true, Dt);

            Assert.That(jumpCount, Is.EqualTo(1));
        }

        // ---------- 천장 / 낙하 / 착지 ----------

        [Test]
        public void CeilingBonk_EndsJumpAndFallsImmediately()
        {
            var motor = new PlayerMotor(settings);
            motor.RequestJump();
            Vector2 velocity = motor.Step(Vector2.zero, JumpHeld, groundDetected: true, Dt);
            Assert.That(velocity.y, Is.GreaterThan(0f));

            velocity.y = 0f; // 천장에 부딪혀 물리 엔진이 상승 속도를 없앤다
            velocity = motor.Step(velocity, JumpHeld, groundDetected: false, Dt);

            Assert.That(motor.IsJumping, Is.False);
            float expectedFallStep = settings.Gravity * settings.fallGravityMultiplier * Dt;
            Assert.That(velocity.y, Is.EqualTo(-expectedFallStep).Within(Epsilon), "최고점 보정 없이 바로 낙하 중력을 받아야 한다");
        }

        [Test]
        public void Falling_IsClampedToMaxFallSpeed()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = Vector2.zero;

            for (int i = 0; i < MaxSimulationSteps; i++)
            {
                velocity = motor.Step(velocity, NoInput, groundDetected: false, Dt);
                Assert.That(velocity.y, Is.GreaterThanOrEqualTo(-settings.maxFallSpeed - Epsilon));
            }

            Assert.That(velocity.y, Is.EqualTo(-settings.maxFallSpeed).Within(Epsilon));
        }

        [Test]
        public void Landing_RaisesLandedOnceWithImpactSpeed()
        {
            var sim = new FlatGroundSimulation(settings, startHeight: 2f);
            int landedCount = 0;
            float impactSpeed = 0f;
            sim.Motor.Landed += speed =>
            {
                landedCount++;
                impactSpeed = speed;
            };

            for (int i = 0; i < MaxSimulationSteps; i++)
                sim.Step();

            Assert.That(landedCount, Is.EqualTo(1));
            Assert.That(impactSpeed, Is.GreaterThan(0f));
        }

        // ---------- 달리기 ----------

        [Test]
        public void Run_ReachesMaxSpeedWithinAccelerationTime_AndNeverExceedsIt()
        {
            var motor = new PlayerMotor(settings);
            var runRight = new MotorInput(1f, false);
            Vector2 velocity = Vector2.zero;

            int steps = Mathf.CeilToInt(settings.groundAccelerationTime / Dt) + 1;
            for (int i = 0; i < steps; i++)
            {
                velocity = StepGrounded(motor, velocity, runRight);
                Assert.That(velocity.x, Is.LessThanOrEqualTo(settings.maxRunSpeed + Epsilon));
            }

            Assert.That(velocity.x, Is.EqualTo(settings.maxRunSpeed).Within(Epsilon));
        }

        [Test]
        public void Run_ReleasingInput_StopsWithinDecelerationTime()
        {
            var motor = new PlayerMotor(settings);
            Vector2 velocity = new Vector2(settings.maxRunSpeed, 0f);

            int steps = Mathf.CeilToInt(settings.groundDecelerationTime / Dt) + 1;
            for (int i = 0; i < steps; i++)
                velocity = StepGrounded(motor, velocity, NoInput);

            Assert.That(velocity.x, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void Run_Reversing_ChangesDirectionWithinDecelerationTime()
        {
            var motor = new PlayerMotor(settings);
            var runLeft = new MotorInput(-1f, false);
            Vector2 velocity = new Vector2(settings.maxRunSpeed, 0f);

            int steps = Mathf.CeilToInt(settings.groundDecelerationTime / Dt) + 1;
            for (int i = 0; i < steps; i++)
                velocity = StepGrounded(motor, velocity, runLeft);

            Assert.That(velocity.x, Is.LessThan(0f));
        }

        [Test]
        public void Facing_FollowsLastHorizontalInput()
        {
            var motor = new PlayerMotor(settings);
            Assert.That(motor.Facing, Is.EqualTo(1));

            Vector2 velocity = StepGrounded(motor, Vector2.zero, new MotorInput(-1f, false));
            Assert.That(motor.Facing, Is.EqualTo(-1));

            velocity = StepGrounded(motor, velocity, NoInput);
            Assert.That(motor.Facing, Is.EqualTo(-1), "입력이 없으면 마지막 방향을 유지한다");

            StepGrounded(motor, velocity, new MotorInput(0.5f, false));
            Assert.That(motor.Facing, Is.EqualTo(1));
        }

        // 바닥 위에서 한 스텝: 물리 엔진처럼 바닥으로 파고드는 속도를 없앤다.
        static Vector2 StepGrounded(PlayerMotor motor, Vector2 velocity, MotorInput input)
        {
            velocity = motor.Step(velocity, input, groundDetected: true, Dt);
            velocity.y = Mathf.Max(velocity.y, 0f);
            return velocity;
        }

        /// <summary>y = 0에 평평한 바닥만 있는 세계. 물리 엔진 대신 바닥 충돌만 흉내 낸다.</summary>
        sealed class FlatGroundSimulation
        {
            public readonly PlayerMotor Motor;
            public Vector2 Position; // 발 위치
            public Vector2 Velocity;
            public float Apex;

            public FlatGroundSimulation(PlayerMovementSettings settings, float startHeight = 0f)
            {
                Motor = new PlayerMotor(settings);
                Position = new Vector2(0f, startHeight);
                Apex = startHeight;
            }

            public void Step(float moveX = 0f, bool jumpHeld = false)
            {
                bool groundDetected = Position.y <= 0f;
                Velocity = Motor.Step(Velocity, new MotorInput(moveX, jumpHeld), groundDetected, Dt);
                Position += Velocity * Dt;
                if (Position.y < 0f)
                {
                    Position.y = 0f;
                    Velocity.y = 0f;
                }
                Apex = Mathf.Max(Apex, Position.y);
            }

            /// <summary>공중에 떴다가 다시 바닥에 닿을 때까지 진행하고, 걸린 스텝 수를 돌려준다.</summary>
            public int StepUntilLanded(bool jumpHeld)
            {
                for (int i = 1; i <= MaxSimulationSteps; i++)
                {
                    Step(jumpHeld: jumpHeld);
                    if (Position.y <= 0f)
                        return i;
                }
                Assert.Fail("제한 스텝 안에 착지하지 않았습니다.");
                return MaxSimulationSteps;
            }
        }
    }
}
