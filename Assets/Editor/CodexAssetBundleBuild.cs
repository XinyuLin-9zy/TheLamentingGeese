#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Utage;

public static class CodexAssetBundleBuild
{
	const string ResourcesStarvelingPath = "Assets/Resources/starveling";
	const string RemoteStarvelingPath = "Assets/RemoteAssets/starveling";
	const string UtageSampleResourcesPath = "Assets/Utage/Sample/Resources";
	const string UtageSampleRemoteResourcesPath = "Assets/Utage/Sample/RemoteResources";
	const string OutputRoot = "BuildAssetBundles";
	const string Level0ScenePath = "Assets/Scenes/level0.unity";
	const string ServerUrlResourceRoot = "Assets/Resources";
	const string ServerUrlResourceName = "CodexAssetBundleServerUrl";
	const string DefaultVersionRoot = "theweepingswan/1.0.0";
	static readonly BuildTarget[] MobileBuildTargets = { BuildTarget.Android, BuildTarget.iOS };

	[MenuItem("Codex/AssetBundles/Prepare Remote Resource Layout")]
	public static void PrepareRemoteResourceLayoutMenu()
	{
		PrepareRemoteResourceLayout();
	}

	[MenuItem("Codex/AssetBundles/Build Android and iOS")]
	public static void BuildAndroidAndIOSMenu()
	{
		BuildAndroidAndIOS();
	}

	[MenuItem("Codex/AssetBundles/Build iOS")]
	public static void BuildIOSMenu()
	{
		BuildIOS();
	}

	[MenuItem("Codex/AssetBundles/Build Editor Host")]
	public static void BuildEditorHostMenu()
	{
		BuildEditorHost();
	}

	[MenuItem("Codex/AssetBundles/Prepare Layout and Build Android and iOS")]
	public static void PrepareLayoutAndBuildAndroidAndIOSMenu()
	{
		PrepareRemoteResourceLayout();
		BuildAndroidAndIOS();
	}

	[MenuItem("Codex/AssetBundles/Prepare Layout and Build Editor Host")]
	public static void PrepareLayoutAndBuildEditorHostMenu()
	{
		PrepareRemoteResourceLayout();
		BuildEditorHost();
	}

	public static void PrepareRemoteResourceLayout()
	{
		EnsureFolder("Assets", "RemoteAssets");
		MoveAssetFolderPreserveGuid(ResourcesStarvelingPath, RemoteStarvelingPath);
		MoveAssetFolderPreserveGuid(UtageSampleResourcesPath, UtageSampleRemoteResourcesPath);
		AssetDatabase.Refresh();
	}

	public static void BuildAndroidAndIOS()
	{
		PrepareRemoteResourceLayout();
		EnsureServerUrlResources();
		EnsureBootstrapInLevel0Scene();

		AssetBundleBuild[] builds = CreateUtageResourceBuilds();
		if (builds.Length == 0)
		{
			throw new InvalidOperationException("No remote UTAGE assets were found under " + RemoteStarvelingPath);
		}

		List<string> builtPlatforms = new List<string>();
		List<string> skippedPlatforms = new List<string>();
		bool requireAllTargets = RequireAllAssetBundleTargets();
		foreach (BuildTarget target in MobileBuildTargets)
		{
			string platformName = GetPlatformName(target);
			if (!IsBuildTargetSupported(target))
			{
				string message = "Build target is not supported by this Unity installation. Install the Unity " + platformName + " Build Support module to build this AssetBundle platform.";
				if (requireAllTargets)
				{
					throw new InvalidOperationException(platformName + ": " + message);
				}

				skippedPlatforms.Add(platformName);
				Debug.LogWarningFormat("CODEX_ASSET_BUNDLE_PLATFORM_SKIPPED platform={0} reason=\"{1}\"", platformName, message);
				continue;
			}

			BuildForTarget(target, builds);
			builtPlatforms.Add(platformName);
		}
		if (builtPlatforms.Count == 0)
		{
			throw new InvalidOperationException("No AssetBundle target platforms were built.");
		}

		AssetDatabase.RemoveUnusedAssetBundleNames();
		Debug.LogFormat(
			"CODEX_ASSET_BUNDLE_BUILD_DONE bundles={0} builtPlatforms={1} skippedPlatforms={2}",
			builds.Length,
			string.Join(",", builtPlatforms.ToArray()),
			string.Join(",", skippedPlatforms.ToArray()));
	}

