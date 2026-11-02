using UnityEngine;

namespace Lampkeeper.Visual
{
    [CreateAssetMenu(fileName = "DashTrailSettings", menuName = "Lampkeeper/Visual/Dash Trail")]
    public sealed class DashTrailSettings : ScriptableObject
    {
        [Min(0.01f)] public float spawnInterval = 0.025f;
        [Min(0.01f)] public float lifetime = 0.22f;
        public Color tint = new Color(0.55f, 0.75f, 1f, 0.45f);
        [Tooltip("동시에 표시할 잔상 수. 실행 시작 시 풀을 만든다.")]
        [Range(1, 32)] public int poolSize = 12;
        public int sortingOrderOffset = -1;
        public Material material;
    }
}
