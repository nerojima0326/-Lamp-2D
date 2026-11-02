namespace Lampkeeper.Player
{
    /// <summary>한 물리 스텝 동안의 플레이어 의도. 입력 장치와 무관하다.</summary>
    public readonly struct MotorInput
    {
        /// <summary>좌우 이동 입력 (-1 ~ 1).</summary>
        public readonly float MoveX;

        /// <summary>점프 버튼을 누르고 있는가 (가변 점프 판정용).</summary>
        public readonly bool JumpHeld;

        public MotorInput(float moveX, bool jumpHeld)
        {
            MoveX = moveX;
            JumpHeld = jumpHeld;
        }
    }
}
