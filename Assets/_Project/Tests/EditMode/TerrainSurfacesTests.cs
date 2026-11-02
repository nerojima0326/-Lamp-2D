using System.Collections.Generic;
using Lampkeeper.Visual;
using NUnit.Framework;
using UnityEngine;

namespace Lampkeeper.Tests
{
    public class TerrainSurfacesTests
    {
        const float Thickness = 0.15f;
        const float MinWidth = 0.2f;
        const float Epsilon = 1e-4f;

        static readonly Rect Floor = new Rect(0f, -2f, 10f, 2f); // 윗면 y = 0, x 0 ~ 10

        static List<Vector2> Spans(params Rect[] others)
        {
            var terrain = new List<Rect> { Floor };
            terrain.AddRange(others);
            return TerrainSurfaces.ExposedTopSpans(Floor, terrain, Thickness, MinWidth);
        }

        static void AssertSpan(Vector2 span, float xMin, float xMax)
        {
            Assert.That(span.x, Is.EqualTo(xMin).Within(Epsilon));
            Assert.That(span.y, Is.EqualTo(xMax).Within(Epsilon));
        }

        [Test]
        public void NothingOnTop_WholeTopIsExposed()
        {
            List<Vector2> spans = Spans();

            Assert.That(spans.Count, Is.EqualTo(1));
            AssertSpan(spans[0], 0f, 10f);
        }

        [Test]
        public void BlockStandingInMiddle_SplitsSurface()
        {
            List<Vector2> spans = Spans(new Rect(4f, 0f, 2f, 1f));

            Assert.That(spans.Count, Is.EqualTo(2));
            AssertSpan(spans[0], 0f, 4f);
            AssertSpan(spans[1], 6f, 10f);
        }

        [Test]
        public void BlockCoveringWholeTop_LeavesNothing()
        {
            Assert.That(Spans(new Rect(-1f, -1f, 12f, 5f)), Is.Empty);
        }

        [Test]
        public void NeighbourTouchingOnlyAtSide_DoesNotCover()
        {
            List<Vector2> spans = Spans(new Rect(10f, -2f, 3f, 2f));

            Assert.That(spans.Count, Is.EqualTo(1));
            AssertSpan(spans[0], 0f, 10f);
        }

        [Test]
        public void BlockBelowOrFarAbove_DoesNotCover()
        {
            List<Vector2> spans = Spans(new Rect(2f, -5f, 3f, 1f), new Rect(2f, 3f, 3f, 1f));

            Assert.That(spans.Count, Is.EqualTo(1));
        }

        [Test]
        public void SliverNarrowerThanMinimum_IsDropped()
        {
            // 왼쪽 끝 0 ~ 0.1만 남는다
            List<Vector2> spans = Spans(new Rect(0.1f, 0f, 9.9f, 1f));

            Assert.That(spans, Is.Empty);
        }
    }
}
