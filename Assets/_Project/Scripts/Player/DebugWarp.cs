using UnityEngine;
using UnityEngine.InputSystem;

namespace Lampkeeper.Player
{
    /// <summary>
    /// 플레이 테스트용: 숫자 키(1~9)로 플레이어를 지정한 지점으로 순간이동시킨다.
    /// 에디터와 개발 빌드에서만 동작한다.
    /// </summary>
    public sealed class DebugWarp : MonoBehaviour
    {
        const int MaxWarpKeys = 9;
        const float HintMargin = 10f;
        const float HintLineHeight = 20f;
        const float HintWidth = 360f;

        [SerializeField] PlayerController player;

        [Tooltip("숫자 키 순서대로의 이동 지점. 오브젝트 이름이 화면 안내에 표시된다.")]
        [SerializeField] Transform[] warpPoints = new Transform[0];

        [Tooltip("화면 왼쪽 위에 단축키 안내를 표시한다")]
        [SerializeField] bool showHint = true;

        int WarpCount => Mathf.Min(warpPoints.Length, MaxWarpKeys);

        void Awake()
        {
            if (!Debug.isDebugBuild || player == null)
                enabled = false;
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            for (int i = 0; i < WarpCount; i++)
            {
                if (warpPoints[i] != null && keyboard[Key.Digit1 + i].wasPressedThisFrame)
                    player.Teleport(warpPoints[i].position);
            }
        }

        void OnGUI()
        {
            if (!showHint)
                return;

            for (int i = 0; i < WarpCount; i++)
            {
                if (warpPoints[i] == null)
                    continue;
                var line = new Rect(HintMargin, HintMargin + i * HintLineHeight, HintWidth, HintLineHeight);
                GUI.Label(line, $"{i + 1}: {warpPoints[i].name}");
            }
        }
    }
}