	public static void BuildIOS()
	{
		BuildSingleMobileTarget(BuildTarget.iOS);
	}

	public static void BuildEditorHost()
	{
		PrepareRemoteResourceLayout();
		EnsureServerUrlResources();
		EnsureBootstrapInLevel0Scene();

		AssetBundleBuild[] builds = CreateUtageResourceBuilds();
		if (builds.Length == 0)
		{
			throw new InvalidOperationException("No remote UTAGE assets were found under " + RemoteStarvelingPath);
		}

		BuildTarget target = GetEditorHostBuildTarget();
		if (!IsBuildTargetSupported(target))
		{
			throw new InvalidOperationException("Build target is not supported by this Unity installation: " + target);
		}

		BuildForTarget(target, builds);
		AssetDatabase.RemoveUnusedAssetBundleNames();
		Debug.LogFormat("CODEX_ASSET_BUNDLE_BUILD_EDITOR_HOST_DONE platform={0} bundles={1}", GetPlatformName(target), builds.Length);
	}

	public static void PrepareRemoteResourceLayoutFromCommandLine()
	{
		PrepareRemoteResourceLayout();
	}

	public static void BuildAndroidAndIOSFromCommandLine()
	{
		BuildAndroidAndIOS();
	}

	public static void BuildIOSFromCommandLine()
	{
		BuildIOS();
	}

	public static void BuildEditorHostFromCommandLine()
	{
		BuildEditorHost();
	}

	public static void EnsureMobileAssetBundleRuntimeSetupFromCommandLine()
	{
		PrepareRemoteResourceLayout();
		EnsureServerUrlResources();
		EnsureBootstrapInLevel0Scene();
	}

	public static void WriteServerUrlResourcesFromEnvironment()
	{
		EnsureServerUrlResources();
	}

	public static string ResolveServerUrlForBuildTarget(BuildTarget target)
	{
		string platformUrl = Environment.GetEnvironmentVariable("CODEX_ASSET_SERVER_URL_" + GetPlatformName(target).ToUpperInvariant());
		if (!string.IsNullOrEmpty(platformUrl))
		{
			return platformUrl.Trim().TrimEnd('/');
		}

		string explicitUrl = Environment.GetEnvironmentVariable("CODEX_ASSET_SERVER_URL");
		if (!string.IsNullOrEmpty(explicitUrl))
		{
			return explicitUrl.Trim().TrimEnd('/');
		}

		string localOverrideUrl = ReadLocalServerUrlOverride(target);
		if (!string.IsNullOrEmpty(localOverrideUrl))
		{
			return localOverrideUrl;
		}

		return GetDefaultServerUrl(target);
	}

	static void BuildSingleMobileTarget(BuildTarget target)
	{
		PrepareRemoteResourceLayout();
		EnsureServerUrlResources();
		EnsureBootstrapInLevel0Scene();

		AssetBundleBuild[] builds = CreateUtageResourceBuilds();
		if (builds.Length == 0)
		{
			throw new InvalidOperationException("No remote UTAGE assets were found under " + RemoteStarvelingPath);
		}
		if (!IsBuildTargetSupported(target))
		{
			throw new InvalidOperationException("Build target is not supported by this Unity installation: " + GetPlatformName(target));
		}

		BuildForTarget(target, builds);
		AssetDatabase.RemoveUnusedAssetBundleNames();
		Debug.LogFormat("CODEX_ASSET_BUNDLE_BUILD_SINGLE_DONE platform={0} bundles={1}", GetPlatformName(target), builds.Length);
	}

