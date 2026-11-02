using System;
using UnityEngine;

namespace Lampkeeper.Visual
{
    public enum PlayerPose { Idle, Run, Jump, Fall }

    [Serializable]
    public sealed class SpriteSequence
    {
        public Sprite[] frames = Array.Empty<Sprite>();
        [Min(0.1f)] public float framesPerSecond = 8f;
        public bool loop = true;
    }

    [CreateAssetMenu(fileName = "PlayerAnimationSettings", menuName = "Lampkeeper/Visual/Player Animation")]
    public sealed class PlayerAnimationSettings : ScriptableObject
    {
        public SpriteSequence idle = new SpriteSequence { framesPerSecond = 4f };
        public SpriteSequence run = new SpriteSequence { framesPerSecond = 10f };
        public SpriteSequence jump = new SpriteSequence { framesPerSecond = 8f, loop = false };
        public SpriteSequence fall = new SpriteSequence { framesPerSecond = 6f };
        [Min(0f)] public float movementThreshold = 0.15f;
        [Tooltip("시트 셀 전체의 월드 높이. 모든 프레임에 동일하게 적용한다.")]
        [Min(0.1f)] public float cellHeight = 2.15f;
        public Vector2 visualOffset = new Vector2(0f, -0.7f);

        public SpriteSequence GetSequence(PlayerPose pose)
        {
            switch (pose)
            {
                case PlayerPose.Run: return run;
                case PlayerPose.Jump: return jump;
                case PlayerPose.Fall: return fall;
                default: return idle;
            }
        }
    }
}
