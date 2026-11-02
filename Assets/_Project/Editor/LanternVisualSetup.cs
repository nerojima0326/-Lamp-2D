using System.IO;
using Lampkeeper.Light;
using Lampkeeper.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Editor
{
    public static class LanternVisualSetup
    {
        const string SheetPath = "Assets/_Project/Art/Characters/Animation/Lantern_Flame.png";
        const string FramesFolder = "Assets/_Project/Art/Characters/Animation/LanternFrames";
        const string SettingsPath = "Assets/_Project/Data/Visual/LanternFlameSettings.asset";
        const int Grid = 2;
        const float PixelsPerUnit = 100f;

        [MenuItem("Tools/Lampkeeper/Apply Lantern Animation")]
        public static void ApplyAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Apply(SceneManager.GetActiveScene())) EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static bool Apply(Scene scene)
        {
            if (!File.Exists(SheetPath)) return false;
            AssetDatabase.ImportAsset(SheetPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
            if (importer.textureType != TextureImporterType.Default || !importer.isReadable
                || importer.filterMode != FilterMode.Point || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed || !importer.alphaIsTransparency)
            {
                importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            var settings = EditorAssetUtility.LoadOrCreateAsset(SettingsPath,
                () => ScriptableObject.CreateInstance<LanternFlameSettings>());
            EditorAssetUtility.EnsureFolder(FramesFolder);
            settings.frames = new Sprite[Grid * Grid];
            int width = texture.width / Grid, height = texture.height / Grid;
            for (int i = 0; i < settings.frames.Length; i++)
            {
                string path = $"{FramesFolder}/Flame_{i}.asset";
                Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frame == null)
                {
                    var rect = new Rect(i % Grid * width, (Grid - 1 - i / Grid) * height, width, height);
                    frame = Sprite.Create(texture, rect, FindCenter(texture, rect), PixelsPerUnit, 0, SpriteMeshType.FullRect);
                    frame.name = $"Flame_{i}";
                    AssetDatabase.CreateAsset(frame, path);
                }
                settings.frames[i] = frame;
            }
            EditorUtility.SetDirty(settings);
            foreach (var lantern in Object.FindObjectsByType<Lantern>(FindObjectsInactive.Include))
            {
                if (lantern.gameObject.scene != scene) continue;
                Transform visual = lantern.transform.Find("Visual");
                if (visual == null) continue;
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                bool firstInstall = visual.GetComponent<LanternFlameAnimation>() == null;
                var animation = EditorAssetUtility.GetOrAddComponent<LanternFlameAnimation>(visual.gameObject);
                EditorAssetUtility.SetReference(animation, "spriteRenderer", renderer);
                EditorAssetUtility.SetReference(animation, "settings", settings);
                renderer.sprite = settings.frames[0];
                renderer.color = Color.white;
                renderer.drawMode = SpriteDrawMode.Simple;
                visual.localPosition = Vector3.zero;
                visual.localScale = Vector3.one * (settings.cellHeight / renderer.sprite.bounds.size.y);
                // 첫 설치 시만 손 위치를 새 기본값에 맞추고, 이후 수동 튜닝은 보존한다.
                var serialized = new SerializedObject(lantern);
                var data = serialized.FindProperty("settings").objectReferenceValue as LanternSettings;
                var holder = serialized.FindProperty("holder").objectReferenceValue as Transform;
                if (firstInstall && data != null)
                {
                    var defaults = ScriptableObject.CreateInstance<LanternSettings>();
                    data.holdOffset = defaults.holdOffset;
                    Object.DestroyImmediate(defaults);
                    EditorUtility.SetDirty(data);
                    if (holder != null) lantern.transform.position = holder.position + (Vector3)data.holdOffset;
                }
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("등불 손 위치와 좌우 불꽃 애니메이션을 적용했습니다.");
            return true;
        }

        static Vector2 FindCenter(Texture2D texture, Rect rect)
        {
            int width = (int)rect.width, height = (int)rect.height;
            var pixels = texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            int minX = width, minY = height, maxX = -1, maxY = -1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].a < 0.5f) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
            return maxX < 0 ? Vector2.one * 0.5f
                : new Vector2((minX + maxX + 1f) / (2f * width), (minY + maxY + 1f) / (2f * height));
        }
    }
}
