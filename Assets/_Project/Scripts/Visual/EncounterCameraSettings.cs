using UnityEngine;

namespace Lampkeeper.Visual
{
    [CreateAssetMenu(fileName = "EncounterCameraSettings", menuName = "Lampkeeper/Visual/Encounter Camera")]
    public sealed class EncounterCameraSettings : ScriptableObject
    {
        [Tooltip("혼자 있을 때 화면 세로 범위의 절반")]
        [Min(0.1f)] public float soloSize = 4.5f;
        [Tooltip("적이 있을 때 화면 세로 범위의 절반")]
        [Min(0.1f)] public float encounterSize = 7f;
        [Min(0f)] public float zoomOutTime = 0.35f;
        [Min(0f)] public float zoomInTime = 0.8f;
        [Min(0f)] public float returnDelay = 1.2f;
        [Tooltip("화면 밖 적을 미리 감지할 여유 (월드 유닛)")]
        [Min(0f)] public float enterMargin = 0.5f;
        [Tooltip("전투 중 적이 화면 밖으로 나갈 때의 추가 여유")]
        [Min(0f)] public float exitMargin = 1f;
    }
}
