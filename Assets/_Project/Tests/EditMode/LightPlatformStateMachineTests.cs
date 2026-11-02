using Lampkeeper.Light;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class LightPlatformStateMachineTests
    {
        const float Dt = 0.02f;
        const int ManySteps = 100;

        LightPlatformSettings settings;
        LightPlatformStateMachine machine;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<LightPlatformSettings>();
            settings.dissolveTime = 0.2f;
            settings.formTime = 0.15f;
            machine = new LightPlatformStateMachine(settings);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        static int StepsWithin(float duration) => Mathf.FloorToInt(duration * 0.5f / Dt);

        static int StepsBeyond(float duration) => Mathf.CeilToInt(duration / Dt) + 1;

        void StepMany(int steps, bool lit, bool occupied = false)
        {
            for (int i = 0; i < steps; i++)
                machine.Step(lit, occupied, Dt);
        }

        [Test]
        public void Snap_MatchesLightImmediately()
        {
            machine.Snap(true);
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Solid));
            Assert.That(machine.CollisionEnabled, Is.True);

            machine.Snap(false);
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Gone));
            Assert.That(machine.CollisionEnabled, Is.False);
        }

        [Test]
        public void LosingLight_StaysSolidUnderfootDuringDissolve()
        {
            machine.Snap(true);

            StepMany(StepsWithin(settings.dissolveTime), lit: false);

            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Dissolving));
            Assert.That(machine.CollisionEnabled, Is.True);
        }

        [Test]
        public void LosingLight_IsGoneAfterDissolveTime()
        {
            machine.Snap(true);

            StepMany(StepsBeyond(settings.dissolveTime), lit: false);

            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Gone));
            Assert.That(machine.CollisionEnabled, Is.False);
            Assert.That(machine.Opacity, Is.EqualTo(0f));
        }

        [Test]
        public void RelitWhileDissolving_RecoversWithoutDroppingCollision()
        {
            machine.Snap(true);
            StepMany(StepsWithin(settings.dissolveTime), lit: false);

            for (int i = 0; i < ManySteps; i++)
            {
                machine.Step(true, false, Dt);
                Assert.That(machine.CollisionEnabled, Is.True);
            }

            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Solid));
            Assert.That(machine.Opacity, Is.EqualTo(1f));
        }

        [Test]
        public void GonePlatform_Lit_CollidesAtOnce_AndIsSolidAfterFormTime()
        {
            machine.Snap(false);

            machine.Step(true, false, Dt);
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Forming));
            Assert.That(machine.CollisionEnabled, Is.True);

            StepMany(StepsBeyond(settings.formTime), lit: true);
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Solid));
        }

        [Test]
        public void GonePlatform_LitWhileOccupied_WaitsUntilEmpty()
        {
            machine.Snap(false);

            StepMany(ManySteps, lit: true, occupied: true);
            Assert.That(machine.CollisionEnabled, Is.False, "겹친 몸이 있으면 충돌을 켜지 않는다");
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Forming));

            machine.Step(true, false, Dt);
            Assert.That(machine.CollisionEnabled, Is.True);
            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Solid));
        }

        [Test]
        public void ZeroDissolveTime_DisappearsInOneStep()
        {
            settings.dissolveTime = 0f;
            machine.Snap(true);

            machine.Step(false, false, Dt);

            Assert.That(machine.State, Is.EqualTo(LightPlatformState.Gone));
        }
    }
}
