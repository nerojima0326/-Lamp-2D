#if LAMPKEEPER_CINEMACHINE
using Unity.Cinemachine;
using UnityEngine;

namespace Lampkeeper.Visual
{
    [DisallowMultipleComponent]
    public sealed class EncounterCameraZoom : CinemachineExtension
    {
        [SerializeField] EncounterCameraSettings settings;

        class ZoomState : VcamExtraStateBase
        {
            public readonly EncounterZoom Zoom = new EncounterZoom();
            public bool Initialized;
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body || settings == null) return;
            var extra = GetExtraState<ZoomState>(vcam);
            if (!extra.Initialized || deltaTime < 0f || !vcam.PreviousStateIsValid)
            {
                extra.Zoom.Reset(settings.soloSize);
                extra.Initialized = true;
            }
            // 전환 도중 확대/축소되는 현재 화면 대신 두 상태의 고정 범위를 써서 경계 흔들림을 방지한다.
            bool wide = extra.Zoom.InEncounter;
            float size = wide ? Mathf.Max(settings.soloSize, settings.encounterSize) : settings.soloSize;
            float margin = settings.enterMargin + (wide ? settings.exitMargin : 0f);
            bool found = false;
            foreach (var enemy in EnemyCameraTarget.Active)
            {
                if (enemy != null && enemy.isActiveAndEnabled && enemy.IsVisibleTarget
                    && EncounterZoom.Overlaps(state.GetCorrectedPosition(), size, state.Lens.Aspect, margin, enemy.WorldBounds))
                {
                    found = true;
                    break;
                }
            }
            var lens = state.Lens;
            lens.OrthographicSize = extra.Zoom.Step(found, Mathf.Max(0f, deltaTime), settings);
            state.Lens = lens;
        }
    }
}
#endif
