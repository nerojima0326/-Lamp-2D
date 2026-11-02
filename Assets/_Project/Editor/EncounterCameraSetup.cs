using Lampkeeper.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if LAMPKEEPER_CINEMACHINE
using Unity.Cinemachine;
#endif

namespace Lampkeeper.Editor
{
    public static class EncounterCameraSetup
    {
        const string SettingsPath = "Assets/_Project/Data/Visual/EncounterCameraSettings.asset";

        [MenuItem("Tools/Lampkeeper/Apply Encounter Camera")]
        public static void ApplyAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Apply(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static void Apply(Scene scene)
        {
#if LAMPKEEPER_CINEMACHINE
            var settings = EditorAssetUtility.LoadOrCreateAsset(SettingsPath,
                () => ScriptableObject.CreateInstance<EncounterCameraSettings>());
            foreach (var camera in Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include))
            {
                if (camera.gameObject.scene != scene || camera.Follow == null) continue;
                var zoom = EditorAssetUtility.GetOrAddComponent<EncounterCameraZoom>(camera.gameObject);
                EditorAssetUtility.SetReference(zoom, "settings", settings);
                EditorUtility.SetDirty(zoom);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("적 감지 줌 카메라를 적용했습니다. 적에는 EnemyCameraTarget을 붙이세요.");
#endif
        }
    }
}