	static string ReadLocalServerUrlOverride(BuildTarget target)
	{
		string projectRoot = Directory.GetParent(Application.dataPath).FullName;
		string path = Path.Combine(projectRoot, "Deploy", "asset-server-url-" + GetPlatformName(target) + ".txt");
		if (!File.Exists(path))
		{
			return "";
		}

		string serverUrl = File.ReadAllText(path).Trim().TrimEnd('/');
		if (!string.IsNullOrEmpty(serverUrl))
		{
			Debug.LogFormat("Using local AssetBundle server URL override for {0}: {1}", GetPlatformName(target), serverUrl);
		}
		return serverUrl;
	}

	static void MoveAssetFolderPreserveGuid(string sourcePath, string destinationPath)
	{
		bool sourceExists = AssetDatabase.IsValidFolder(sourcePath);
		bool destinationExists = AssetDatabase.IsValidFolder(destinationPath);
		if (!sourceExists && destinationExists)
		{
			Debug.Log("Remote resource folder already prepared: " + destinationPath);
			return;
		}
		if (!sourceExists)
		{
			Debug.Log("Source resource folder is absent, skipping: " + sourcePath);
			return;
		}
		if (destinationExists)
		{
			throw new InvalidOperationException("Both source and destination exist. Refusing to merge: " + sourcePath + " -> " + destinationPath);
		}

		string sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
		string destinationParent = Path.GetDirectoryName(destinationPath).Replace('\\', '/');
		EnsureFolderPath(destinationParent);

		string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
		if (!string.IsNullOrEmpty(error))
		{
			throw new InvalidOperationException("Failed to move " + sourcePath + " -> " + destinationPath + ": " + error);
		}

		string destinationGuid = AssetDatabase.AssetPathToGUID(destinationPath);
		if (!string.Equals(sourceGuid, destinationGuid, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("GUID changed while moving " + sourcePath + " -> " + destinationPath);
		}

		Debug.LogFormat("Moved {0} -> {1} with GUID {2}", sourcePath, destinationPath, destinationGuid);
	}

	static AssetBundleBuild[] CreateUtageResourceBuilds()
	{
		if (!AssetDatabase.IsValidFolder(RemoteStarvelingPath))
		{
			throw new DirectoryNotFoundException(RemoteStarvelingPath);
		}

		List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
		foreach (string guid in AssetDatabase.FindAssets("", new[] { RemoteStarvelingPath }))
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (AssetDatabase.IsValidFolder(assetPath)) continue;
			if (assetPath.EndsWith(".keep", StringComparison.OrdinalIgnoreCase)) continue;

			AssetImporter importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null) continue;

			string bundleName = ToUtageAssetBundleName(assetPath);
			if (!string.Equals(importer.assetBundleName, bundleName, StringComparison.Ordinal))
			{
				importer.SetAssetBundleNameAndVariant(bundleName, "");
				AssetDatabase.WriteImportSettingsIfDirty(assetPath);
			}

			builds.Add(new AssetBundleBuild
			{
				assetBundleName = bundleName,
				assetNames = new[] { assetPath }
			});
		}

		return builds
			.OrderBy(x => x.assetBundleName, StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}

	static string ToUtageAssetBundleName(string assetPath)
	{
		string normalizedRoot = RemoteStarvelingPath.TrimEnd('/') + "/";
		string normalizedPath = assetPath.Replace('\\', '/');
		if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("Asset is outside remote Starveling root: " + assetPath);
		}

