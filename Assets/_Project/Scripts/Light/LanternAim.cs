using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>이동 입력을 등불 던지기 방향으로 바꾼다. 결과는 8방향 단위 벡터.</summary>
    public static class LanternAim
    {
        const float DirectionStepDegrees = 45f;

        public static Vector2 Resolve(Vector2 input, int facing, float deadzone)
        {
            if (input.magnitude < deadzone || input == Vector2.zero)
                return new Vector2(facing < 0 ? -1f : 1f, 0f);

            // 퍼즐에서 예측할 수 있도록 아날로그 입력도 8방향으로 맞춘다.
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            float snapped = Mathf.Round(angle / DirectionStepDegrees) * DirectionStepDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
        }
    }
}
