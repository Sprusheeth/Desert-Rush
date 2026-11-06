using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DesertRush.Editor
{
    public static class DesertWindowsBuild
    {
        [MenuItem("Tools/Desert Rush/Build Windows Player")]
        public static void Build()
        {
            string[] scenes = Array.ConvertAll(
                Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled), scene => scene.path);
            if (scenes.Length != 2 || !scenes[0].EndsWith("/MainMenu.unity", StringComparison.Ordinal)
                || !scenes[1].EndsWith("/Gameplay.unity", StringComparison.Ordinal))
                throw new InvalidOperationException("Generate the MainMenu and Gameplay scenes first.");

            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Desert Rush Windows"));
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(output, "Desert Rush.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build ended with " + report.summary.result);
            Debug.Log("Desert Rush Windows player built at " + report.summary.outputPath);
        }
    }
}
