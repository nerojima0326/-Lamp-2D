using Lampkeeper.Visual;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class EncounterZoomTests
    {
        EncounterCameraSettings settings;
        EncounterZoom zoom;
        [SetUp] public void Setup()
        {
            settings = ScriptableObject.CreateInstance<EncounterCameraSettings>();
            zoom = new EncounterZoom();
            zoom.Reset(settings.soloSize);
        }
        [TearDown] public void Cleanup() => Object.DestroyImmediate(settings);

        [Test] public void SoloStaysClose()
        {
            Assert.That(zoom.Step(false, 1f, settings), Is.EqualTo(settings.soloSize));
        }
        [Test] public void EnemySmoothlyWidensWithoutJumping()
        {
            float first = zoom.Step(true, 0.02f, settings);
            Assert.That(first, Is.GreaterThan(settings.soloSize).And.LessThan(settings.encounterSize));
            for (int i = 0; i < 300; i++) zoom.Step(true, 0.02f, settings);
            Assert.That(zoom.Size, Is.EqualTo(settings.encounterSize).Within(0.01f));
        }
        [Test] public void ExitDelayThenReturnsToSolo()
        {
            zoom.Step(true, 0.1f, settings);
            zoom.Step(false, settings.returnDelay * 0.5f, settings);
            Assert.That(zoom.InEncounter, Is.True);
            zoom.Step(false, settings.returnDelay, settings);
            Assert.That(zoom.InEncounter, Is.False);
            for (int i = 0; i < 500; i++) zoom.Step(false, 0.02f, settings);
            Assert.That(zoom.Size, Is.EqualTo(settings.soloSize).Within(0.01f));
        }
        [Test] public void ReturningEnemyRestartsDelay()
        {
            zoom.Step(true, 0.1f, settings);
            zoom.Step(false, settings.returnDelay * 0.75f, settings);
            zoom.Step(true, 0.1f, settings);
            zoom.Step(false, settings.returnDelay * 0.75f, settings);
            Assert.That(zoom.InEncounter, Is.True);
        }
        [Test] public void DetectionUsesBoundsAndAspectAndRejectsVerticalOffscreen()
        {
            Assert.That(EncounterZoom.Overlaps(Vector2.zero, 4f, 2f, 0f,
                new Bounds(new Vector3(8.5f, 0f), Vector3.one * 2f)), Is.True);
            Assert.That(EncounterZoom.Overlaps(Vector2.zero, 4f, 1f, 0f,
                new Bounds(new Vector3(8.5f, 0f), Vector3.one * 2f)), Is.False);
            Assert.That(EncounterZoom.Overlaps(Vector2.zero, 4f, 2f, 0f,
                new Bounds(new Vector3(0f, 6f), Vector3.one)), Is.False);
        }
        [Test] public void ResetClearsEncounter()
        {
            zoom.Step(true, 1f, settings);
            zoom.Reset(settings.soloSize);
            Assert.That(zoom.InEncounter, Is.False);
            Assert.That(zoom.Size, Is.EqualTo(settings.soloSize));
        }
    }
}
