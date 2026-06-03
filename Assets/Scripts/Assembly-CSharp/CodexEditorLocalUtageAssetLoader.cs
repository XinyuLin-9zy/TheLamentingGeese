#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utage;

public static class CodexEditorLocalUtageAssetLoader
{
	const string RequestRoot = "Starveling";
	const string RemoteAssetRoot = "Assets/RemoteAssets/starveling";

	static readonly Dictionary<string, string> resolvedPathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public static void Register()
	{
		resolvedPathCache.Clear();
		CustomLoadManager customLoadManager = AssetFileManager.GetCustomLoadManager();
		customLoadManager.OnFindAsset -= FindAsset;
		customLoadManager.OnFindAsset += FindAsset;
		Debug.LogFormat("CODEX_EDITOR_LOCAL_ASSET_LOADER root={0}", RemoteAssetRoot);
	}

	static void FindAsset(AssetFileManager manager, AssetFileInfo fileInfo, IAssetFileSettingData settingData, ref AssetFileBase asset)
	{
		string assetPath;
		if (!TryResolveAssetPath(fileInfo.FileName, out assetPath))
		{
			return;
		}

		asset = new CodexEditorLocalUtageAssetFile(manager, fileInfo, settingData, assetPath);
	}

	static bool TryResolveAssetPath(string fileName, out string assetPath)
	{
		assetPath = "";
		if (string.IsNullOrEmpty(fileName) || FilePathUtil.IsAbsoluteUri(fileName))
		{
			return false;
		}

		string normalized = fileName.Replace('\\', '/').TrimStart('/');
		string rootPrefix = RequestRoot + "/";
		if (!normalized.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		string relativePath = normalized.Substring(rootPrefix.Length);
		if (resolvedPathCache.TryGetValue(relativePath, out assetPath))
		{
			return !string.IsNullOrEmpty(assetPath);
		}

		string aliasedPath = ApplyAlias(relativePath);
		bool resolved = TryFindRemoteAsset(aliasedPath, out assetPath);
		resolvedPathCache[relativePath] = resolved ? assetPath : "";
		return resolved;
	}

	static string ApplyAlias(string relativePath)
	{
		string bundleName = ChangeExtension(relativePath, ".asset");
		foreach (CodexUtageAssetBundleAliases.Entry alias in CodexUtageAssetBundleAliases.Entries)
		{
			if (!string.Equals(bundleName, alias.RequestBundleName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string extension = Path.GetExtension(relativePath);
			return string.IsNullOrEmpty(extension)
				? alias.SourceBundleName
				: ChangeExtension(alias.SourceBundleName, extension);
		}

		return relativePath;
	}

	static string ChangeExtension(string path, string extension)
	{
		return Path.ChangeExtension(path.Replace('\\', '/'), extension).Replace('\\', '/');
	}

	static bool TryFindRemoteAsset(string relativePath, out string assetPath)
	{
		assetPath = (RemoteAssetRoot + "/" + relativePath).Replace('\\', '/');
		if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != null)
		{
			return true;
		}

		string folder = Path.GetDirectoryName(assetPath);
		if (string.IsNullOrEmpty(folder))
		{
			folder = RemoteAssetRoot;
		}
		folder = folder.Replace('\\', '/');

		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
		if (string.IsNullOrEmpty(fileNameWithoutExtension))
		{
			return false;
		}

		string[] searchFolders = AssetDatabase.IsValidFolder(folder) ? new[] { folder } : new[] { RemoteAssetRoot };
		foreach (string guid in AssetDatabase.FindAssets(fileNameWithoutExtension, searchFolders))
		{
			string foundPath = AssetDatabase.GUIDToAssetPath(guid);
			if (AssetDatabase.IsValidFolder(foundPath))
			{
				continue;
			}
			if (!string.Equals(Path.GetFileNameWithoutExtension(foundPath), fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (AssetDatabase.GetMainAssetTypeAtPath(foundPath) == null)
			{
				continue;
			}

			assetPath = foundPath;
			return true;
		}

		assetPath = "";
		return false;
	}
}

sealed class CodexEditorLocalUtageAssetFile : AssetFileBase
{
	readonly string assetPath;

	public CodexEditorLocalUtageAssetFile(AssetFileManager manager, AssetFileInfo fileInfo, IAssetFileSettingData settingData, string assetPath)
		: base(manager, fileInfo, settingData)
	{
		this.assetPath = assetPath;
	}

	public override bool CheckCacheOrLocal()
	{
		return true;
	}

	public override IEnumerator LoadAsync(Action onComplete, Action onFailed)
	{
		IsLoadEnd = false;
		IsLoadError = false;
		LoadErrorMsg = "";

		UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(assetPath, GetLoadType());
		if (asset == null)
		{
			SetLoadError("EditorLocalAsset Load Error");
			onFailed();
			yield break;
		}

		LoadAsset(asset, onComplete, onFailed);
		yield break;
	}

	Type GetLoadType()
	{
		switch (FileType)
		{
			case AssetFileType.Text:
				return typeof(TextAsset);
			case AssetFileType.Texture:
				return typeof(Texture2D);
			case AssetFileType.Sound:
				return typeof(AudioClip);
			case AssetFileType.UnityObject:
			default:
				return typeof(UnityEngine.Object);
		}
	}

	void LoadAsset(UnityEngine.Object asset, Action onComplete, Action onFailed)
	{
		switch (FileType)
		{
			case AssetFileType.Text:
				Text = asset as TextAsset;
				if (Text == null) SetLoadError("EditorLocalAsset Type Error");
				break;
			case AssetFileType.Texture:
				Texture = asset as Texture2D;
				if (Texture == null) SetLoadError("EditorLocalAsset Type Error");
				break;
			case AssetFileType.Sound:
				Sound = asset as AudioClip;
				if (Sound == null) SetLoadError("EditorLocalAsset Type Error");
				break;
			case AssetFileType.UnityObject:
			default:
				UnityObject = asset;
				if (UnityObject == null) SetLoadError("EditorLocalAsset Type Error");
				break;
		}

		if (IsLoadError)
		{
			onFailed();
		}
		else
		{
			IsLoadEnd = true;
			onComplete();
		}
	}

	void SetLoadError(string message)
	{
		LoadErrorMsg = message + " : load from " + assetPath;
		IsLoadError = true;
	}

	public override void Unload()
	{
		Text = null;
		Texture = null;
		Sound = null;
		UnityObject = null;
		IsLoadEnd = false;
		Priority = AssetFileLoadPriority.DownloadOnly;
	}
}
#endif
