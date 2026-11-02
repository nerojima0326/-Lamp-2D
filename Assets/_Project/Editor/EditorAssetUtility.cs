using UnityEditor;
using UnityEngine;

namespace Lampkeeper.Editor
{
    /// <summary>에디터 메뉴 스크립트들이 공유하는 에셋·폴더·직렬화 도우미.</summary>
    internal static class EditorAssetUtility
    {
        /// <summary>path에 에셋이 있으면 불러오고, 없으면 create로 만들어 저장한다.</summary>
        public static T LoadOrCreateAsset<T>(string path, System.Func<T> create) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            EnsureFolder(ParentFolder(path));
            asset = create();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = ParentFolder(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(parent.Length + 1));
        }

        public static string ParentFolder(string assetPath) => assetPath.Substring(0, assetPath.LastIndexOf('/'));

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        // 직렬화된 private 필드에 인스펙터와 같은 방식으로 값을 넣는다.
        public static void SetReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            FindProperty(serialized, fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetReferenceArray(Object target, string fieldName, Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty array = FindProperty(serialized, fieldName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetEnum(Object target, string fieldName, int enumValueIndex)
        {
            var serialized = new SerializedObject(target);
            FindProperty(serialized, fieldName).enumValueIndex = enumValueIndex;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static SerializedProperty FindProperty(SerializedObject serialized, string fieldName)
        {
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
                throw new System.InvalidOperationException($"{serialized.targetObject.GetType().Name}에 '{fieldName}' 필드가 없습니다. 이 필드를 쓰는 에디터 스크립트를 함께 수정하세요.");
            return property;
        }
    }
}
