using Lampkeeper.Visual;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class PlayerAnimationClockTests
    {
        [TestCase(true, 0f, 0f, PlayerPose.Idle)]
        [TestCase(true, 5f, 0f, PlayerPose.Run)]
        [TestCase(true, -5f, 0f, PlayerPose.Run)]
        [TestCase(false, 5f, 2f, PlayerPose.Jump)]
        [TestCase(false, 0f, 0f, PlayerPose.Fall)]
        [TestCase(false, 0f, -3f, PlayerPose.Fall)]
        public void SelectsPoseFromPhysics(bool grounded, float x, float y, PlayerPose pose)
        {
            var clock = new PlayerAnimationClock();
            clock.Step(grounded, new Vector2(x, y), 0.15f, 0.02f);
            Assert.That(clock.Pose, Is.EqualTo(pose));
        }
        [Test] public void StateChangeRestartsFramesAndLandingStopsFall()
        {
            var clock = new PlayerAnimationClock();
            clock.Step(true, Vector2.zero, 0.15f, 1f);
            clock.Step(false, Vector2.down, 0.15f, 0.02f);
            Assert.That(clock.Frame(4, 10f, true), Is.Zero);
            clock.Step(true, Vector2.zero, 0.15f, 0.02f);
            Assert.That(clock.Pose, Is.EqualTo(PlayerPose.Idle));
            Assert.That(clock.Elapsed, Is.Zero);
        }
        [Test] public void LoopWrapsAndNonLoopHoldsLastFrame()
        {
            var clock = new PlayerAnimationClock();
            clock.Step(true, Vector2.zero, 0.15f, 0.55f);
            Assert.That(clock.Frame(4, 10f, true), Is.EqualTo(1));
            Assert.That(clock.Frame(4, 10f, false), Is.EqualTo(3));
            Assert.That(clock.Frame(0, 10f, true), Is.EqualTo(-1));
        }
    }
}
