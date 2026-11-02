using System.Collections.Generic;
using Lampkeeper.Light;
using Lampkeeper.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Lampkeeper.Editor
{
    /// <summary>
    /// 현재 씬에 비주얼 설정(빛, 그림자, 팔레트 색)을 적용한다.
    /// 있으면 찾아서 다시 설정하고 없을 때만 만들므로, 몇 번을 실행해도 결과가 같다.
    /// 게임플레이 오브젝트(LightSource, 지형 콜라이더)는 건드리지 않고 비주얼 컴포넌트만 붙인다.
    /// </summary>
    public static class VisualPass
    {
        const string PalettePath = "Assets/_Project/Data/Visual/VisualPalette.asset";
        const string LightingProfilePath = "Assets/_Project/Data/Visual/LightingProfile.asset";
        const string TerrainStylePath = "Assets/_Project/Data/Visual/TerrainStyle.asset";

        // 그레이박스 빌더가 쓰는 이름 규칙
        const string LevelRootName = "Level";

        const string SurfaceNamePrefix = "Surface";

        const string SceneLightingName = "[Visual] Scene Lighting";
        const string GlobalLightName = "Global Light 2D";
        const string CoreLightName = "Light Core";
        const string HaloLightName = "Light Halo";

        [MenuItem("Tools/Lampkeeper/Apply Visual Pass")]
        public static void ApplyToActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            Apply(scene);
            if (!string.IsNullOrEmpty(scene.path))
                EditorSceneManager.SaveScene(scene);
            Debug.Log($"비주얼 패스를 적용했습니다: {scene.name}. 색은 {PalettePath}, 빛은 {LightingProfilePath}에서 조정합니다.");
        }

        public static void Apply(Scene scene)
        {
            VisualPalette palette = EditorAssetUtility.LoadOrCreateAsset(PalettePath, () => ScriptableObject.CreateInstance<VisualPalette>());
            LightingProfile profile = EditorAssetUtility.LoadOrCreateAsset(LightingProfilePath, () => ScriptableObject.CreateInstance<LightingProfile>());
            TerrainStyle terrainStyle = EditorAssetUtility.LoadOrCreateAsset(TerrainStylePath, () => ScriptableObject.CreateInstance<TerrainStyle>());
            AssetDatabase.SaveAssets();

            ApplySceneLighting(palette, profile);
            ApplyLightSources(palette, profile);
            ApplyTerrain(palette, profile, terrainStyle);
            ApplyLightPlatforms(palette);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        static void ApplySceneLighting(VisualPalette palette, LightingProfile profile)
        {
            SceneLighting lighting = Object.FindAnyObjectByType<SceneLighting>(FindObjectsInactive.Include);
            if (lighting == null)
                lighting = new GameObject(SceneLightingName).AddComponent<SceneLighting>();

            Light2D globalLight = FindGlobalLight();
            if (globalLight == null)
            {
                globalLight = new GameObject(GlobalLightName).AddComponent<Light2D>();
                globalLight.lightType = Light2D.LightType.Global;
            }

            EditorAssetUtility.SetReference(lighting, "palette", palette);
            EditorAssetUtility.SetReference(lighting, "profile", profile);
            EditorAssetUtility.SetReference(lighting, "globalLight", globalLight);
            EditorAssetUtility.SetReference(lighting, "targetCamera", Camera.main);
        }

        static Light2D FindGlobalLight()
        {
            foreach (Light2D light in Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include))
            {
                if (light.lightType == Light2D.LightType.Global)
                    return light;
            }
            return null;
        }

        static void ApplyLightSources(VisualPalette palette, LightingProfile profile)
        {
            foreach (LightSource source in Object.FindObjectsByType<LightSource>(FindObjectsInactive.Include))
            {
                // 예전에는 광원 오브젝트에 2D 라이트를 직접 붙였다. 이제 비주얼은 자식 라이트가 맡으므로 정리한다.
                var legacyLight = source.GetComponent<Light2D>();
                if (legacyLight != null)
                    Object.DestroyImmediate(legacyLight);

                bool isLantern = source.GetComponent<Lantern>() != null;
                var visual = EditorAssetUtility.GetOrAddComponent<LightSourceVisual>(source.gameObject);
                Light2D core = GetOrCreateChildLight(source.transform, CoreLightName);
                Light2D halo = isLantern ? GetOrCreateChildLight(source.transform, HaloLightName) : null;

                EditorAssetUtility.SetReference(visual, "source", source);
                EditorAssetUtility.SetReference(visual, "palette", palette);
                EditorAssetUtility.SetReference(visual, "profile", profile);
                EditorAssetUtility.SetEnum(visual, "role", (int)(isLantern ? LightSourceVisual.Role.Lantern : LightSourceVisual.Role.Ember));
                EditorAssetUtility.SetReference(visual, "core", core);
                EditorAssetUtility.SetReference(visual, "halo", halo);
            }
        }

        static Light2D GetOrCreateChildLight(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, false);
            }

            var light = EditorAssetUtility.GetOrAddComponent<Light2D>(child.gameObject);
            light.lightType = Light2D.LightType.Point;
            return light;
        }

        static void ApplyTerrain(VisualPalette palette, LightingProfile profile, TerrainStyle terrainStyle)
        {
            GameObject level = GameObject.Find(LevelRootName);
            if (level == null)
            {
                Debug.LogWarning($"'{LevelRootName}' 오브젝트가 없어 지형 비주얼을 건너뜁니다.");
                return;
            }

            var blocks = new List<BoxCollider2D>();
            foreach (Transform child in level.transform)
            {
                var collider = child.GetComponent<BoxCollider2D>();
                if (collider != null)
                    blocks.Add(collider);
            }
            List<Rect> terrain = blocks.ConvertAll(WorldRect);

            bool hasScaledBlocks = false;
            for (int i = 0; i < blocks.Count; i++)
            {
                Transform block = blocks[i].transform;
                var renderer = block.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.color = palette.terrainColor;
                EditorAssetUtility.GetOrAddComponent<ShadowCaster2D>(block.gameObject);
                RebuildSurfaces(block, renderer, terrain[i], terrain, palette, terrainStyle);
                hasScaledBlocks |= block.localScale != Vector3.one;
            }

            var style = EditorAssetUtility.GetOrAddComponent<TerrainShadowStyle>(level);
            EditorAssetUtility.SetReference(style, "profile", profile);
            style.Refresh();

            if (hasScaledBlocks)
                Debug.LogWarning("지형 블록이 Transform 스케일로 크기를 정하고 있어 그림자 가장자리가 정확하지 않습니다. 'Lampkeeper/그레이박스 씬 만들기'로 씬을 다시 만드세요.");
        }

        static Rect WorldRect(BoxCollider2D collider)
        {
            Vector2 size = Vector2.Scale(collider.size, collider.transform.lossyScale);
            size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            Vector2 center = collider.transform.TransformPoint(collider.offset);
            return new Rect(center - size * 0.5f, size);
        }

        // 바위 몸통은 스스로의 그림자로 어두우므로, 빛을 받아 윗면을 드러낼 띠를 블록 바로 위에 따로 얹는다.
        // 몸통 밖에 있어서 자기 그림자에 덮이지 않고, 다른 벽이 드리우는 그림자는 그대로 받는다.
        static void RebuildSurfaces(Transform block, SpriteRenderer blockRenderer, Rect rect, IReadOnlyList<Rect> terrain, VisualPalette palette, TerrainStyle style)
        {
            // 다시 실행해도 같은 결과가 되도록 기존 띠를 지우고 새로 만든다.
            for (int i = block.childCount - 1; i >= 0; i--)
            {
                Transform child = block.GetChild(i);
                if (child.name.StartsWith(SurfaceNamePrefix))
                    Object.DestroyImmediate(child.gameObject);
            }
            if (blockRenderer == null)
                return;

            List<Vector2> spans = TerrainSurfaces.ExposedTopSpans(rect, terrain, style.surfaceThickness, style.minSurfaceWidth);
            for (int i = 0; i < spans.Count; i++)
            {
                var surface = new GameObject($"{SurfaceNamePrefix} {i}");
                surface.transform.SetParent(block, false);
                surface.transform.position = new Vector2((spans[i].x + spans[i].y) * 0.5f, rect.yMax + style.surfaceThickness * 0.5f);

                var renderer = surface.AddComponent<SpriteRenderer>();
                renderer.sprite = blockRenderer.sprite;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(spans[i].y - spans[i].x, style.surfaceThickness);
                renderer.color = palette.terrainSurfaceColor;
                renderer.sortingOrder = style.surfaceSortingOrder;
            }
        }

        static void ApplyLightPlatforms(VisualPalette palette)
        {
            foreach (LightPlatform platform in Object.FindObjectsByType<LightPlatform>(FindObjectsInactive.Include))
            {
                var renderer = platform.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.color = palette.memoryPlatformColor;
            }
        }
    }
}
