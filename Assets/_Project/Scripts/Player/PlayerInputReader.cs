using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lampkeeper.Player
{
    /// <summary>
    /// 플레이어 입력. 기본 바인딩(키보드 + 게임패드)을 코드에 정의해서 에셋 설정 없이 바로 동작한다.
    /// 키 리바인딩이 필요해지면 .inputactions 에셋으로 옮긴다.
    /// </summary>
    public sealed class PlayerInputReader : IDisposable
    {
        // mode=1(Digital): 대각선 입력에서도 좌우 값이 정규화로 줄어들지 않게 한다.
        const string DigitalVector2Composite = "2DVector(mode=1)";

        readonly InputAction move;
        readonly InputAction jump;
        readonly InputAction lantern;
        readonly InputAction dash;

        public PlayerInputReader()
        {
            move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            move.AddCompositeBinding(DigitalVector2Composite)
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding(DigitalVector2Composite)
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");

            jump = new InputAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Keyboard>/z");
            jump.AddBinding("<Gamepad>/buttonSouth");

            // 던지기와 회수를 한 버튼으로 한다 (들고 있으면 던지고, 손을 떠나 있으면 부른다).
            lantern = new InputAction("Lantern", InputActionType.Button);
            lantern.AddBinding("<Keyboard>/x");
            lantern.AddBinding("<Keyboard>/j");
            lantern.AddBinding("<Gamepad>/buttonWest");

            dash = new InputAction("Dash", InputActionType.Button);
            dash.AddBinding("<Keyboard>/leftShift");
            dash.AddBinding("<Keyboard>/rightShift");
            dash.AddBinding("<Gamepad>/rightShoulder");
        }

        /// <summary>이동·조준 입력. x는 좌우 이동, 전체 벡터는 등불 조준에 쓴다.</summary>
        public Vector2 Move => move.ReadValue<Vector2>();

        public bool JumpPressedThisFrame => jump.WasPressedThisFrame();

        public bool JumpHeld => jump.IsPressed();

        public bool LanternPressedThisFrame => lantern.WasPressedThisFrame();
        public bool DashPressedThisFrame => dash.WasPressedThisFrame();

        public void Enable()
        {
            move.Enable();
            jump.Enable();
            lantern.Enable();
            dash.Enable();
        }

        public void Disable()
        {
            move.Disable();
            jump.Disable();
            lantern.Disable();
            dash.Disable();
        }

        public void Dispose()
        {
            move.Dispose();
            jump.Dispose();
            lantern.Dispose();
            dash.Dispose();
        }
    }
}
