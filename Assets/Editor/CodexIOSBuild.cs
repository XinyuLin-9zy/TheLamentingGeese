#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CodexIOSBuild
{
    private const string DefaultOutputPath = "Builds/iOS/TheLamentingGeese-iOS";
    private const string StatusPath = "Builds/iOS/CodexIOSBuild.status.txt";

    [MenuItem("Codex/Build iOS Xcode Project")]
    public static void BuildXcodeProject()
    {
        string outputPath = Environment.GetEnvironmentVariable("CODEX_IOS_OUTPUT");
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = DefaultOutputPath;
        }

        outputPath = outputPath.Replace('\\', '/').TrimEnd('/');
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string fullOutputPath = Path.GetFullPath(Path.Combine(projectRoot, outputPath));
        CleanGeneratedOutput(projectRoot, fullOutputPath);
        Directory.CreateDirectory(fullOutputPath);

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

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                throw new Exception("Failed to switch active build target to iOS.");
            }
        }

        CodexAssetBundleBuild.EnsureMobileAssetBundleRuntimeSetupFromCommandLine();

        string applicationId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
        if (!IsValidBundleIdentifier(applicationId))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.zerocreations.thelamentinggeese");
        }

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        CodexIOSAppIcon.Apply();

        Debug.LogFormat(
            "CODEX_IOS_BUILD_START path={0} scenes={1} appId={2} version={3} backend={4} sdk={5} targetDevice={6} insecureHttp={7}",
            outputPath,
            string.Join(",", scenes),
            PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS),
            PlayerSettings.bundleVersion,
            PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS),
            PlayerSettings.iOS.sdkVersion,
            PlayerSettings.iOS.targetDevice,
            PlayerSettings.insecureHttpOption);

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            targetGroup = BuildTargetGroup.iOS,
            options = BuildOptions.None
        });

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            CodexIOSAppIcon.PostprocessXcodeProject(fullOutputPath);
        }
        WriteStatus(statusFile, summary.result.ToString().ToLowerInvariant(), outputPath, scenes, report);
        Debug.LogFormat(
            "CODEX_IOS_BUILD_RESULT result={0} path={1} size={2} totalErrors={3} totalWarnings={4} time={5}",
            summary.result,
            summary.outputPath,
            summary.totalSize,
            summary.totalErrors,
            summary.totalWarnings,
            summary.totalTime);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception("iOS Xcode project build failed: " + summary.result);
        }
    }

    private static bool IsValidBundleIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains(".") || value.Any(char.IsUpper))
        {
            return false;
        }

        foreach (string segment in value.Split('.'))
        {
            if (string.IsNullOrEmpty(segment)) return false;
            foreach (char c in segment)
            {
                bool valid = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-';
                if (!valid) return false;
            }
        }
        return true;
    }

    private static void CleanGeneratedOutput(string projectRoot, string fullOutputPath)
    {
        string fullBuildsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Builds"));
        if (!fullOutputPath.StartsWith(fullBuildsRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to clean outside Builds: " + fullOutputPath);
        }
        if (Directory.Exists(fullOutputPath))
        {
            Directory.Delete(fullOutputPath, true);
        }
    }

    private static void WriteStatus(string statusFile, string state, string outputPath, string[] scenes, BuildReport report)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("state=" + state);
        builder.AppendLine("updated=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        builder.AppendLine("output=" + outputPath);
        builder.AppendLine("scenes=" + string.Join(",", scenes));
        builder.AppendLine("appId=" + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
        builder.AppendLine("version=" + PlayerSettings.bundleVersion);
        builder.AppendLine("backend=" + PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS));
        builder.AppendLine("sdk=" + PlayerSettings.iOS.sdkVersion);
        builder.AppendLine("targetDevice=" + PlayerSettings.iOS.targetDevice);
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
