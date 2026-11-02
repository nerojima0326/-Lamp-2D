using System.IO;
using Lampkeeper.Light;
using Lampkeeper.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if LAMPKEEPER_CINEMACHINE
using Unity.Cinemachine;
#endif

namespace Lampkeeper.Editor
{
    /// <summary>
    /// 조작감·빛 기믹 테스트용 그레이박스 씬과 필요한 에셋을 만든다. 빛·그림자·색은 마지막에 <see cref="VisualPass"/>가 입힌다.
    /// 이미 있는 에셋(설정, 물리 재질, 스프라이트)은 그대로 재사용하므로 튜닝 값이 초기화되지 않는다.
    /// </summary>
    public static class GreyboxSceneBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/M1_Greybox.unity";
        const string SettingsPath = "Assets/_Project/Data/PlayerMovementSettings.asset";
        const string LanternSettingsPath = "Assets/_Project/Data/LanternSettings.asset";
        const string LightPlatformSettingsPath = "Assets/_Project/Data/LightPlatformSettings.asset";
        const string NoFrictionMaterialPath = "Assets/_Project/Data/PlayerNoFriction.physicsMaterial2D";
        const string SquareSpritePath = "Assets/_Project/Art/Greybox/GreyboxSquare.png";

        // 1×1 유닛 흰색 정사각형. 블록은 Sliced 모드로 크기만 지정한다.
        const int SquarePixels = 4;

        static readonly Vector2 PlayerSize = new Vector2(0.8f, 1.4f);
        static readonly Vector2 PlayerSpawn = new Vector2(1f, 1f);
        const float PlayerEdgeRadius = 0.05f;
        const int PlayerSortingOrder = 10;

        static readonly Vector2 LanternSize = new Vector2(0.35f, 0.45f);
        const int LanternSortingOrder = 11;

        const float CameraZ = -10f;
        const float CameraOrthographicSize = 7f;
        static readonly Vector3 FollowDamping = new Vector3(0.25f, 0.45f, 0f);
        const float LookaheadTime = 0.3f;
        const float LookaheadSmoothing = 6f;

        static readonly Color PlayerColor = new Color(1f, 0.72f, 0.3f);
        static readonly Color LanternColor = new Color(1f, 0.88f, 0.5f);

        // 레벨 블록: Rect(왼쪽 x, 아래 y, 너비, 높이). 기본 바닥 윗면은 y = 0.
        // 기본 튜닝 기준: 최대 점프 3.2, 최대 속도로 뛴 점프 거리 약 6.
        static readonly Rect[] LevelBlocks =
        {
            // 구간 1: 달리기, 점프 높이, 머리 박기
            new Rect(-3f, -3.5f, 1f, 18f),        // 왼쪽 벽
            new Rect(-2f, -3.5f, 32f, 3.5f),      // 시작 바닥 (x -2 ~ 30)
            new Rect(6f, 0f, 2f, 1f),             // 높이 1 턱
            new Rect(10f, 0f, 2f, 2f),            // 높이 2 턱
            new Rect(14f, 0f, 2f, 3f),            // 높이 3 턱 (최대 점프에 가까움)
            new Rect(19f, 2.2f, 6f, 1f),          // 낮은 천장 (점프하면 머리를 박는다)

            // 구간 2: 간격 2 / 3.5 / 5. 구덩이는 얕아서 떨어져도 점프로 다시 올라온다.
            new Rect(30f, -3.5f, 18.5f, 1f),      // 구덩이 바닥 (윗면 y -2.5)
            new Rect(32f, -3.5f, 4f, 3.5f),       // 간격 2 뒤 발판
            new Rect(39.5f, -3.5f, 4f, 3.5f),     // 간격 3.5 뒤 발판
            new Rect(48.5f, -3.5f, 36.5f, 3.5f),  // 간격 5 뒤 긴 바닥 (x 48.5 ~ 85)

            // 구간 3: 떠 있는 계단을 올라 높이 10에서 뛰어내린다 (최대 낙하 속도 확인).
            // 계단 아래는 비어 있어 걸어서 돌아올 수 있다.
            new Rect(58f, 2f, 3f, 0.5f),
            new Rect(61.5f, 4.5f, 3f, 0.5f),
            new Rect(65f, 7f, 3f, 0.5f),
            new Rect(68.5f, 9.5f, 4f, 0.5f),

            // 구간 4 (M2): 빛 발판 구덩이. 떨어지면 왼쪽 계단으로 돌아 나온다.
            new Rect(85f, -7f, 2f, 5f),           // 탈출 계단 (윗면 -2)
            new Rect(87f, -7f, 2f, 3f),           // 탈출 계단 (윗면 -4)
            new Rect(85f, -7f, 26f, 1f),          // 구덩이 바닥 (윗면 -6)
            new Rect(111f, -7f, 11f, 7f),         // 도착 바닥 (x 111 ~ 122)
            new Rect(122f, -7f, 1f, 21f),         // 오른쪽 벽
        };

