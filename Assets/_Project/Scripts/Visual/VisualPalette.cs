using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>
    /// 화면 전체의 색. 원칙: 대부분은 차갑고 어둡게, 등불 주변만 따뜻하게.
    /// 비주얼 컴포넌트들이 매 프레임 읽으므로 플레이 중 바꾸면 바로 반영된다.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualPalette", menuName = "Lampkeeper/Visual/Palette")]
    public sealed class VisualPalette : ScriptableObject
    {
        [Header("어둠·배경")]
        [Tooltip("카메라 배경. 아무것도 없는 곳의 색.")]
        public Color voidColor = new Color32(0x0B, 0x0B, 0x14, 0xFF);

        public Color deepColor = new Color32(0x14, 0x14, 0x28, 0xFF);

        public Color duskColor = new Color32(0x1E, 0x1E, 0x3A, 0xFF);

        [Header("실루엣")]
        [Tooltip("원경 폐허 실루엣 (채도 낮은 청회색)")]
        public Color farSilhouetteColor = new Color32(0x3A, 0x40, 0x56, 0xFF);

        [Tooltip("중경 실루엣")]
        public Color midSilhouetteColor = new Color32(0x26, 0x2A, 0x3E, 0xFF);

        [Header("지형")]
        [Tooltip("지형 바위 몸통. 스스로의 그림자로 거의 실루엣이 된다.")]
        public Color terrainColor = new Color32(0x6E, 0x72, 0x8C, 0xFF);

        [Tooltip("지형 윗면 띠. 등불 빛을 받아 밝게 드러나는 부분.")]
        public Color terrainSurfaceColor = new Color32(0xB4, 0xB8, 0xCC, 0xFF);

        [Tooltip("빛 발판 (기억으로 이루어진 땅)")]
        public Color memoryPlatformColor = new Color32(0xB8, 0xD0, 0xFF, 0xFF);

        [Header("빛")]
        [Tooltip("전역 빛. 어둠 속 실루엣이 희미하게 보이게 하는 차가운 색.")]
        public Color ambientLightColor = new Color32(0x6A, 0x78, 0xB8, 0xFF);

        [Tooltip("등불의 밝은 핵")]
        public Color lanternCoreColor = new Color32(0xFF, 0xB3, 0x47, 0xFF);

        [Tooltip("등불의 은은한 번짐")]
        public Color lanternHaloColor = new Color32(0xFF, 0x8C, 0x42, 0xFF);

        [Tooltip("불씨 강조색")]
        public Color emberColor = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
    }
}
