using System;
using UnityEngine;

namespace Lampkeeper.Light
{
    public enum LanternState
    {
        Held,
        Thrown,
        Planted,
        Returning,
    }

    /// <summary>
    /// 날아가는 등불의 앞을 검사한다. distance 안에서 무언가에 닿으면 참과 닿기까지의 거리를 돌려준다.
    /// </summary>
    public delegate bool LanternCast(Vector2 origin, Vector2 direction, float distance, out float hitDistance);

    /// <summary>
    /// 등불의 상태 머신과 움직임 순수 로직: 들고 있음 → 날아감 → 고정됨 → 회수 중 → 들고 있음.
    /// 충돌 검사는 <see cref="LanternCast"/>로 받아서 물리 엔진 없이 테스트한다.
    /// </summary>
    public sealed class LanternMotion
    {
        readonly LanternSettings settings;

        Vector2 throwDirection;
        float traveled;

        public LanternMotion(LanternSettings settings)
        {
            this.settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public LanternState State { get; private set; } = LanternState.Held;

        public Vector2 Position { get; private set; }

        public bool TryThrow(Vector2 origin, Vector2 direction)
        {
            if (State != LanternState.Held || direction == Vector2.zero)
                return false;

            State = LanternState.Thrown;
            Position = origin;
            throwDirection = direction.normalized;
            traveled = 0f;
            return true;
        }

        public bool TryRecall()
        {
            if (State != LanternState.Thrown && State != LanternState.Planted)
                return false;

            State = LanternState.Returning;
            return true;
        }

        /// <summary>날아오는 과정 없이 바로 손에 쥔다. 순간이동·리스폰용.</summary>
        public void SnapToHeld(Vector2 holdPosition)
        {
            State = LanternState.Held;
            Position = holdPosition;
        }

        public void Step(float deltaTime, Vector2 holdPosition, LanternCast cast)
        {
            switch (State)
            {
                case LanternState.Held:
                    Position = holdPosition;
                    break;
                case LanternState.Thrown:
                    StepThrown(deltaTime, cast);
                    break;
                case LanternState.Returning:
                    StepReturning(deltaTime, holdPosition);
                    break;
            }
        }

        void StepThrown(float deltaTime, LanternCast cast)
        {
            float stepLength = Mathf.Min(settings.throwSpeed * deltaTime, settings.throwDistance - traveled);
            if (cast(Position, throwDirection, stepLength, out float hitDistance))
            {
                Position += throwDirection * hitDistance;
                State = LanternState.Planted;
                return;
            }

            Position += throwDirection * stepLength;
            traveled += stepLength;
            if (traveled >= settings.throwDistance)
                State = LanternState.Planted;
        }

        void StepReturning(float deltaTime, Vector2 holdPosition)
        {
            Position = Vector2.MoveTowards(Position, holdPosition, settings.returnSpeed * deltaTime);
            if ((holdPosition - Position).sqrMagnitude <= settings.catchDistance * settings.catchDistance)
            {
                Position = holdPosition;
                State = LanternState.Held;
            }
        }
    }
}
