using System.Collections.Generic;
using UnityEngine;

namespace Lampkeeper.Visual
{
    /// <summary>적의 비주얼에 붙인다. 사망 후 시체가 남는 경우 이 컴포넌트도 비활성화한다.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyCameraTarget : MonoBehaviour
    {
        static readonly HashSet<EnemyCameraTarget> targets = new HashSet<EnemyCameraTarget>();
        public static IEnumerable<EnemyCameraTarget> Active => targets;
        [SerializeField] Renderer targetRenderer;

        public Bounds WorldBounds => targetRenderer != null
            ? targetRenderer.bounds : new Bounds(transform.position, Vector3.zero);
        public bool IsVisibleTarget => targetRenderer == null || targetRenderer.enabled;

        void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        }
        void OnEnable() => targets.Add(this);
        void OnDisable() => targets.Remove(this);
        void OnDestroy() => targets.Remove(this);
    }
}
