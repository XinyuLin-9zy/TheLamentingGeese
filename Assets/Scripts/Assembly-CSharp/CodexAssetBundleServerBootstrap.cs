using System;
using UnityEngine;
using Utage;

[DefaultExecutionOrder(-10000)]
public class CodexAssetBundleServerBootstrap : MonoBehaviour
{
	[SerializeField] AdvEngineStarter starter;
	[SerializeField] string serverUrlResourceName = "CodexAssetBundleServerUrl";
	[SerializeField] bool configureInEditor = true;
	[SerializeField] bool configureInPlayer = true;
	[SerializeField] string fallbackServerUrl = "";

	void Awake()
	{
		if ((Application.isEditor && !configureInEditor) || (!Application.isEditor && !configureInPlayer))
		{
			return;
		}

		AdvEngineStarter target = starter;
		if (target == null)
		{
			target = FindObjectOfType<AdvEngineStarter>(true);
		}
		if (target == null)
		{
			Debug.LogError("CodexAssetBundleServerBootstrap could not find AdvEngineStarter.");
			return;
		}

		CodexAssetRuntimeMode mode = CodexAssetRuntimeModeConfig.GetActiveMode();
#if UNITY_EDITOR
		if (Application.isEditor && mode == CodexAssetRuntimeMode.EditorLocalAssets)
		{
			target.Strage = AdvEngineStarter.StrageType.Local;
			CodexEditorLocalUtageAssetLoader.Register();
			Debug.LogFormat(
				"CODEX_ASSET_RUNTIME_MODE mode={0} strage={1} rootResourceDir={2}",
				mode,
				target.Strage,
				target.RootResourceDir);
			return;
		}
#endif

		AssetBundleTargetFlags assetBundleTarget = AssetBundleHelper.RuntimeAssetBundleTarget();
		string serverUrl = ResolveServerUrl(target.ServerUrl, assetBundleTarget);
		if (string.IsNullOrEmpty(serverUrl))
		{
			Debug.LogError("AssetBundle ServerUrl is empty. Set Resources/CodexAssetBundleServerUrl_<Platform>.txt or CODEX_ASSET_SERVER_URL before building.");
			return;
		}

		target.Strage = AdvEngineStarter.StrageType.ServerAndLocalScenario;
		target.ServerUrl = serverUrl.TrimEnd('/');
		bool bootDownloadScreen = ConfigureBootDownloadScreen();
		if (bootDownloadScreen)
		{
			target.IsLoadOnAwake = false;
		}
		RegisterAssetBundleAliases(target);
		Debug.LogFormat(
			"CODEX_ASSET_SERVER_BOOTSTRAP mode={0} platform={1} assetBundleTarget={2} strage={3} loadOnAwake={4} serverUrl={5}",
			mode,
			Application.platform,
			assetBundleTarget,
			target.Strage,
			target.IsLoadOnAwake,
			target.ServerUrl);
	}

	string ResolveServerUrl(string currentServerUrl, AssetBundleTargetFlags target)
	{
		string url = GetCommandLineServerUrl();
		if (!string.IsNullOrEmpty(url)) return url;

		string platformSuffix = GetPlatformSuffix(target);
		url = Environment.GetEnvironmentVariable("CODEX_ASSET_SERVER_URL_" + platformSuffix.ToUpperInvariant());
		if (!string.IsNullOrEmpty(url)) return url;

		url = Environment.GetEnvironmentVariable("CODEX_ASSET_SERVER_URL");
		if (!string.IsNullOrEmpty(url)) return url;

#if UNITY_EDITOR
		string editorHostPlatformSuffix = GetEditorHostPlatformSuffix();
		if (!string.IsNullOrEmpty(editorHostPlatformSuffix))
		{
			url = LoadServerUrlResource(serverUrlResourceName + "_" + editorHostPlatformSuffix);
			if (!string.IsNullOrEmpty(url)) return url;
		}
#endif

		url = LoadServerUrlResource(serverUrlResourceName + "_" + platformSuffix);
		if (!string.IsNullOrEmpty(url)) return url;

		url = LoadServerUrlResource(serverUrlResourceName);
		if (!string.IsNullOrEmpty(url)) return url;

		if (!string.IsNullOrEmpty(fallbackServerUrl)) return fallbackServerUrl;
		return currentServerUrl;
	}

	void RegisterAssetBundleAliases(AdvEngineStarter target)
	{
		string rootUrl = FilePathUtil.Combine(target.ServerUrl, AssetBundleHelper.RuntimeAssetBundleTarget().ToString());
		AssetBundleInfoManager manager = AssetFileManager.GetInstance().AssetBundleInfoManager;
		int count = 0;
		foreach (CodexUtageAssetBundleAliases.Entry alias in CodexUtageAssetBundleAliases.Entries)
		{
			string requestPath = FilePathUtil.Combine(rootUrl, alias.RequestBundleName);
			if (manager.FindAssetBundleInfo(requestPath) != null)
			{
				continue;
			}

			string sourceUrl = FilePathUtil.Combine(rootUrl, alias.SourceBundleName);
			manager.AddAssetBundleInfo(requestPath, sourceUrl);
			++count;
			Debug.LogFormat("CODEX_ASSET_BUNDLE_ALIAS_MAP request={0} source={1}", requestPath, sourceUrl);
		}
		Debug.LogFormat("CODEX_ASSET_BUNDLE_ALIAS_MAP_DONE count={0}", count);
	}

	string LoadServerUrlResource(string resourceName)
	{
		TextAsset text = Resources.Load<TextAsset>(resourceName);
		return text == null ? "" : text.text.Trim();
	}

	string GetCommandLineServerUrl()
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i];
			if (arg.StartsWith("-assetServerUrl=", StringComparison.OrdinalIgnoreCase))
			{
				return arg.Substring("-assetServerUrl=".Length).Trim();
			}
			if (string.Equals(arg, "-assetServerUrl", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				return args[i + 1].Trim();
			}
		}
		return "";
	}

	bool ConfigureBootDownloadScreen()
	{
		bool hasBootDownloadScreen = false;
		foreach (UtageUguiBoot boot in Resources.FindObjectsOfTypeAll<UtageUguiBoot>())
		{
			if (boot != null && boot.gameObject.scene.IsValid() && boot.loadWait != null)
			{
				boot.isWaitDownLoad = true;
				hasBootDownloadScreen = true;
			}
		}
		return hasBootDownloadScreen;
	}

	string GetPlatformSuffix(AssetBundleTargetFlags target)
	{
		switch (target)
		{
			case AssetBundleTargetFlags.Android:
				return "Android";
			case AssetBundleTargetFlags.iOS:
				return "iOS";
			case AssetBundleTargetFlags.Windows:
				return "Windows";
			case AssetBundleTargetFlags.OSX:
				return "OSX";
			default:
				return target.ToString();
		}
	}

#if UNITY_EDITOR
	string GetEditorHostPlatformSuffix()
	{
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
				return "Windows";
			case RuntimePlatform.OSXEditor:
				return "OSX";
			default:
				return "";
		}
	}
#endif
}
