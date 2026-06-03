#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CodexAndroidBuild
{
    private const string DefaultOutputPath = "Builds/Android/TheLamentingGeese-1.3-android-arm64.apk";
    private const string StatusPath = "Builds/Android/CodexAndroidBuild.status.txt";

    [MenuItem("Codex/Build Android APK")]
    public static void BuildApk()
    {
        string outputPath = Environment.GetEnvironmentVariable("CODEX_ANDROID_OUTPUT");
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = DefaultOutputPath;
        }

        outputPath = outputPath.Replace('\\', '/');
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string statusFile = Path.Combine(projectRoot, StatusPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(statusFile));

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            scenes = new[] { "Assets/Scenes/level0.unity" };
        }

        WriteStatus(statusFile, "running", outputPath, scenes, null);

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new Exception("Failed to switch active build target to Android.");
            }
        }

        CodexAssetBundleBuild.EnsureMobileAssetBundleRuntimeSetupFromCommandLine();

        AssetDatabase.ImportAsset(
            "Assets/Resources/fonts & materials/LiberationSans SDF - Fallback.asset",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(
            "Assets/Resources/fonts & materials/LiberationSans SDF Material.mat",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        EditorUserBuildSettings.buildAppBundle = false;

        if (PlayerSettings.Android.bundleVersionCode <= 0)
        {
            PlayerSettings.Android.bundleVersionCode = 1;
        }

        if (PlayerSettings.Android.targetArchitectures == AndroidArchitecture.None)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        string applicationId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        if (string.IsNullOrEmpty(applicationId) || applicationId.Any(char.IsUpper))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zerocreations.thelamentinggeese");
        }

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;

        Debug.LogFormat(
            "CODEX_ANDROID_BUILD_START path={0} scenes={1} appId={2} version={3} versionCode={4} backend={5} arch={6} insecureHttp={7}",
            outputPath,
            string.Join(",", scenes),
            PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
            PlayerSettings.bundleVersion,
            PlayerSettings.Android.bundleVersionCode,
            PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android),
            PlayerSettings.Android.targetArchitectures,
            PlayerSettings.insecureHttpOption);

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        });

        BuildSummary summary = report.summary;
        WriteStatus(statusFile, summary.result.ToString().ToLowerInvariant(), outputPath, scenes, report);
        Debug.LogFormat(
            "CODEX_ANDROID_BUILD_RESULT result={0} path={1} size={2} totalErrors={3} totalWarnings={4} time={5}",
            summary.result,
            summary.outputPath,
            summary.totalSize,
            summary.totalErrors,
            summary.totalWarnings,
            summary.totalTime);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Android APK build failed: " + summary.result);
        }
    }

    private static void WriteStatus(string statusFile, string state, string outputPath, string[] scenes, BuildReport report)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("state=" + state);
        builder.AppendLine("updated=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        builder.AppendLine("output=" + outputPath);
        builder.AppendLine("scenes=" + string.Join(",", scenes));
        builder.AppendLine("appId=" + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
        builder.AppendLine("version=" + PlayerSettings.bundleVersion);
        builder.AppendLine("versionCode=" + PlayerSettings.Android.bundleVersionCode);
        builder.AppendLine("backend=" + PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));
        builder.AppendLine("arch=" + PlayerSettings.Android.targetArchitectures);
        builder.AppendLine("insecureHttpOption=" + PlayerSettings.insecureHttpOption);

        if (report != null)
        {
            BuildSummary summary = report.summary;
            builder.AppendLine("result=" + summary.result);
            builder.AppendLine("summaryOutput=" + summary.outputPath);
            builder.AppendLine("totalSize=" + summary.totalSize);
            builder.AppendLine("totalErrors=" + summary.totalErrors);
            builder.AppendLine("totalWarnings=" + summary.totalWarnings);
            builder.AppendLine("totalTime=" + summary.totalTime);

            foreach (BuildStep step in report.steps)
            {
                builder.AppendLine("step=" + step.name + " duration=" + step.duration);
                foreach (BuildStepMessage message in step.messages)
                {
                    builder.AppendLine("message=" + message.type + ": " + message.content.Replace("\r", " ").Replace("\n", " "));
                }
            }
        }

        File.WriteAllText(statusFile, builder.ToString());
    }
}
#endif
