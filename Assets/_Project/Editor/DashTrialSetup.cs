using Lampkeeper.Light;
using Lampkeeper.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Editor
{
    // 독립 시험 구간. 기존 레벨을 덮어쓰지 않으며 숫자 5로 진입한다.
    public static class DashTrialSetup
    {
        const string Prefix = "Dash Trial - ";
        const string SquarePath = "Assets/_Project/Art/Greybox/GreyboxSquare.png";
        const string PlatformSettingsPath = "Assets/_Project/Data/LightPlatformSettings.asset";

        [MenuItem("Tools/Lampkeeper/Apply Dash Trial")]
        public static void ApplyAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = SceneManager.GetActiveScene();
            if (!Apply(scene)) return;
            VisualPass.Apply(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("대시와 등불 시험 구간을 저장했습니다. 플레이 중 5번 키로 이동합니다.");
        }

        public static bool Apply(Scene scene)
        {
            var level = GameObject.Find("Level");
            var platforms = GameObject.Find("Light Platforms");
            var warp = Object.FindAnyObjectByType<DebugWarp>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
            var settings = AssetDatabase.LoadAssetAtPath<LightPlatformSettings>(PlatformSettingsPath);
            if (level == null || platforms == null || warp == null || sprite == null || settings == null
                || level.scene != scene || platforms.scene != scene || warp.gameObject.scene != scene)
            {
                Debug.LogWarning("대시 시험 구간: 기존 그레이박스 씬과 설정 에셋이 필요합니다.");
                return false;
            }

            Block(level.transform, "Start", new Rect(140f, -3.5f, 8f, 3.5f), sprite);
            Block(level.transform, "Dash Landing", new Rect(155.5f, -3.5f, 3f, 3.5f), sprite);
            Block(level.transform, "Goal", new Rect(173f, -3.5f, 7f, 3.5f), sprite);
            Block(level.transform, "Safety Floor", new Rect(140f, -7f, 40f, 1f), sprite);
            Block(level.transform, "Return Step 1", new Rect(152.5f, -6f, 2f, 2f), sprite);
            Block(level.transform, "Return Step 2", new Rect(149.5f, -6f, 2f, 4f), sprite);
            var light = Block(platforms.transform, "Lantern Landing", new Rect(165f, -0.5f, 2f, 0.5f), sprite);
            EditorAssetUtility.SetReference(EditorAssetUtility.GetOrAddComponent<LightPlatform>(light), "settings", settings);

            Transform point = warp.transform.Find(Prefix + "대시와 등불");
            if (point == null)
            {
                point = new GameObject(Prefix + "대시와 등불").transform;
                point.SetParent(warp.transform, false);
            }
            point.position = new Vector3(145f, 1f, 0f);
            var serialized = new SerializedObject(warp);
            var points = serialized.FindProperty("warpPoints");
            if (points.arraySize < 5) points.arraySize = 5;
            points.GetArrayElementAtIndex(4).objectReferenceValue = point;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            return true;
        }

        static GameObject Block(Transform parent, string name, Rect rect, Sprite sprite)
        {
            Transform child = parent.Find(Prefix + name);
            if (child == null)
            {
                child = new GameObject(Prefix + name).transform;
                child.SetParent(parent, false);
            }
            child.position = rect.center;
            var renderer = EditorAssetUtility.GetOrAddComponent<SpriteRenderer>(child.gameObject);
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = rect.size;
            EditorAssetUtility.GetOrAddComponent<BoxCollider2D>(child.gameObject).size = rect.size;
            return child.gameObject;
        }
    }
}
