using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lampkeeper.Visual
{
    /// <summary>
    /// 지형 그림자 설정을 프로필 값에 맞춘다. 지형은 벽 뒤로 그림자를 드리우고, 스스로의 그림자로 바위 몸통을 어둡게 한다
    /// (빛이 바위 속을 비추지 않게). 빛을 받는 윗면은 블록 위에 따로 얹는 표면 띠가 맡는다.
    /// 지형 블록들의 부모에 붙인다.
    /// </summary>
    [ExecuteAlways]
    public sealed class TerrainShadowStyle : MonoBehaviour
    {
        [SerializeField] LightingProfile profile;

        ShadowCaster2D[] casters;
        float appliedTrim = float.NaN;

        void OnEnable() => Refresh();

        /// <summary>자식 그림자 캐스터 목록을 다시 모으고 설정을 다시 적용한다. 블록을 추가·삭제한 뒤 호출한다.</summary>
        public void Refresh()
        {
            casters = GetComponentsInChildren<ShadowCaster2D>(true);
            appliedTrim = float.NaN;
            ApplyIfChanged();
        }

        void Update() => ApplyIfChanged();

        void ApplyIfChanged()
        {
            if (profile == null || casters == null || profile.terrainShadowTrim == appliedTrim)
                return;

            foreach (ShadowCaster2D caster in casters)
            {
                if (caster == null)
                    continue;
                caster.castingOption = ShadowCaster2D.ShadowCastingOptions.CastAndSelfShadow;
                // 로컬 단위로 적용된다. 블록은 스케일 없이 크기를 정하므로 대개 1로 나누게 된다.
                Vector3 scale = caster.transform.lossyScale;
                caster.trimEdge = profile.terrainShadowTrim / Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            }
            appliedTrim = profile.terrainShadowTrim;
        }
    }
}
