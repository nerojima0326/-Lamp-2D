using Lampkeeper.Player;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public sealed class PlayerDashTests
    {
        const float Dt = 0.02f;
        const float Tolerance = 0.0001f;
        static readonly MotorInput Idle = new MotorInput(0f, false);
        PlayerMovementSettings settings;
        PlayerMotor motor;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            motor = new PlayerMotor(settings);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        [TestCase(-1)]
        [TestCase(1)]
        public void Dash_UsesFacingWithoutMoveInput(int direction)
        {
            motor.Step(Vector2.zero, new MotorInput(direction, false), true, Dt);
            motor.RequestDash();
            Vector2 velocity = motor.Step(Vector2.zero, Idle, true, Dt);
            Assert.That(velocity.x, Is.EqualTo(direction * settings.dashSpeed));
            Assert.That(motor.IsDashing, Is.True);
        }

        [Test]
        public void Dash_LocksDirectionUntilFinished()
        {
            motor.RequestDash();
            Vector2 velocity = motor.Step(Vector2.zero, Idle, false, Dt);
            velocity = motor.Step(velocity, new MotorInput(-1f, false), false, Dt);
            Assert.That(velocity.x, Is.GreaterThan(0f));
            Assert.That(motor.Facing, Is.EqualTo(1));
        }

        [TestCase(0.02f)]
        [TestCase(0.03f)]
        public void Dash_CoversConfiguredDistanceAndThenFalls(float dt)
        {
            motor.RequestDash();
            Vector2 velocity = new Vector2(0f, -12f);
            float distance = 0f;
            int dashSteps = Mathf.CeilToInt(settings.dashDuration / dt);
            for (int i = 0; i < dashSteps; i++)
            {
                velocity = motor.Step(velocity, Idle, false, dt);
                Assert.That(velocity.y, Is.Zero);
                distance += velocity.x * dt;
            }
            Assert.That(distance, Is.EqualTo(settings.dashSpeed * settings.dashDuration).Within(Tolerance));
            velocity = motor.Step(velocity, Idle, false, dt);
            Assert.That(motor.IsDashing, Is.False);
            Assert.That(velocity.y, Is.LessThan(0f));
            Assert.That(Mathf.Abs(velocity.x), Is.LessThanOrEqualTo(settings.maxRunSpeed));
        }

        [Test]
        public void Cooldown_RejectsPressWithoutQueuingIt()
        {
            motor.RequestDash();
            Vector2 velocity = motor.Step(Vector2.zero, Idle, true, Dt);
            for (int i = 0; i < 10; i++) velocity = motor.Step(velocity, Idle, true, Dt);
            motor.RequestDash();
            velocity = motor.Step(velocity, Idle, true, Dt);
            Assert.That(motor.IsDashing, Is.False);
            for (int i = 0; i < 30; i++)
            {
                velocity = motor.Step(velocity, Idle, true, Dt);
                Assert.That(motor.IsDashing, Is.False);
            }
            motor.RequestDash();
            motor.Step(velocity, Idle, true, Dt);
            Assert.That(motor.IsDashing, Is.True);
        }

        [Test]
        public void RepeatedPress_DoesNotExtendCurrentDash()
        {
            Vector2 velocity = Vector2.zero;
            for (int i = 0; i < 9; i++)
            {
                motor.RequestDash();
                velocity = motor.Step(velocity, Idle, false, Dt);
            }
            Assert.That(motor.IsDashing, Is.False);
        }

        [Test]
        public void Dash_CancelsAscentAndDoesNotLeaveCoyoteJumpInAir()
        {
            motor.RequestJump();
            Vector2 velocity = motor.Step(Vector2.zero, new MotorInput(0f, true), true, Dt);
            Assert.That(motor.IsJumping, Is.True);
            motor.RequestDash();
            for (int i = 0; i < 8; i++) velocity = motor.Step(velocity, Idle, false, Dt);
            motor.RequestJump();
            velocity = motor.Step(velocity, Idle, false, Dt);
            Assert.That(motor.IsJumping, Is.False);
            Assert.That(velocity.y, Is.LessThan(0f));
        }

        [Test]
        public void Reset_ClearsActiveDashAndPendingRequest()
        {
            motor.RequestDash();
            motor.Step(Vector2.zero, Idle, false, Dt);
            motor.RequestDash();
            motor.ResetDash();
            motor.Step(Vector2.zero, Idle, false, Dt);
            Assert.That(motor.IsDashing, Is.False);
            motor.RequestDash();
            motor.Step(Vector2.zero, Idle, false, Dt);
            Assert.That(motor.IsDashing, Is.True);
        }
    }
}