		string relativePath = normalizedPath.Substring(normalizedRoot.Length);
		return Path.ChangeExtension(relativePath, ".asset").Replace('\\', '/');
	}

	static void BuildForTarget(BuildTarget target, AssetBundleBuild[] builds)
	{
		string platformName = GetPlatformName(target);
		string outputPath = Path.Combine(OutputRoot, platformName).Replace('\\', '/');
		CleanGeneratedOutput(outputPath);
		Directory.CreateDirectory(outputPath);

		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
			outputPath,
			builds,
			BuildAssetBundleOptions.ChunkBasedCompression,
			target);

		if (manifest == null)
		{
			throw new InvalidOperationException("BuildPipeline.BuildAssetBundles returned null for " + target);
		}

		ValidateManifest(outputPath, platformName, manifest);
	}

	static void ValidateManifest(string outputPath, string platformName, AssetBundleManifest manifest)
	{
		string platformManifest = Path.Combine(outputPath, platformName);
		if (!File.Exists(platformManifest))
		{
			throw new FileNotFoundException("Platform manifest is missing", platformManifest);
		}

		HashSet<string> manifestBundles = new HashSet<string>(manifest.GetAllAssetBundles(), StringComparer.OrdinalIgnoreCase);
		string[] probeAssetPaths =
		{
			FirstAssetUnder(RemoteStarvelingPath + "/texture/event"),
			FirstAssetUnder(RemoteStarvelingPath + "/texture/character"),
			FirstAssetUnder(RemoteStarvelingPath + "/sound")
		};

		foreach (string probeAssetPath in probeAssetPaths.Where(x => !string.IsNullOrEmpty(x)))
		{
			string expectedBundle = ToUtageAssetBundleName(probeAssetPath);
			if (!manifestBundles.Contains(expectedBundle))
			{
				throw new InvalidOperationException("Manifest does not contain expected UTAGE bundle: " + expectedBundle);
			}
		}
		foreach (CodexUtageAssetBundleAliases.Entry alias in CodexUtageAssetBundleAliases.Entries)
		{
			if (!manifestBundles.Contains(alias.SourceBundleName))
			{
				throw new InvalidOperationException("Manifest does not contain expected UTAGE alias source bundle: " + alias.SourceBundleName);
			}
		}

		Debug.LogFormat("CODEX_ASSET_BUNDLE_PLATFORM_READY platform={0} output={1} bundles={2}", platformName, outputPath, manifestBundles.Count);
	}

	static bool IsBuildTargetSupported(BuildTarget target)
	{
		BuildTargetGroup targetGroup = GetBuildTargetGroup(target);
		return targetGroup != BuildTargetGroup.Unknown && BuildPipeline.IsBuildTargetSupported(targetGroup, target);
	}

	static bool RequireAllAssetBundleTargets()
	{
		string value = Environment.GetEnvironmentVariable("CODEX_REQUIRE_ALL_ASSET_BUNDLE_TARGETS");
		return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
	}

	static string FirstAssetUnder(string folder)
	{
		if (!AssetDatabase.IsValidFolder(folder)) return "";
		return AssetDatabase.FindAssets("", new[] { folder })
			.Select(AssetDatabase.GUIDToAssetPath)
			.FirstOrDefault(path => !AssetDatabase.IsValidFolder(path));
	}

	static void EnsureBootstrapInLevel0Scene()
	{
		UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
		if (!activeScene.IsValid() || activeScene.path != Level0ScenePath)
		{
			EditorSceneManager.OpenScene(Level0ScenePath, OpenSceneMode.Single);
		}

		CodexAssetBundleServerBootstrap bootstrap = UnityEngine.Object.FindObjectOfType<CodexAssetBundleServerBootstrap>(true);
		if (bootstrap == null)
		{
			GameObject go = new GameObject("CodexAssetBundleServerBootstrap");
			bootstrap = go.AddComponent<CodexAssetBundleServerBootstrap>();
		}

		SerializedObject serialized = new SerializedObject(bootstrap);
		serialized.FindProperty("starter").objectReferenceValue = UnityEngine.Object.FindObjectOfType<AdvEngineStarter>(true);
		serialized.FindProperty("configureInEditor").boolValue = true;
		serialized.FindProperty("configureInPlayer").boolValue = true;
		serialized.ApplyModifiedPropertiesWithoutUndo();

		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
	}

	static void EnsureServerUrlResources()
	{
		EnsureFolderPath(ServerUrlResourceRoot);
		WriteServerUrlResource("Android", ResolveServerUrlForBuildTarget(BuildTarget.Android));
		WriteServerUrlResource("iOS", ResolveServerUrlForBuildTarget(BuildTarget.iOS));
		WriteServerUrlResource("Windows", GetDefaultServerUrl(BuildTarget.StandaloneWindows64));
		WriteServerUrlResource("OSX", GetDefaultServerUrl(BuildTarget.StandaloneOSX));
	}

	static void WriteServerUrlResource(string platformName, string serverUrl)
	{
		string path = Path.Combine(ServerUrlResourceRoot, ServerUrlResourceName + "_" + platformName + ".txt").Replace('\\', '/');
		string normalizedUrl = serverUrl.Trim().TrimEnd('/');
		if (File.Exists(path) && File.ReadAllText(path).Trim() == normalizedUrl)
		{
			return;
		}
		File.WriteAllText(path, normalizedUrl + Environment.NewLine);
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		Debug.LogFormat("Wrote {0}: {1}", path, normalizedUrl);
	}

	static string GetDefaultServerUrl(BuildTarget target)
	{
		switch (target)
		{
			case BuildTarget.Android:
				return "http://10.0.2.2:8000/" + DefaultVersionRoot;
			case BuildTarget.iOS:
			case BuildTarget.StandaloneOSX:
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
			default:
				return "http://127.0.0.1:8000/" + DefaultVersionRoot;
		}
	}

	static string GetPlatformName(BuildTarget target)
	{
		switch (target)
		{
			case BuildTarget.Android:
				return "Android";
			case BuildTarget.iOS:
				return "iOS";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "Windows";
			case BuildTarget.StandaloneOSX:
				return "OSX";
			default:
				return target.ToString();
		}
	}

	static BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
	{
		switch (target)
		{
			case BuildTarget.Android:
				return BuildTargetGroup.Android;
			case BuildTarget.iOS:
				return BuildTargetGroup.iOS;
			case BuildTarget.StandaloneOSX:
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return BuildTargetGroup.Standalone;
			default:
				return BuildTargetGroup.Unknown;
		}
	}

	static BuildTarget GetEditorHostBuildTarget()
	{
#if UNITY_EDITOR_OSX
		return BuildTarget.StandaloneOSX;
#elif UNITY_EDITOR_WIN
		return BuildTarget.StandaloneWindows64;
#else
		return EditorUserBuildSettings.activeBuildTarget;
#endif
	}

	static void CleanGeneratedOutput(string outputPath)
	{
		string projectRoot = Directory.GetParent(Application.dataPath).FullName;
		string fullOutputPath = Path.GetFullPath(Path.Combine(projectRoot, outputPath));
		string fullOutputRoot = Path.GetFullPath(Path.Combine(projectRoot, OutputRoot));
		if (!fullOutputPath.StartsWith(fullOutputRoot, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Refusing to clean outside " + OutputRoot + ": " + fullOutputPath);
		}
		if (Directory.Exists(fullOutputPath))
		{
			Directory.Delete(fullOutputPath, true);
		}
	}

	static void EnsureFolder(string parent, string child)
	{
		string path = parent.TrimEnd('/') + "/" + child;
		if (!AssetDatabase.IsValidFolder(path))
		{
			AssetDatabase.CreateFolder(parent, child);
		}
	}

	static void EnsureFolderPath(string path)
	{
		string normalized = path.Replace('\\', '/').Trim('/');
		string[] parts = normalized.Split('/');
		if (parts.Length == 0 || parts[0] != "Assets")
		{
			throw new ArgumentException("Unity asset folder must be under Assets: " + path);
		}

		string current = "Assets";
		for (int i = 1; i < parts.Length; i++)
		{
			string next = current + "/" + parts[i];
			if (!AssetDatabase.IsValidFolder(next))
			{
				AssetDatabase.CreateFolder(current, parts[i]);
			}
			current = next;
		}
	}

}
#endif
