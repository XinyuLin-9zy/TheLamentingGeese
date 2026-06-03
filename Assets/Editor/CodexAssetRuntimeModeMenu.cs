#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Utage;

public static class CodexAssetRuntimeModeMenu
{
	const string EditorLocalPath = "Codex/Asset Runtime Mode/Editor Local Assets";
	const string RemotePath = "Codex/Asset Runtime Mode/Remote AssetBundles";
	const string AndroidTargetPath = "Codex/Asset Runtime Mode/Remote Target/Android";
	const string IosTargetPath = "Codex/Asset Runtime Mode/Remote Target/iOS";
	const string ActiveBuildTargetPath = "Codex/Asset Runtime Mode/Remote Target/Use Active Build Target";
	const string PrintPath = "Codex/Asset Runtime Mode/Print Current Mode";

	[MenuItem(EditorLocalPath, false, 0)]
	static void SetEditorLocalAssets()
	{
		CodexAssetRuntimeModeConfig.SetEditorMode(CodexAssetRuntimeMode.EditorLocalAssets);
		Debug.Log("CODEX_ASSET_RUNTIME_MODE_SET mode=EditorLocalAssets");
	}

	[MenuItem(EditorLocalPath, true)]
	static bool ValidateEditorLocalAssets()
	{
		Menu.SetChecked(EditorLocalPath, CodexAssetRuntimeModeConfig.GetActiveMode() == CodexAssetRuntimeMode.EditorLocalAssets);
		return true;
	}

	[MenuItem(RemotePath, false, 1)]
	static void SetRemoteAssetBundles()
	{
		CodexAssetRuntimeModeConfig.SetEditorMode(CodexAssetRuntimeMode.RemoteAssetBundles);
		Debug.LogFormat("CODEX_ASSET_RUNTIME_MODE_SET mode=RemoteAssetBundles target={0}", AssetBundleHelper.RuntimeAssetBundleTarget());
	}

	[MenuItem(RemotePath, true)]
	static bool ValidateRemoteAssetBundles()
	{
		Menu.SetChecked(RemotePath, CodexAssetRuntimeModeConfig.GetActiveMode() == CodexAssetRuntimeMode.RemoteAssetBundles);
		return true;
	}

	[MenuItem(AndroidTargetPath, false, 20)]
	static void SetRemoteTargetAndroid()
	{
		AssetBundleHelper.SetEditorAssetBundleTarget(AssetBundleTargetFlags.Android);
		Debug.Log("CODEX_ASSET_BUNDLE_EDITOR_TARGET_SET target=Android");
	}

	[MenuItem(AndroidTargetPath, true)]
	static bool ValidateRemoteTargetAndroid()
	{
		Menu.SetChecked(AndroidTargetPath, AssetBundleHelper.RuntimeAssetBundleTarget() == AssetBundleTargetFlags.Android);
		return true;
	}

	[MenuItem(IosTargetPath, false, 21)]
	static void SetRemoteTargetIos()
	{
		AssetBundleHelper.SetEditorAssetBundleTarget(AssetBundleTargetFlags.iOS);
		Debug.Log("CODEX_ASSET_BUNDLE_EDITOR_TARGET_SET target=iOS");
	}

	[MenuItem(IosTargetPath, true)]
	static bool ValidateRemoteTargetIos()
	{
		Menu.SetChecked(IosTargetPath, AssetBundleHelper.RuntimeAssetBundleTarget() == AssetBundleTargetFlags.iOS);
		return true;
	}

	[MenuItem(ActiveBuildTargetPath, false, 22)]
	static void UseActiveBuildTarget()
	{
		AssetBundleHelper.ClearEditorAssetBundleTarget();
		Debug.LogFormat("CODEX_ASSET_BUNDLE_EDITOR_TARGET_SET target=ActiveBuildTarget resolved={0}", AssetBundleHelper.RuntimeAssetBundleTarget());
	}

	[MenuItem(ActiveBuildTargetPath, true)]
	static bool ValidateUseActiveBuildTarget()
	{
		Menu.SetChecked(ActiveBuildTargetPath, !AssetBundleHelper.HasEditorAssetBundleTargetOverride());
		return true;
	}

	[MenuItem(PrintPath, false, 40)]
	static void PrintCurrentMode()
	{
		Debug.LogFormat(
			"CODEX_ASSET_RUNTIME_MODE_CURRENT mode={0} remoteTarget={1} targetOverride={2}",
			CodexAssetRuntimeModeConfig.GetActiveMode(),
			AssetBundleHelper.RuntimeAssetBundleTarget(),
			AssetBundleHelper.HasEditorAssetBundleTargetOverride());
	}
}
#endif
