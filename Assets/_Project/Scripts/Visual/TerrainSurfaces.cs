using System.Collections.Generic;
using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>
    /// 지형 블록 윗면 중 다른 블록에 덮이지 않고 드러난 구간을 구한다.
    /// 윗면 띠(빛을 받는 지형 표면)를 이 구간에만 놓아, 위에 얹힌 블록을 가로지르지 않게 한다.
    /// </summary>
    public static class TerrainSurfaces
    {
        /// <summary>
        /// block 윗면 바로 위 두께 thickness의 띠에서, others 중 어느 것에도 덮이지 않는 x 구간들 (xMin, xMax).
        /// minWidth보다 좁은 조각은 버린다.
        /// </summary>
        public static List<Vector2> ExposedTopSpans(Rect block, IReadOnlyList<Rect> others, float thickness, float minWidth)
        {
            var spans = new List<Vector2> { new Vector2(block.xMin, block.xMax) };
            var strip = new Rect(block.xMin, block.yMax, block.width, thickness);

            for (int i = 0; i < others.Count; i++)
            {
                Rect other = others[i];
                if (other == block || !Overlaps(strip, other))
                    continue;
                spans = Subtract(spans, other.xMin, other.xMax);
            }

            spans.RemoveAll(span => span.y - span.x < minWidth);
            return spans;
        }

        // 경계만 맞닿은 경우는 덮인 것으로 보지 않는다.
        static bool Overlaps(Rect a, Rect b) => a.xMin < b.xMax && b.xMin < a.xMax && a.yMin < b.yMax && b.yMin < a.yMax;

        static List<Vector2> Subtract(List<Vector2> spans, float cutMin, float cutMax)
        {
            var result = new List<Vector2>(spans.Count + 1);
            foreach (Vector2 span in spans)
            {
                if (cutMax <= span.x || cutMin >= span.y)
                {
                    result.Add(span);
                    continue;
                }
                if (cutMin > span.x)
                    result.Add(new Vector2(span.x, cutMin));
                if (cutMax < span.y)
                    result.Add(new Vector2(cutMax, span.y));
            }
            return result;
        }
    }
}
