using System.Collections.Generic;
using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>빛을 내는 것. LightField에 등록되어 조회 대상이 된다.</summary>
    public interface ILightEmitter
    {
        LightCircle Circle { get; }
    }

    /// <summary>
    /// "여기가 밝은가"를 답하는 조회 서비스. 게임 전체는 <see cref="Main"/>을 공유하고,
    /// 테스트는 별도 인스턴스를 만든다. 광원이 많아져 느려지면 여기에 공간 분할을 넣는다.
    /// </summary>
    public sealed class LightField
    {
        readonly List<ILightEmitter> emitters = new List<ILightEmitter>();

        public static LightField Main { get; } = new LightField();

        public int Count => emitters.Count;

        public void Register(ILightEmitter emitter)
        {
            if (!emitters.Contains(emitter))
                emitters.Add(emitter);
        }

        public void Unregister(ILightEmitter emitter) => emitters.Remove(emitter);

        public void Clear() => emitters.Clear();

        public bool IsLit(Vector2 point)
        {
            for (int i = 0; i < emitters.Count; i++)
            {
                if (emitters[i].Circle.Contains(point))
                    return true;
            }
            return false;
        }

        /// <summary>영역의 일부라도 어떤 빛 안에 있으면 참.</summary>
        public bool IsLit(Rect area)
        {
            for (int i = 0; i < emitters.Count; i++)
            {
                if (emitters[i].Circle.Overlaps(area))
                    return true;
            }
            return false;
        }

        /// <summary>가장 강하게 비추는 광원 기준의 노출도 (0 = 어둠).</summary>
        public float GetExposure(Vector2 point)
        {
            float exposure = 0f;
            for (int i = 0; i < emitters.Count; i++)
                exposure = Mathf.Max(exposure, emitters[i].Circle.ExposureAt(point));
            return exposure;
        }

        // 도메인 리로드 없이 플레이 모드에 들어가도 이전 세션의 광원이 남지 않게 한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetMain() => Main.Clear();
    }
}
