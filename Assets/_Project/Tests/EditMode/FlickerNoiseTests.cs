using Lampkeeper.Visual;
using NUnit.Framework;

namespace Lampkeeper.Tests
{
    public class FlickerNoiseTests
    {
        const float Seed = 3.7f;
        const int Samples = 2000;
        const float TimeStep = 0.01f;
        // 프레임 간 시간 간격에서 허용하는 최대 변화량 (펄린 노이즈의 기울기 상한보다 넉넉하게)
        const float MaxChangePerStep = 0.1f;

        [Test]
        public void Sample_StaysWithinMinusOneToOne()
        {
            for (int i = 0; i < Samples; i++)
            {
                float value = FlickerNoise.Sample(Seed, i * TimeStep);
                Assert.That(value, Is.InRange(-1f, 1f));
            }
        }

        [Test]
        public void Sample_IsDeterministic()
        {
            Assert.That(FlickerNoise.Sample(Seed, 1.23f), Is.EqualTo(FlickerNoise.Sample(Seed, 1.23f)));
        }

        [Test]
        public void Sample_ChangesSmoothlyOverTime()
        {
            float previous = FlickerNoise.Sample(Seed, 0f);
            for (int i = 1; i < Samples; i++)
            {
                float current = FlickerNoise.Sample(Seed, i * TimeStep);
                Assert.That(System.Math.Abs(current - previous), Is.LessThan(MaxChangePerStep));
                previous = current;
            }
        }

        [Test]
        public void DifferentSeeds_GiveDifferentPatterns()
        {
            bool anyDifferent = false;
            for (int i = 0; i < Samples && !anyDifferent; i++)
                anyDifferent = FlickerNoise.Sample(Seed, i * TimeStep) != FlickerNoise.Sample(Seed + 10.5f, i * TimeStep);

            Assert.That(anyDifferent, Is.True);
        }
    }
}
