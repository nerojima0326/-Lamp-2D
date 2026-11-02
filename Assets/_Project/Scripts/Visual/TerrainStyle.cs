using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>지형 겉모습 튜닝 값. 바뀐 값은 Tools/Lampkeeper/Apply Visual Pass를 다시 실행하면 반영된다.</summary>
    [CreateAssetMenu(fileName = "TerrainStyle", menuName = "Lampkeeper/Visual/Terrain Style")]
    public sealed class TerrainStyle : ScriptableObject
    {
        [Tooltip("블록 윗면 바로 위에 얹는 표면 띠의 두께 (유닛). 바위 몸통은 스스로의 그림자로 어둡고, 이 띠가 빛을 받아 윗면 형태를 드러낸다.")]
        [Min(0.01f)] public float surfaceThickness = 0.14f;

        [Tooltip("이보다 좁게 드러난 윗면 조각에는 띠를 놓지 않는다 (유닛)")]
        [Min(0f)] public float minSurfaceWidth = 0.2f;

        [Tooltip("표면 띠의 정렬 순서. 플레이어보다 높으면 발이 살짝 묻혀 땅에 붙어 보인다.")]
        public int surfaceSortingOrder = 15;

        [Header("시작 구간 질감 미리보기")]
        [Tooltip("이 X 좌표보다 왼쪽에서 시작하는 지형에 돌 질감을 적용한다.")]
        public float texturePreviewEndX = 30f;
        [Tooltip("돌 무늬의 색과 불투명도")]
        public Color textureColor = new Color(0.65f, 0.7f, 0.8f, 0.32f);
    }
}
