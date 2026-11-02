using Lampkeeper.Player;
using UnityEngine;

namespace Lampkeeper.Visual
{
    [DisallowMultipleComponent]
    public sealed class PlayerSpriteAnimation : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] Rigidbody2D body;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] PlayerAnimationSettings settings;
        readonly PlayerAnimationClock clock = new PlayerAnimationClock();

        void OnEnable() => clock.Reset();

        void LateUpdate()
        {
            if (player == null || player.Motor == null || body == null || spriteRenderer == null || settings == null) return;
            clock.Step(player.Motor.IsGrounded, body.linearVelocity, settings.movementThreshold, Time.deltaTime);
            SpriteSequence sequence = settings.GetSequence(clock.Pose);
            int frame = clock.Frame(sequence.frames.Length, sequence.framesPerSecond, sequence.loop);
            if (frame >= 0 && sequence.frames[frame] != null) spriteRenderer.sprite = sequence.frames[frame];
            spriteRenderer.flipX = player.Motor.Facing < 0;
            if (settings.idle.frames.Length > 0 && settings.idle.frames[0] != null)
                transform.localScale = Vector3.one * (settings.cellHeight / settings.idle.frames[0].bounds.size.y);
            transform.localPosition = settings.visualOffset;
        }
    }
}
