using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lampkeeper.Editor
{
    /// <summary>
    /// 브라우저에서 바로 플레이할 수 있는 WebGL 빌드를 만든다.
    /// GitHub Actions(.github/workflows/webgl-pages.yml)가 BuildForCI를 호출해 GitHub Pages에 올린다.
    /// </summary>
    public static class WebBuild
    {
        const string ScenePath = "Assets/_Project/Scenes/M1_Greybox.unity";
        const string DefaultOutputPath = "Builds/WebGL";

        // game-ci/unity-builder가 출력 폴더를 넘겨줄 때 쓰는 인자 이름.
        const string OutputPathArgument = "-customBuildPath";

        [MenuItem("Lampkeeper/웹 빌드 (WebGL)")]
        public static void BuildFromMenu()
        {
            BuildReport report = Build(DefaultOutputPath);
            if (report.summary.result == BuildResult.Succeeded)
                EditorUtility.RevealInFinder(DefaultOutputPath);
        }

        /// <summary>배치 모드(CI) 진입점. 실패하면 종료 코드 1로 끝내 워크플로가 실패로 표시되게 한다.</summary>
        public static void BuildForCI()
        {
            BuildReport report = Build(ReadArgument(OutputPathArgument) ?? DefaultOutputPath);
            if (report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        static BuildReport Build(string outputPath)
        {
            // GitHub Pages는 압축 파일에 Content-Encoding 헤더를 붙이지 않는다.
            // 압축 해제 폴백을 켜야 브라우저가 .gz 파일을 직접 풀어서 실행한다.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"WebGL 빌드 {report.summary.result}: {outputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
            return report;
        }

        static string ReadArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return null;
        }
    }
}
