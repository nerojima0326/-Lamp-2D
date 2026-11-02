using System;
using UnityEngine;

namespace Lampkeeper.Light
{
    public enum LightPlatformState
    {
        Solid,
        Dissolving,
        Gone,
        Forming,
    }

    /// <summary>
    /// 빛 발판의 상태 전환 순수 로직. 빛을 받는지와 안에 몸이 겹쳐 있는지만 입력으로 받는다.
    /// Opacity는 연속 값이라 사라지는 중에 다시 빛을 받으면 그 자리에서 되살아난다.
    /// </summary>
    public sealed class LightPlatformStateMachine
    {
        readonly LightPlatformSettings settings;

        public LightPlatformStateMachine(LightPlatformSettings settings)
        {
            this.settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public LightPlatformState State { get; private set; } = LightPlatformState.Gone;

        /// <summary>0 = 완전히 사라짐, 1 = 완전히 실체화.</summary>
        public float Opacity { get; private set; }

        public bool CollisionEnabled { get; private set; }

        /// <summary>전환 연출 없이 현재 빛 상태로 즉시 맞춘다. 씬 시작 시 한 번 호출한다.</summary>
        public void Snap(bool lit)
        {
            State = lit ? LightPlatformState.Solid : LightPlatformState.Gone;
            Opacity = lit ? 1f : 0f;
            CollisionEnabled = lit;
        }

        public void Step(bool lit, bool occupied, float deltaTime)
        {
            if (lit)
                StepTowardSolid(occupied, deltaTime);
            else
                StepTowardGone(deltaTime);
        }

        void StepTowardSolid(bool occupied, float deltaTime)
        {
            // 안에 겹친 몸을 밀어내지 않도록, 비어 있을 때만 충돌을 켠다.
            if (!CollisionEnabled && !occupied)
                CollisionEnabled = true;

            Opacity = Advance(Opacity, 1f, settings.formTime, deltaTime);
            State = Opacity >= 1f && CollisionEnabled ? LightPlatformState.Solid : LightPlatformState.Forming;
        }

        void StepTowardGone(float deltaTime)
        {
            Opacity = Advance(Opacity, 0f, settings.dissolveTime, deltaTime);
            if (Opacity <= 0f)
            {
                State = LightPlatformState.Gone;
                CollisionEnabled = false;
            }
            else
            {
                State = LightPlatformState.Dissolving;
            }
        }

        static float Advance(float current, float target, float duration, float deltaTime)
            => duration <= 0f ? target : Mathf.MoveTowards(current, target, deltaTime / duration);
    }
}
