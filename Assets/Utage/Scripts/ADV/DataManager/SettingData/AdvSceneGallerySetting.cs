// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Utage
{

	/// <summary>
	/// シーン回想のデータ
	/// </summary>
	public class AdvSceneGallerySettingData : AdvSettingDictinoayItemBase
	{
		/// <summary>
		/// シナリオラベル
		/// </summary>
		public string ScenarioLabel { get { return this.Key; } }
		
		/// <summary>
		/// タイトル
		/// </summary>
		public string Title { get { return this.title; } }
		string title;

		/// <summary>
		/// タイトル(ローカライズ対応済み)
		/// </summary>
		public string LocalizedTitle
		{
			get
			{
				if (this.RowData == null) return Title;
				return AdvParser.ParseCellLocalizedText(this.RowData, AdvColumnName.Title);
			}
		}

		/// <summary>
		/// カテゴリ名
		/// </summary>
		public string Category { get { return this.category; } }
		string category;

		/// <summary>
		/// サムネイル用ファイル名
		/// </summary>
		string thumbnailName;

		/// <summary>
		/// サムネイル用ファイルパス
		/// </summary>
		public string ThumbnailPath { get { return this.thumbnailPath; } }
		string thumbnailPath;

		/// <summary>
		/// サムネイル用ファイルのバージョン
		/// </summary>
		public int ThumbnailVersion { get { return this.thumbnailVersion; } }
		int thumbnailVersion;

		/// <summary>
		/// StringGridの一行からデータ初期化
		/// </summary>
		/// <param name="row">初期化するためのデータ</param>
		/// <returns>成否</returns>
		public override bool InitFromStringGridRow(StringGridRow row)
		{
			string key = AdvCommandParser.ParseScenarioLabel(row, AdvColumnName.ScenarioLabel);
			InitKey(key);
			this.title = AdvParser.ParseCellOptional<string>(row, AdvColumnName.Title,"");
			this.thumbnailName = AdvParser.ParseCell<string>(row, AdvColumnName.Thumbnail);
			this.thumbnailVersion = AdvParser.ParseCellOptional<int>(row, AdvColumnName.ThumbnailVersion, 0);
			this.category = AdvParser.ParseCellOptional<string>(row, AdvColumnName.Categolly, "");
			this.RowData = row;

			return true;
		}

		public void BootInit(AdvSettingDataManager dataManager)
		{
			this.thumbnailPath = dataManager.BootSetting.ThumbnailDirInfo.FileNameToPath(thumbnailName);
		}

		internal static AdvSceneGallerySettingData CreateRuntimeFallback(string scenarioLabel, string title, string thumbnailFileName, string category, AdvSettingDataManager dataManager)
		{
			var data = new AdvSceneGallerySettingData();
			data.InitKey(scenarioLabel);
			data.title = string.IsNullOrEmpty(title) ? scenarioLabel : title;
			data.thumbnailName = thumbnailFileName;
			data.thumbnailVersion = 0;
			data.category = category;
			data.RowData = null;
			data.BootInit(dataManager);
			return data;
		}
	}

	/// <summary>
	/// シーン回想のデータ
	/// </summary>
	public class AdvSceneGallerySetting : AdvSettingDataDictinoayBase<AdvSceneGallerySettingData>
	{
		const string FallbackCategory = "\u8BB0\u5FC6\u56DE\u60F3";
		const string EndingPrefix = "\u7ED3\u5C40-";
		const string EndingTitlePrefix = "\u7ED3\u5C40\uFF1A";

		readonly struct RuntimeFallbackEntry
		{
			public RuntimeFallbackEntry(string scenarioLabel, string title, string thumbnailKey)
			{
				ScenarioLabel = scenarioLabel;
				Title = title;
				ThumbnailKey = thumbnailKey;
			}

			public string ScenarioLabel { get; }
			public string Title { get; }
			public string ThumbnailKey { get; }
		}

		static readonly RuntimeFallbackEntry[] PreferredFallbackEntries =
		{
			new RuntimeFallbackEntry("结局-寻闯", "结局：寻闯", "寻闯"),
			new RuntimeFallbackEntry("结局-出家", "结局：出家", "出家"),
			new RuntimeFallbackEntry("结局-远舟", "结局：远舟", "远舟"),
			new RuntimeFallbackEntry("结局-新朝", "结局：新朝", "新朝"),
			new RuntimeFallbackEntry("结局-哭庙", "结局：哭庙", "哭庙"),
			new RuntimeFallbackEntry("结局-通海", "结局：通海", "通海"),
			new RuntimeFallbackEntry("结局-烬志", "结局：烬志", "烬志"),
			new RuntimeFallbackEntry("结局-殉情", "结局：殉情", "桥上幻景"),
			new RuntimeFallbackEntry("聊斋", "结局：聊斋", "聊斋"),
			new RuntimeFallbackEntry("红楼", "结局：红楼", "红楼"),
		};

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		public override void BootInit( AdvSettingDataManager dataManager )
		{
			EnsureRuntimeFallback(dataManager);
			foreach (AdvSceneGallerySettingData data in List)
			{
				data.BootInit(dataManager);
			}
		}

		/// <summary>
		/// 全てのリソースをダウンロード
		/// </summary>
		public override void DownloadAll()
		{
			//ファイルマネージャーにバージョンの登録
			foreach (AdvSceneGallerySettingData data in List)
			{
				AssetFileManager.Download(data.ThumbnailPath);
			}
		}

		/// <summary>
		/// ギャラリー用のデータを取得
		/// </summary>
		/// <param name="category">カテゴリ</param>
		public List<AdvSceneGallerySettingData> CreateGalleryDataList(string category)
		{
			List<AdvSceneGallerySettingData> list = new List<AdvSceneGallerySettingData>();
			foreach (var item in List)
			{
				if (item.Category == category)
				{
					list.Add(item);
				}
			}
			return list;
		}

		/// <summary>
		/// カテゴリのリストを取得
		/// </summary>
		public List<string> CreateCategoryList()
		{
			List<string> list = new List<string>();
			foreach (var item in List)
			{
				if (string.IsNullOrEmpty(item.ThumbnailPath)) continue;
				if (!list.Contains(item.Category))
				{
					list.Add(item.Category);
				}
			}
			return list;
		}

		public bool Contains(string key)
		{
			return Dictionary.ContainsKey(key);
		}

		void EnsureRuntimeFallback(AdvSettingDataManager dataManager)
		{
			if (List.Count > 0) return;
			if (dataManager == null || dataManager.ImportedScenarios == null) return;

			Dictionary<string, string> thumbnailLookup = LoadFallbackThumbnailLookup(dataManager);
			if (thumbnailLookup.Count <= 0) return;
			HashSet<string> importedScenarioLabels = CollectImportedScenarioLabels(dataManager);
			if (TryAddPreferredFallbackEntries(dataManager, importedScenarioLabels, thumbnailLookup))
			{
				Debug.LogFormat("SceneGallery setting was empty. Rebuilt {0} curated fallback entries from imported scenario sheets and thumbnail resources.", List.Count);
				return;
			}

			foreach (AdvChapterData chapter in dataManager.ImportedScenarios.Chapters)
			{
				if (chapter == null) continue;
				foreach (AdvImportBook book in chapter.DataList)
				{
					if (book == null) continue;
					foreach (AdvImportScenarioSheet sheet in book.ImportGridList)
					{
						if (sheet == null) continue;
						string sheetName = sheet.SheetName;
						if (string.IsNullOrEmpty(sheetName)) continue;
						if (Dictionary.ContainsKey(sheetName)) continue;

						string displayTitle;
						string thumbnailRawName;
						if (!TryResolveFallbackThumbnail(sheetName, thumbnailLookup, out displayTitle, out thumbnailRawName)) continue;

						AddData(AdvSceneGallerySettingData.CreateRuntimeFallback(
							sheetName,
							displayTitle,
							thumbnailRawName + ".png",
							FallbackCategory,
							dataManager));
					}
				}
			}

			if (List.Count > 0)
			{
				Debug.LogFormat("SceneGallery setting was empty. Rebuilt {0} fallback entries from imported scenario sheets and thumbnail resources.", List.Count);
			}
		}

		HashSet<string> CollectImportedScenarioLabels(AdvSettingDataManager dataManager)
		{
			HashSet<string> labels = new HashSet<string>(StringComparer.Ordinal);
			foreach (AdvChapterData chapter in dataManager.ImportedScenarios.Chapters)
			{
				if (chapter == null) continue;
				foreach (AdvImportBook book in chapter.DataList)
				{
					if (book == null) continue;
					foreach (AdvImportScenarioSheet sheet in book.ImportGridList)
					{
						if (sheet == null || string.IsNullOrEmpty(sheet.SheetName)) continue;
						labels.Add(sheet.SheetName);
					}
				}
			}
			return labels;
		}

		bool TryAddPreferredFallbackEntries(
			AdvSettingDataManager dataManager,
			HashSet<string> importedScenarioLabels,
			Dictionary<string, string> thumbnailLookup)
		{
			if (importedScenarioLabels == null || importedScenarioLabels.Count <= 0) return false;

			int addedCount = 0;
			foreach (RuntimeFallbackEntry entry in PreferredFallbackEntries)
			{
				if (!importedScenarioLabels.Contains(entry.ScenarioLabel)) continue;
				if (!thumbnailLookup.TryGetValue(entry.ThumbnailKey, out string thumbnailRawName)) continue;

				AddData(AdvSceneGallerySettingData.CreateRuntimeFallback(
					entry.ScenarioLabel,
					entry.Title,
					thumbnailRawName + ".png",
					FallbackCategory,
					dataManager));
				++addedCount;
			}
			return addedCount > 0;
		}

		Dictionary<string, string> LoadFallbackThumbnailLookup(AdvSettingDataManager dataManager)
		{
			string resourcePath = dataManager.BootSetting.ThumbnailDirInfo.defaultDir;
			if (string.IsNullOrEmpty(resourcePath))
			{
				return new Dictionary<string, string>(StringComparer.Ordinal);
			}

			resourcePath = resourcePath.Replace('\\', '/').ToLowerInvariant();
			Texture2D[] textures = Resources.LoadAll<Texture2D>(resourcePath);
			Dictionary<string, string> names = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (Texture2D texture in textures)
			{
				if (texture == null) continue;
				if (!IsSceneGalleryFallbackThumbnail(texture.name)) continue;

				string displayName = NormalizeFallbackName(texture.name);
				if (string.IsNullOrEmpty(displayName)) continue;
				if (!names.ContainsKey(displayName))
				{
					names.Add(displayName, texture.name);
				}
			}
			return names;
		}

		static bool IsSceneGalleryFallbackThumbnail(string thumbnailName)
		{
			if (string.IsNullOrEmpty(thumbnailName)) return false;
			if (thumbnailName.StartsWith("cg", StringComparison.OrdinalIgnoreCase)) return false;
			return thumbnailName.Any(ch => ch > 127);
		}

		static string NormalizeFallbackName(string rawName)
		{
			if (string.IsNullOrEmpty(rawName)) return rawName;
			return rawName.Trim();
		}

		static bool TryResolveFallbackThumbnail(string sheetName, Dictionary<string, string> thumbnailLookup, out string displayTitle, out string thumbnailRawName)
		{
			displayTitle = NormalizeFallbackName(sheetName);
			thumbnailRawName = null;

			if (string.IsNullOrEmpty(displayTitle) || thumbnailLookup == null)
			{
				return false;
			}

			if (thumbnailLookup.TryGetValue(displayTitle, out thumbnailRawName))
			{
				return true;
			}

			if (displayTitle.StartsWith(EndingPrefix, StringComparison.Ordinal))
			{
				string suffix = displayTitle.Substring(EndingPrefix.Length).Trim();
				if (thumbnailLookup.TryGetValue(suffix, out thumbnailRawName))
				{
					displayTitle = EndingTitlePrefix + suffix;
					return true;
				}
			}

			return false;
		}
	}
}
