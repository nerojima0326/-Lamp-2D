using Lampkeeper.Light;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class LanternMotionTests
    {
        const float Dt = 0.02f;
        const int MaxSteps = 1000;
        const float Epsilon = 1e-3f;

        static readonly LanternCast NoObstacle = (Vector2 origin, Vector2 direction, float distance, out float hitDistance) =>
        {
            hitDistance = distance;
            return false;
        };

        LanternSettings settings;
        LanternMotion motion;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<LanternSettings>();
            settings.throwSpeed = 20f;
            settings.throwDistance = 6f;
            settings.returnSpeed = 30f;
            settings.catchDistance = 0.5f;
            motion = new LanternMotion(settings);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        // x = wallX에 서 있는 세로 벽
        static LanternCast WallAt(float wallX) => (Vector2 origin, Vector2 direction, float distance, out float hitDistance) =>
        {
            hitDistance = distance;
            if (direction.x <= 0f)
                return false;
            float toWall = (wallX - origin.x) / direction.x;
            if (toWall > distance)
                return false;
            hitDistance = Mathf.Max(0f, toWall);
            return true;
        };

        void StepWhile(LanternState state, Vector2 holdPosition, LanternCast cast)
        {
            for (int i = 0; i < MaxSteps && motion.State == state; i++)
                motion.Step(Dt, holdPosition, cast);
            Assert.That(motion.State, Is.Not.EqualTo(state), "제한 스텝 안에 상태가 바뀌지 않았습니다");
        }

        [Test]
        public void Held_FollowsHoldPosition()
        {
            motion.Step(Dt, new Vector2(2f, 3f), NoObstacle);

            Assert.That(motion.State, Is.EqualTo(LanternState.Held));
            Assert.That(motion.Position, Is.EqualTo(new Vector2(2f, 3f)));
        }

        [Test]
        public void Throw_WithoutObstacle_PlantsInMidairAtThrowDistance()
        {
            Assert.That(motion.TryThrow(Vector2.zero, Vector2.right), Is.True);

            StepWhile(LanternState.Thrown, Vector2.zero, NoObstacle);

            Assert.That(motion.State, Is.EqualTo(LanternState.Planted));
            Assert.That(motion.Position.x, Is.EqualTo(settings.throwDistance).Within(Epsilon));
        }

        [Test]
        public void Throw_Diagonal_TravelsThrowDistanceAlongAim()
        {
            motion.TryThrow(Vector2.zero, new Vector2(1f, 1f));

            StepWhile(LanternState.Thrown, Vector2.zero, NoObstacle);

            Assert.That(motion.Position.magnitude, Is.EqualTo(settings.throwDistance).Within(Epsilon));
            Assert.That(motion.Position.x, Is.EqualTo(motion.Position.y).Within(Epsilon));
        }

        [Test]
        public void Throw_IntoWall_PlantsAtWall()
        {
            motion.TryThrow(Vector2.zero, Vector2.right);

            StepWhile(LanternState.Thrown, Vector2.zero, WallAt(3f));

            Assert.That(motion.State, Is.EqualTo(LanternState.Planted));
            Assert.That(motion.Position.x, Is.EqualTo(3f).Within(Epsilon));
        }

        [Test]
        public void Throw_WhenNotHeld_OrWithoutDirection_IsIgnored()
        {
            Assert.That(motion.TryThrow(Vector2.zero, Vector2.zero), Is.False);

            motion.TryThrow(Vector2.zero, Vector2.right);
            Assert.That(motion.TryThrow(Vector2.zero, Vector2.left), Is.False);
        }

        [Test]
        public void Recall_WhileHeld_IsIgnored()
        {
            Assert.That(motion.TryRecall(), Is.False);
            Assert.That(motion.State, Is.EqualTo(LanternState.Held));
        }

        [Test]
        public void Recall_FromPlanted_ReturnsToHand()
        {
            motion.TryThrow(Vector2.zero, Vector2.right);
            StepWhile(LanternState.Thrown, Vector2.zero, NoObstacle);

            Assert.That(motion.TryRecall(), Is.True);
            StepWhile(LanternState.Returning, Vector2.zero, NoObstacle);

            Assert.That(motion.State, Is.EqualTo(LanternState.Held));
            Assert.That(motion.Position, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Recall_InFlight_TurnsBack()
        {
            motion.TryThrow(Vector2.zero, Vector2.right);
            motion.Step(Dt, Vector2.zero, NoObstacle);

            Assert.That(motion.TryRecall(), Is.True);
            Assert.That(motion.State, Is.EqualTo(LanternState.Returning));
        }

        [Test]
        public void SnapToHeld_FromPlanted_IsHeldAtHandImmediately()
        {
            motion.TryThrow(Vector2.zero, Vector2.right);
            StepWhile(LanternState.Thrown, Vector2.zero, NoObstacle);

            motion.SnapToHeld(new Vector2(-50f, 2f));

            Assert.That(motion.State, Is.EqualTo(LanternState.Held));
            Assert.That(motion.Position, Is.EqualTo(new Vector2(-50f, 2f)));
        }

        [Test]
        public void Returning_CatchesUpWithMovingHolder()
        {
            const float holderSpeed = 8.5f;
            motion.TryThrow(Vector2.zero, Vector2.left);
            StepWhile(LanternState.Thrown, Vector2.zero, NoObstacle);
            motion.TryRecall();

            Vector2 holder = Vector2.zero;
            for (int i = 0; i < MaxSteps && motion.State == LanternState.Returning; i++)
            {
                holder.x += holderSpeed * Dt;
                motion.Step(Dt, holder, NoObstacle);
            }

            Assert.That(motion.State, Is.EqualTo(LanternState.Held));
            Assert.That(motion.Position, Is.EqualTo(holder));
        }
    }
}
