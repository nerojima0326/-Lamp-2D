using Lampkeeper.Light;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class LightFieldTests
    {
        const float Epsilon = 1e-4f;

        sealed class FakeEmitter : ILightEmitter
        {
            public FakeEmitter(Vector2 center, float radius, float intensity = 1f)
            {
                Circle = new LightCircle(center, radius, intensity);
            }

            public LightCircle Circle { get; set; }
        }

        static LightField FieldWith(params ILightEmitter[] emitters)
        {
            var field = new LightField();
            foreach (ILightEmitter emitter in emitters)
                field.Register(emitter);
            return field;
        }

        [Test]
        public void Point_InsideOrOnEdge_IsLit_OutsideIsNot()
        {
            LightField field = FieldWith(new FakeEmitter(Vector2.zero, 3f));

            Assert.That(field.IsLit(new Vector2(1f, 1f)), Is.True);
            Assert.That(field.IsLit(new Vector2(3f, 0f)), Is.True, "반경 경계는 밝은 것으로 본다");
            Assert.That(field.IsLit(new Vector2(3.01f, 0f)), Is.False);
        }

        [Test]
        public void ZeroRadius_LightsNothing()
        {
            LightField field = FieldWith(new FakeEmitter(Vector2.zero, 0f));

            Assert.That(field.IsLit(Vector2.zero), Is.False);
        }

        [Test]
        public void Area_PartlyInsideLight_IsLit()
        {
            LightField field = FieldWith(new FakeEmitter(Vector2.zero, 3f));
            var area = new Rect(2f, -1f, 4f, 2f); // 중심은 빛 밖, 왼쪽 끝만 빛 안

            Assert.That(field.IsLit(area), Is.True);
        }

        [Test]
        public void Area_InsideBoundingBoxButOutsideCircle_IsNotLit()
        {
            LightField field = FieldWith(new FakeEmitter(Vector2.zero, 1f));
            var area = new Rect(0.8f, 0.8f, 1f, 1f); // 가장 가까운 모서리까지 거리 ≈ 1.13

            Assert.That(field.IsLit(area), Is.False);
        }

        [Test]
        public void Exposure_FallsOffLinearly_AndIsZeroOutside()
        {
            LightField field = FieldWith(new FakeEmitter(Vector2.zero, 4f, intensity: 2f));

            Assert.That(field.GetExposure(Vector2.zero), Is.EqualTo(2f).Within(Epsilon));
            Assert.That(field.GetExposure(new Vector2(2f, 0f)), Is.EqualTo(1f).Within(Epsilon));
            Assert.That(field.GetExposure(new Vector2(5f, 0f)), Is.EqualTo(0f));
        }

        [Test]
        public void Exposure_UsesStrongestLight()
        {
            LightField field = FieldWith(
                new FakeEmitter(Vector2.zero, 4f, intensity: 1f),
                new FakeEmitter(new Vector2(1f, 0f), 2f, intensity: 3f));

            // 첫 광원: 1 × (1 - 1/4) = 0.75, 두 번째: 3 × (1 - 0/2) = 3
            Assert.That(field.GetExposure(new Vector2(1f, 0f)), Is.EqualTo(3f).Within(Epsilon));
        }

        [Test]
        public void Register_Twice_CountsOnce_AndUnregisterRemoves()
        {
            var emitter = new FakeEmitter(Vector2.zero, 3f);
            var field = new LightField();

            field.Register(emitter);
            field.Register(emitter);
            Assert.That(field.Count, Is.EqualTo(1));

            field.Unregister(emitter);
            Assert.That(field.IsLit(Vector2.zero), Is.False);
        }

        [Test]
        public void MovingEmitter_IsQueriedAtCurrentPosition()
        {
            var emitter = new FakeEmitter(Vector2.zero, 1f);
            LightField field = FieldWith(emitter);

            emitter.Circle = new LightCircle(new Vector2(10f, 0f), 1f, 1f);

            Assert.That(field.IsLit(Vector2.zero), Is.False);
            Assert.That(field.IsLit(new Vector2(10f, 0f)), Is.True);
        }
    }
}
