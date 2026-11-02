using Lampkeeper.Player;
using Lampkeeper.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Editor
{
    public static class PlayerDashSetup
    {
        const string SettingsPath = "Assets/_Project/Data/Visual/DashTrailSettings.asset";
        const string MaterialPath = "Assets/_Project/Art/Characters/Animation/DashAfterimage.mat";

        [MenuItem("Tools/Lampkeeper/Apply Player Dash")]
        public static void ApplyAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = SceneManager.GetActiveScene();
            Apply(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Shift 대시와 잔상을 연결하고 씬을 저장했습니다.");
        }

        public static void Apply(Scene scene)
        {
            var settings = EditorAssetUtility.LoadOrCreateAsset(SettingsPath,
                () => ScriptableObject.CreateInstance<DashTrailSettings>());
            if (settings.material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader != null)
                {
                    settings.material = EditorAssetUtility.LoadOrCreateAsset(MaterialPath, () => new Material(shader));
                    EditorUtility.SetDirty(settings);
                }
            }
            foreach (var player in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include))
            {
                if (player.gameObject.scene != scene) continue;
                Transform visual = player.transform.Find("Visual");
                if (visual == null || !visual.TryGetComponent<SpriteRenderer>(out var source)) continue;
                var trail = EditorAssetUtility.GetOrAddComponent<PlayerDashTrail>(visual.gameObject);
                EditorAssetUtility.SetReference(trail, "player", player);
                EditorAssetUtility.SetReference(trail, "source", source);
                EditorAssetUtility.SetReference(trail, "settings", settings);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