        // 빛 발판. 기본 튜닝(등불 반경 3.5, 던지기 7) 기준으로,
        // 첫 발판은 들고 다가가면 보이고 그 뒤로는 등불을 던져야 길이 생긴다.
        static readonly Rect[] LightPlatformBlocks =
        {
            new Rect(88f, -0.5f, 2f, 0.5f),       // 윗면 0, 바닥 끝에서 간격 3
            new Rect(94f, 0f, 2f, 0.5f),          // 윗면 0.5, 간격 4
            new Rect(100f, 0.5f, 2f, 0.5f),       // 윗면 1, 간격 4
            new Rect(105.5f, 0f, 2f, 0.5f),       // 윗면 0.5, 간격 3.5 (도착 바닥까지 3.5)
        };

        // 테스트용 순간이동 지점 (숫자 키 1부터 순서대로). 이름은 화면 안내에 표시된다.
        static readonly (string Name, Vector2 Position)[] WarpPoints =
        {
            ("시작 - 달리기, 점프 높이", PlayerSpawn),
            ("간격 점프", new Vector2(27f, 1f)),
            ("떠 있는 계단, 높은 낙하", new Vector2(55f, 1f)),
            ("빛 발판 (M2)", new Vector2(83f, 1f)),
        };

        [MenuItem("Lampkeeper/그레이박스 씬 만들기")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                && !EditorUtility.DisplayDialog("그레이박스 씬", $"{ScenePath}이(가) 이미 있습니다. 새로 만들어 덮어쓸까요?", "덮어쓰기", "취소"))
                return;

            // 씬을 먼저 바꾼다. NewScene(Single)이 쓰이지 않는 에셋을 언로드하므로,
            // 그 전에 불러 둔 ScriptableObject 참조는 죽은 객체가 되어 직렬화되지 않는다.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayerMovementSettings settings = EditorAssetUtility.LoadOrCreateAsset(SettingsPath, () => ScriptableObject.CreateInstance<PlayerMovementSettings>());
            LanternSettings lanternSettings = EditorAssetUtility.LoadOrCreateAsset(LanternSettingsPath, () => ScriptableObject.CreateInstance<LanternSettings>());
            LightPlatformSettings platformSettings = EditorAssetUtility.LoadOrCreateAsset(LightPlatformSettingsPath, () => ScriptableObject.CreateInstance<LightPlatformSettings>());
            PhysicsMaterial2D noFriction = EditorAssetUtility.LoadOrCreateAsset(NoFrictionMaterialPath, CreateNoFrictionMaterial);
            Sprite square = LoadOrCreateSquareSprite();
            AssetDatabase.SaveAssets();

            Camera camera = CreateCamera();
            CreateLevel(square);
            CreateLightPlatforms(square, platformSettings);
            GameObject player = CreatePlayer(square, settings, noFriction);
            LightSource ember = CreateEmber(player);
            Lantern lantern = CreateLantern(square, lanternSettings, player, ember);
            var controller = player.GetComponent<PlayerController>();
            EditorAssetUtility.SetReference(controller, "lantern", lantern);
            CreateDebugWarp(controller);
            DashTrialSetup.Apply(scene);
            CreateFollowCamera(camera, player.transform);
            EncounterCameraSetup.Apply(scene);
            VisualPass.Apply(scene);

            EditorAssetUtility.EnsureFolder(EditorAssetUtility.ParentFolder(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = player;
            Debug.Log($"그레이박스 씬을 만들었습니다: {ScenePath}. 튜닝은 Assets/_Project/Data/의 설정 에셋에서 플레이 중에도 바꿀 수 있습니다.");
        }

        static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(PlayerSpawn.x, PlayerSpawn.y, CameraZ);

            // 배경색은 SceneLighting이 팔레트에 맞춘다.
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraOrthographicSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            go.AddComponent<AudioListener>();
            return camera;
        }

        static void CreateLevel(Sprite square)
        {
            var root = new GameObject("Level").transform;
            for (int i = 0; i < LevelBlocks.Length; i++)
                CreateBlock($"Block {i:00}", root, LevelBlocks[i], square);
        }

        static void CreateLightPlatforms(Sprite square, LightPlatformSettings settings)
        {
            var root = new GameObject("Light Platforms").transform;
            for (int i = 0; i < LightPlatformBlocks.Length; i++)
            {
                GameObject block = CreateBlock($"Light Platform {i:00}", root, LightPlatformBlocks[i], square);
                EditorAssetUtility.SetReference(block.AddComponent<LightPlatform>(), "settings", settings);
            }
        }

        // 색은 VisualPass가 팔레트로 입힌다.
        static GameObject CreateBlock(string name, Transform parent, Rect rect, Sprite square)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = rect.center;

