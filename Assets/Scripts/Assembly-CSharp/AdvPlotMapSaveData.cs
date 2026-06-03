using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utage;

public class AdvPlotMapSaveData : IBinaryIO
{
	private const int VERSION = 0;

	private List<string> unlockPlots;

	public Dictionary<string, int> plotKeyChaCountDict;

	public int totalNumber;

	public string SaveKey => "AdvPlotMapSaveData";

	public void AddPlotKey(string plotKey)
	{
		EnsureData();
		plotKey = PlotMapProgressStore.Normalize(plotKey);
		if (string.IsNullOrEmpty(plotKey)) return;

		if (!unlockPlots.Contains(plotKey))
		{
			unlockPlots.Add(plotKey);
		}
		PlotMapProgressStore.Unlock(plotKey);
	}

	public void ClearAllPLotKey()
	{
		EnsureData();
		unlockPlots.Clear();
		PlotMapProgressStore.ClearAll();
	}

	public bool CheckPlotKey(string plotKey)
	{
		EnsureData();
		plotKey = PlotMapProgressStore.Normalize(plotKey);
		return string.IsNullOrEmpty(plotKey) || unlockPlots.Contains(plotKey) || PlotMapProgressStore.IsUnlocked(plotKey);
	}

	public void RegisterChapterNumData(string plotKey, int chaCount)
	{
		EnsureData();
		plotKey = PlotMapProgressStore.Normalize(plotKey);
		if (string.IsNullOrEmpty(plotKey)) return;

		plotKeyChaCountDict[plotKey] = chaCount;
		totalNumber = Mathf.Max(totalNumber, plotKeyChaCountDict.Count);
	}

	public bool CheckKeyListUnlocked(List<string> targetKeyList)
	{
		if (targetKeyList == null || targetKeyList.Count == 0) return true;
		foreach (string key in targetKeyList)
		{
			if (!CheckPlotKey(key)) return false;
		}
		return true;
	}

	public float GetUnlockProcess()
	{
		EnsureData();
		int total = totalNumber > 0 ? totalNumber : plotKeyChaCountDict.Count;
		if (total <= 0) return 1f;
		return Mathf.Clamp01((float)unlockPlots.Count / total);
	}

	public void OnWrite(BinaryWriter writer)
	{
		EnsureData();
		writer.Write(VERSION);
		writer.Write(unlockPlots.Count);
		foreach (string plot in unlockPlots)
		{
			writer.Write(plot ?? "");
		}

		writer.Write(plotKeyChaCountDict.Count);
		foreach (KeyValuePair<string, int> pair in plotKeyChaCountDict)
		{
			writer.Write(pair.Key ?? "");
			writer.Write(pair.Value);
		}
		writer.Write(totalNumber);
	}

	public void OnRead(BinaryReader reader)
	{
		EnsureData();
		unlockPlots.Clear();
		plotKeyChaCountDict.Clear();

		reader.ReadInt32();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; ++i)
		{
			AddPlotKey(reader.ReadString());
		}

		if (reader.BaseStream.Position >= reader.BaseStream.Length) return;
		int chapterCount = reader.ReadInt32();
		for (int i = 0; i < chapterCount; ++i)
		{
			RegisterChapterNumData(reader.ReadString(), reader.ReadInt32());
		}

		if (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			totalNumber = reader.ReadInt32();
		}
	}

	private void EnsureData()
	{
		if (unlockPlots == null) unlockPlots = new List<string>();
		if (plotKeyChaCountDict == null) plotKeyChaCountDict = new Dictionary<string, int>();
	}
}

internal static class PlotMapProgressStore
{
	private const string UnlockPrefix = "PlotMap.Unlock.";
	private const string IndexKey = "PlotMap.Unlock.Index";
	private const string ChapterNameKey = "PlotMap.ChapterName";

	private static HashSet<string> cachedKeys;

	public static string CurrentChapterName
	{
		get => PlayerPrefs.GetString(ChapterNameKey, "");
		set => PlayerPrefs.SetString(ChapterNameKey, value ?? "");
	}

	public static string Normalize(string key)
	{
		return string.IsNullOrWhiteSpace(key) ? "" : key.Trim().TrimStart('*');
	}

	public static bool HasAnyUnlock()
	{
		EnsureIndex();
		return cachedKeys.Count > 0;
	}

	public static void Unlock(string key)
	{
		key = Normalize(key);
		if (string.IsNullOrEmpty(key)) return;

		EnsureIndex();
		if (cachedKeys.Add(key))
		{
			SaveIndex();
		}
		PlayerPrefs.SetInt(UnlockPrefix + key, 1);
		PlayerPrefs.Save();
	}

	public static bool IsUnlocked(string key)
	{
		key = Normalize(key);
		return string.IsNullOrEmpty(key) || PlayerPrefs.GetInt(UnlockPrefix + key, 0) == 1;
	}

	public static bool AreUnlocked(IList<string> keys)
	{
		if (keys == null || keys.Count == 0) return true;
		foreach (string key in keys)
		{
			if (!IsUnlocked(key)) return false;
		}
		return true;
	}

	public static bool AnyUnlocked(IList<string> keys)
	{
		if (keys == null || keys.Count == 0) return false;
		foreach (string key in keys)
		{
			string normalizedKey = Normalize(key);
			if (string.IsNullOrEmpty(normalizedKey)) continue;
			if (IsUnlocked(normalizedKey)) return true;
		}
		return false;
	}

	public static void ClearAll()
	{
		EnsureIndex();
		foreach (string key in cachedKeys)
		{
			PlayerPrefs.DeleteKey(UnlockPrefix + key);
		}
		cachedKeys.Clear();
		PlayerPrefs.DeleteKey(IndexKey);
		PlayerPrefs.Save();
	}

	private static void EnsureIndex()
	{
		if (cachedKeys != null) return;

		cachedKeys = new HashSet<string>();
		string raw = PlayerPrefs.GetString(IndexKey, "");
		if (string.IsNullOrEmpty(raw)) return;

		string[] keys = raw.Split('\n');
		foreach (string key in keys)
		{
			string normalized = Normalize(key);
			if (!string.IsNullOrEmpty(normalized))
			{
				cachedKeys.Add(normalized);
			}
		}
	}

	private static void SaveIndex()
	{
		PlayerPrefs.SetString(IndexKey, string.Join("\n", cachedKeys));
	}
}
