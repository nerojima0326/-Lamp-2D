using UnityEngine;

namespace Lampkeeper.Light
{
    /// <summary>
    /// 반경과 세기를 가진 게임플레이 광원. 켜져 있는 동안 <see cref="LightField.Main"/>에 등록된다.
    /// 화면에 어떻게 보일지는 모른다. 비주얼(Lampkeeper.Visual)이 이 값을 읽어 2D 라이트를 맞춘다.
    /// </summary>
    public sealed class LightSource : MonoBehaviour, ILightEmitter
    {
        [Tooltip("빛이 닿는 반경 (유닛)")]
        [SerializeField, Min(0f)] float radius = 3f;

        [Tooltip("빛의 세기. 노출도 판정에 쓰인다.")]
        [SerializeField, Min(0f)] float intensity = 1f;

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public float Intensity
        {
            get => intensity;
            set => intensity = value;
        }

        public LightCircle Circle => new LightCircle(transform.position, radius, intensity);

        void OnEnable() => LightField.Main.Register(this);

        void OnDisable() => LightField.Main.Unregister(this);
    }
}
