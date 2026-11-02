using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Lampkeeper.Editor
{
    /// <summary>
    /// URP 2D 렌더러를 쓰는 파이프라인 에셋을 만들고 프로젝트 기본 렌더 파이프라인으로 지정한다.
    /// 'Assets/Create/Rendering/URP Asset (with 2D Renderer)' 메뉴와 같은 일을 고정된 경로에 한다.
    /// 이미 만든 에셋은 재사용하므로 여러 번 실행해도 설정이 초기화되지 않는다.
    /// </summary>
    public static class UrpSetup
    {
        const string PipelineAssetPath = "Assets/_Project/Settings/Lampkeeper_URP.asset";
        const string RendererDataPath = "Assets/_Project/Settings/Lampkeeper_URP_2DRenderer.asset";

        // URP 패키지에 포함된 기본 포스트 프로세스 데이터. 렌더러에 지정해야 블룸 같은 효과를 쓸 수 있다.
        const string DefaultPostProcessDataPath = "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";

        // Renderer2DData의 직렬화 필드 이름. 공개 setter가 없어 인스펙터와 같은 방식(SerializedObject)으로 지정한다.
        const string PostProcessDataFieldName = "m_PostProcessData";

        [MenuItem("Lampkeeper/URP 2D 렌더러 설정")]
        public static void Setup()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
                pipeline = CreatePipelineAsset();

            GraphicsSettings.defaultRenderPipeline = pipeline;
            AssetDatabase.SaveAssets();
            WarnAboutQualityOverrides(pipeline);

            Debug.Log($"URP 2D 렌더러를 기본 렌더 파이프라인으로 지정했습니다: {PipelineAssetPath}. " +
                      "씬의 스프라이트가 2D 라이트를 받게 하려면 'Lampkeeper/그레이박스 씬 만들기'를 다시 실행하세요.");
        }

        static UniversalRenderPipelineAsset CreatePipelineAsset()
        {
            EditorAssetUtility.EnsureFolder(EditorAssetUtility.ParentFolder(PipelineAssetPath));

            var rendererData = ScriptableObject.CreateInstance<Renderer2DData>();
            AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
            AssignDefaultPostProcessData(rendererData);

            var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            return pipeline;
        }

        static void AssignDefaultPostProcessData(Renderer2DData rendererData)
        {
            var postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(DefaultPostProcessDataPath);
            var serialized = new SerializedObject(rendererData);
            SerializedProperty property = serialized.FindProperty(PostProcessDataFieldName);
            if (postProcessData == null || property == null)
            {
                Debug.LogWarning("URP 기본 포스트 프로세스 데이터를 지정하지 못했습니다. 2D 렌더러 에셋의 인스펙터에서 Post-processing Data를 직접 지정하세요.", rendererData);
                return;
            }

            property.objectReferenceValue = postProcessData;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WarnAboutQualityOverrides(RenderPipelineAsset pipeline)
        {
            // 품질 단계에 지정된 파이프라인은 기본 파이프라인보다 우선한다.
            string[] qualityNames = QualitySettings.names;
            for (int i = 0; i < qualityNames.Length; i++)
            {
                RenderPipelineAsset qualityPipeline = QualitySettings.GetRenderPipelineAssetAt(i);
                if (qualityPipeline != null && qualityPipeline != pipeline)
                    Debug.LogWarning($"품질 단계 '{qualityNames[i]}'에 다른 렌더 파이프라인({qualityPipeline.name})이 지정되어 있어 기본값보다 우선합니다. " +
                                     $"Project Settings > Quality에서 비우거나 {PipelineAssetPath}로 바꾸세요.");
            }
        }
    }
}
