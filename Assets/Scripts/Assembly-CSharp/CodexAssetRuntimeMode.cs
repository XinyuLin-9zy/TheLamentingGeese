using System;
using UnityEngine;

public enum CodexAssetRuntimeMode
{
	EditorLocalAssets = 0,
	RemoteAssetBundles = 1,
}

public static class CodexAssetRuntimeModeConfig
{
	public const string PrefKey = "Codex.AssetRuntimeMode";

	public static CodexAssetRuntimeMode GetActiveMode()
	{
#if UNITY_EDITOR
		CodexAssetRuntimeMode commandLineMode;
		if (TryGetCommandLineMode(out commandLineMode))
		{
			return commandLineMode;
		}

		return (CodexAssetRuntimeMode)PlayerPrefs.GetInt(PrefKey, (int)CodexAssetRuntimeMode.EditorLocalAssets);
#else
		return CodexAssetRuntimeMode.RemoteAssetBundles;
#endif
	}

#if UNITY_EDITOR
	public static void SetEditorMode(CodexAssetRuntimeMode mode)
	{
		PlayerPrefs.SetInt(PrefKey, (int)mode);
		PlayerPrefs.Save();
	}

	static bool TryGetCommandLineMode(out CodexAssetRuntimeMode mode)
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			string value = "";
			string arg = args[i];
			if (arg.StartsWith("-assetRuntimeMode=", StringComparison.OrdinalIgnoreCase))
			{
				value = arg.Substring("-assetRuntimeMode=".Length);
			}
			else if (string.Equals(arg, "-assetRuntimeMode", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				value = args[i + 1];
			}

			if (!string.IsNullOrEmpty(value) && TryParseMode(value, out mode))
			{
				return true;
			}
		}

		mode = CodexAssetRuntimeMode.EditorLocalAssets;
		return false;
	}

	static bool TryParseMode(string value, out CodexAssetRuntimeMode mode)
	{
		if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "editorlocal", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "editorlocalassets", StringComparison.OrdinalIgnoreCase))
		{
			mode = CodexAssetRuntimeMode.EditorLocalAssets;
			return true;
		}
		if (string.Equals(value, "remote", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "assetbundle", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "assetbundles", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(value, "remoteassetbundles", StringComparison.OrdinalIgnoreCase))
		{
			mode = CodexAssetRuntimeMode.RemoteAssetBundles;
			return true;
		}

		mode = CodexAssetRuntimeMode.EditorLocalAssets;
		return false;
	}
#endif
}
