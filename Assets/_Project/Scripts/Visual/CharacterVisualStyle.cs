using UnityEngine;

namespace Lampkeeper.Visual
{
    [CreateAssetMenu(fileName = "CharacterVisualStyle", menuName = "Lampkeeper/Visual/Character Style")]
    public sealed class CharacterVisualStyle : ScriptableObject
    {
        [Min(0.01f)] public float playerHeight = 1.7f;
        public Vector2 playerOffset = new Vector2(0f, 0.15f);
        [Min(0.01f)] public float lanternHeight = 0.65f;
    }
}
