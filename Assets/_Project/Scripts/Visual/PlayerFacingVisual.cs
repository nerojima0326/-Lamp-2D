using Lampkeeper.Player;
using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>이동 로직을 읽어 캐릭터 스프라이트의 방향만 바꾼다.</summary>
    public sealed class PlayerFacingVisual : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] SpriteRenderer spriteRenderer;

        void LateUpdate()
        {
            if (player != null && player.Motor != null && spriteRenderer != null)
                spriteRenderer.flipX = player.Motor.Facing < 0;
        }
    }
}