            // Transform 스케일 대신 크기를 지정한다. 그림자 가장자리와 텍스처가 늘어나지 않는다.
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = rect.size;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = rect.size;
            return go;
        }

        static GameObject CreatePlayer(Sprite square, PlayerMovementSettings settings, PhysicsMaterial2D noFriction)
        {
            var player = new GameObject("Player");
            player.transform.position = PlayerSpawn;

            // 콜라이더와 비주얼을 분리해 두면 나중에 스프라이트로 교체하거나 스쿼시/스트레치를 넣기 쉽다.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localScale = new Vector3(PlayerSize.x, PlayerSize.y, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = PlayerColor;
            renderer.sortingOrder = PlayerSortingOrder;

            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sharedMaterial = noFriction;

            // 모서리를 살짝 둥글게 해서 블록 이음새에 걸리지 않게 한다. 바깥 크기는 PlayerSize와 같다.
            var collider = player.AddComponent<BoxCollider2D>();
            collider.edgeRadius = PlayerEdgeRadius;
            collider.size = PlayerSize - Vector2.one * (PlayerEdgeRadius * 2f);

            EditorAssetUtility.SetReference(player.AddComponent<PlayerController>(), "settings", settings);
            return player;
        }

        // 반경은 런타임에 Lantern이 설정 에셋 값으로 맞추고, 2D 라이트는 VisualPass가 붙인다.
        static LightSource CreateEmber(GameObject player)
        {
            var go = new GameObject("Ember");
            go.transform.SetParent(player.transform, false);
            return go.AddComponent<LightSource>();
        }

        static Lantern CreateLantern(Sprite square, LanternSettings settings, GameObject player, LightSource ember)
        {
            var go = new GameObject("Lantern");
            go.transform.position = (Vector2)player.transform.position + settings.holdOffset;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(LanternSize.x, LanternSize.y, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = LanternColor;
            renderer.sortingOrder = LanternSortingOrder;

            var lightSource = go.AddComponent<LightSource>();
            var lantern = go.AddComponent<Lantern>();
            EditorAssetUtility.SetReference(lantern, "settings", settings);
            EditorAssetUtility.SetReference(lantern, "holder", player.transform);
            EditorAssetUtility.SetReference(lantern, "lightSource", lightSource);
            EditorAssetUtility.SetReference(lantern, "ember", ember);
            return lantern;
        }

        static void CreateDebugWarp(PlayerController player)
        {
            var root = new GameObject("Debug Warp");
            var points = new Object[WarpPoints.Length];
            for (int i = 0; i < WarpPoints.Length; i++)
            {
                var point = new GameObject(WarpPoints[i].Name).transform;
                point.SetParent(root.transform, false);
                point.position = WarpPoints[i].Position;
                points[i] = point;
            }

            var warp = root.AddComponent<DebugWarp>();
            EditorAssetUtility.SetReference(warp, "player", player);
            EditorAssetUtility.SetReferenceArray(warp, "warpPoints", points);
        }

        static void CreateFollowCamera(Camera camera, Transform target)
        {
#if LAMPKEEPER_CINEMACHINE
            camera.gameObject.AddComponent<CinemachineBrain>();

            var go = new GameObject("Follow Camera");
            go.transform.position = camera.transform.position;
            var followCamera = go.AddComponent<CinemachineCamera>();
            followCamera.Follow = target;
            LensSettings lens = followCamera.Lens;
            lens.OrthographicSize = CameraOrthographicSize;
            followCamera.Lens = lens;

            var composer = go.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = -CameraZ;
            composer.Damping = FollowDamping;
            composer.Lookahead = new LookaheadSettings
            {
                Enabled = true,
                Time = LookaheadTime,
                Smoothing = LookaheadSmoothing,
                IgnoreY = true,
            };
#else
            Debug.LogWarning("Cinemachine 3이 설치되어 있지 않아 카메라가 고정됩니다. Package Manager에서 Cinemachine을 설치한 뒤 메뉴를 다시 실행하세요.");
#endif
        }

        static PhysicsMaterial2D CreateNoFrictionMaterial() => new PhysicsMaterial2D { friction = 0f, bounciness = 0f };

        static Sprite LoadOrCreateSquareSprite()
        {
            if (!File.Exists(SquareSpritePath))
            {
                EditorAssetUtility.EnsureFolder(EditorAssetUtility.ParentFolder(SquareSpritePath));
                var texture = new Texture2D(SquarePixels, SquarePixels);
                var pixels = new Color32[SquarePixels * SquarePixels];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;
                texture.SetPixels32(pixels);
                File.WriteAllBytes(SquareSpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SquareSpritePath);
            }

            // Sliced 모드로 늘려 쓰려면 Full Rect 메시가 필요하다. 예전에 만든 에셋도 여기서 맞춘다.
            var importer = (TextureImporter)AssetImporter.GetAtPath(SquareSpritePath);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (importer.textureType != TextureImporterType.Sprite || settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.textureType = TextureImporterType.Sprite;
                settings.spriteMode = (int)SpriteImportMode.Single;
                settings.spritePixelsPerUnit = SquarePixels;
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.filterMode = FilterMode.Point;
                settings.mipmapEnabled = false;
                importer.SetTextureSettings(settings);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
        }
    }
}
