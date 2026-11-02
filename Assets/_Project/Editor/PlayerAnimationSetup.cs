using System.IO;
using Lampkeeper.Player;
using Lampkeeper.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Editor
{
    public static class PlayerAnimationSetup
    {
        const string SheetPath = "Assets/_Project/Art/Characters/Animation/Lampkeeper_Movement.png";
        const string FramesFolder = "Assets/_Project/Art/Characters/Animation/Frames";
        const string SettingsPath = "Assets/_Project/Data/Visual/PlayerAnimationSettings.asset";
        const int Columns = 4;
        const int Rows = 4;
        const float PixelsPerUnit = 100f;

        [MenuItem("Tools/Lampkeeper/Apply Player Animation")]
        public static void ApplyAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Apply(SceneManager.GetActiveScene()))
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static bool Apply(Scene scene)
        {
            if (!File.Exists(SheetPath)) return false;
            AssetDatabase.ImportAsset(SheetPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
            if (importer.filterMode != FilterMode.Point || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed || !importer.alphaIsTransparency || !importer.isReadable)
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            Color32[] pixels = sheet.GetPixels32();
            var settings = EditorAssetUtility.LoadOrCreateAsset(SettingsPath,
                () => ScriptableObject.CreateInstance<PlayerAnimationSettings>());
            EditorAssetUtility.EnsureFolder(FramesFolder);
            int width = sheet.width / Columns;
            int height = sheet.height / Rows;
            for (int row = 0; row < Rows; row++)
            {
                var sequence = settings.GetSequence((PlayerPose)row);
                sequence.frames = new Sprite[Columns];
                for (int column = 0; column < Columns; column++)
                {
                    string name = $"{(PlayerPose)row}_{column}";
                    string path = $"{FramesFolder}/{name}.asset";
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null)
                    {
                        var rect = new Rect(column * width, (Rows - row - 1) * height, width, height);
                        sprite = Sprite.Create(sheet, rect, FootPivot(pixels, sheet.width, rect),
                            PixelsPerUnit, 0, SpriteMeshType.FullRect);
                        sprite.name = name;
                        AssetDatabase.CreateAsset(sprite, path);
                    }
                    sequence.frames[column] = sprite;
                }
            }
            EditorUtility.SetDirty(settings);
            foreach (var player in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include))
            {
                if (player.gameObject.scene != scene) continue;
                Transform visual = player.transform.Find("Visual");
                if (visual == null) continue;
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                var facing = visual.GetComponent<PlayerFacingVisual>();
                if (facing != null) Object.DestroyImmediate(facing);
                var animation = EditorAssetUtility.GetOrAddComponent<PlayerSpriteAnimation>(visual.gameObject);
                EditorAssetUtility.SetReference(animation, "player", player);
                EditorAssetUtility.SetReference(animation, "body", player.GetComponent<Rigidbody2D>());
                EditorAssetUtility.SetReference(animation, "spriteRenderer", renderer);
                EditorAssetUtility.SetReference(animation, "settings", settings);
                renderer.sprite = settings.idle.frames[0];
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.color = Color.white;
                visual.localScale = Vector3.one * (settings.cellHeight / renderer.sprite.bounds.size.y);
                visual.localPosition = settings.visualOffset;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("플레이어 대기/달리기/점프/낙하 애니메이션을 연결했습니다.");
            return true;
        }

        // 프레임에 그려진 이동 대신 실제 Rigidbody 이동을 사용하도록 발 아래를 기준점으로 맞춘다.
        static Vector2 FootPivot(Color32[] pixels, int sheetWidth, Rect rect)
        {
            int left = (int)rect.width, right = 0, bottom = (int)rect.height;
            for (int y = 0; y < rect.height; y++)
            for (int x = 0; x < rect.width; x++)
            {
                if (pixels[((int)rect.y + y) * sheetWidth + (int)rect.x + x].a < 128) continue;
                left = Mathf.Min(left, x);
                right = Mathf.Max(right, x);
                bottom = Mathf.Min(bottom, y);
            }
            return bottom == (int)rect.height ? Vector2.one * 0.5f
                : new Vector2((left + right + 1f) * 0.5f / rect.width, bottom / rect.height);
        }
    }
}
