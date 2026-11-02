using Lampkeeper.Light;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class LanternAimTests
    {
        const float Deadzone = 0.35f;
        const float Epsilon = 1e-4f;

        static void AssertDirection(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Epsilon));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Epsilon));
        }

        [Test]
        public void NoInput_ThrowsTowardFacing()
        {
            AssertDirection(LanternAim.Resolve(Vector2.zero, 1, Deadzone), Vector2.right);
            AssertDirection(LanternAim.Resolve(Vector2.zero, -1, Deadzone), Vector2.left);
        }

        [Test]
        public void InputBelowDeadzone_ThrowsTowardFacing()
        {
            AssertDirection(LanternAim.Resolve(new Vector2(0f, 0.2f), -1, Deadzone), Vector2.left);
        }

        [Test]
        public void Up_ThrowsStraightUp()
        {
            AssertDirection(LanternAim.Resolve(Vector2.up, 1, Deadzone), Vector2.up);
        }

        [Test]
        public void KeyboardDiagonal_IsUnitDiagonal()
        {
            AssertDirection(LanternAim.Resolve(new Vector2(1f, 1f), 1, Deadzone), new Vector2(1f, 1f).normalized);
        }

        [Test]
        public void AnalogInput_SnapsToNearestOfEightDirections()
        {
            AssertDirection(LanternAim.Resolve(new Vector2(0.9f, 0.3f), 1, Deadzone), Vector2.right);
            AssertDirection(LanternAim.Resolve(new Vector2(-0.5f, 0.6f), 1, Deadzone), new Vector2(-1f, 1f).normalized);
        }
    }
}
