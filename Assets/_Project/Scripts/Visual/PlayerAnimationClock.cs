using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>물리 상태로 포즈를 선택하고 프레임 시간을 관리한다.</summary>
    public sealed class PlayerAnimationClock
    {
        public PlayerPose Pose { get; private set; }
        public float Elapsed { get; private set; }

        public void Reset() { Pose = PlayerPose.Idle; Elapsed = 0f; }

        public void Step(bool grounded, Vector2 velocity, float threshold, float deltaTime)
        {
            PlayerPose next = !grounded ? (velocity.y > 0f ? PlayerPose.Jump : PlayerPose.Fall)
                : Mathf.Abs(velocity.x) > threshold ? PlayerPose.Run : PlayerPose.Idle;
            if (next != Pose) { Pose = next; Elapsed = 0f; }
            else Elapsed += Mathf.Max(0f, deltaTime);
        }

        public int Frame(int count, float fps, bool loop)
        {
            if (count <= 0) return -1;
            int frame = Mathf.FloorToInt(Elapsed * Mathf.Max(0f, fps));
            return loop ? frame % count : Mathf.Min(frame, count - 1);
        }
    }
}
