using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utage;

public class AdvVoiceCollectionData : IBinaryIO
{
	private const int VERSION = 0;
	private const string SaveKeyValue = "AdvVoiceCollectionData";
	private const string PlayerPrefsPayloadKey = "AdvVoiceCollectionData.Payload";

	private static readonly Dictionary<AdvEngine, AdvVoiceCollectionData> Instances = new Dictionary<AdvEngine, AdvVoiceCollectionData>();
	private static AdvVoiceCollectionData fallbackInstance;

	private AdvEngine engine;
	private bool loadedFromFallback;

	public List<AdvBacklog> collectionLogs;

	public string SaveKey => SaveKeyValue;

	public AdvEngine Engine => engine;

	public List<AdvBacklog> CollectionLogs
	{
		get
		{
			EnsureData();
			return collectionLogs;
		}
	}

	public static AdvVoiceCollectionData Get(AdvEngine engine)
	{
		AdvVoiceCollectionData data;
		if (engine == null)
		{
			if (fallbackInstance == null)
			{
				fallbackInstance = new AdvVoiceCollectionData();
				fallbackInstance.LoadFromFallbackIfNeeded();
			}
			return fallbackInstance;
		}

		if (!Instances.TryGetValue(engine, out data) || data == null)
		{
			data = new AdvVoiceCollectionData();
			data.engine = engine;
			Instances[engine] = data;
			data.LoadFromFallbackIfNeeded();
		}
		else
		{
			data.engine = engine;
			data.LoadFromFallbackIfNeeded();
		}

		EnsureSaveComponent(engine);
		return data;
	}

	public static bool ToggleCollection(AdvEngine engine, AdvBacklog log)
	{
		AdvVoiceCollectionData data = Get(engine);
		if (data == null || log == null || string.IsNullOrEmpty(log.MainVoiceFileName))
		{
			return false;
		}

		if (data.CheckLogCollected(log.MainVoiceFileName))
		{
			data.RemoveCollectionLogs(log);
			return false;
		}

		data.RegisterCollectionLogs(log);
		return true;
	}

	public static bool CheckCollected(AdvEngine engine, AdvBacklog log)
	{
		return log != null && CheckCollected(engine, log.MainVoiceFileName);
	}

	public static bool CheckCollected(AdvEngine engine, string visitKey)
	{
		return Get(engine).CheckLogCollected(visitKey);
	}

	public static List<AdvBacklog> GetCollectionLogs(AdvEngine engine)
	{
		return Get(engine).CollectionLogs;
	}

	public bool CheckLogCollected(string visitKey)
	{
		EnsureData();
		visitKey = NormalizeKey(visitKey);
		if (string.IsNullOrEmpty(visitKey)) return false;

		foreach (AdvBacklog log in collectionLogs)
		{
			if (NormalizeKey(GetLogKey(log)) == visitKey)
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterCollectionLogs(AdvBacklog log)
	{
		EnsureData();
		if (log == null || string.IsNullOrEmpty(log.MainVoiceFileName)) return;

		RemoveLogByKey(log.MainVoiceFileName, false);
		AddLog(CloneLog(log));
		Persist();
	}

	public void RemoveCollectionLogs(AdvBacklog log)
	{
		if (log == null) return;
		RemoveCollectionLogs(log.MainVoiceFileName);
	}

	public void RemoveCollectionLogs(string visitKey)
	{
		EnsureData();
		if (RemoveLogByKey(visitKey, true))
		{
			Persist();
		}
	}

	public void OnWrite(BinaryWriter writer)
	{
		WritePayload(writer);
	}

	public void OnRead(BinaryReader reader)
	{
		ReadPayload(reader);
		loadedFromFallback = true;
		SaveToFallback();
	}

	private void AddLog(AdvBacklog log)
	{
		if (log == null || string.IsNullOrEmpty(log.MainVoiceFileName)) return;
		collectionLogs.Add(log);
	}

	private bool RemoveLogByKey(string visitKey, bool ensureData)
	{
		if (ensureData) EnsureData();

		visitKey = NormalizeKey(visitKey);
		if (string.IsNullOrEmpty(visitKey)) return false;

		bool removed = false;
		for (int i = collectionLogs.Count - 1; i >= 0; --i)
		{
			if (NormalizeKey(GetLogKey(collectionLogs[i])) != visitKey) continue;
			collectionLogs.RemoveAt(i);
			removed = true;
		}
		return removed;
	}

	private void EnsureData()
	{
		if (collectionLogs == null)
		{
			collectionLogs = new List<AdvBacklog>();
		}
	}

	private void Persist()
	{
		SaveToFallback();
		if (engine != null && engine.SystemSaveData != null)
		{
			engine.SystemSaveData.Write();
		}
	}

	private void LoadFromFallbackIfNeeded()
	{
		if (loadedFromFallback) return;
		loadedFromFallback = true;
		EnsureData();

		string payload = PlayerPrefs.GetString(PlayerPrefsPayloadKey, "");
		if (string.IsNullOrEmpty(payload)) return;

		try
		{
			byte[] bytes = System.Convert.FromBase64String(payload);
			using (MemoryStream stream = new MemoryStream(bytes))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				ReadPayload(reader);
			}
		}
		catch (System.Exception exception)
		{
			Debug.LogWarning("Failed to read voice collection fallback data: " + exception.Message);
			collectionLogs.Clear();
		}
	}

	private void SaveToFallback()
	{
		EnsureData();
		try
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					WritePayload(writer);
				}
				PlayerPrefs.SetString(PlayerPrefsPayloadKey, System.Convert.ToBase64String(stream.ToArray()));
				PlayerPrefs.Save();
			}
		}
		catch (System.Exception exception)
		{
			Debug.LogWarning("Failed to write voice collection fallback data: " + exception.Message);
		}
	}

	private void WritePayload(BinaryWriter writer)
	{
		EnsureData();
		writer.Write(VERSION);
		writer.Write(collectionLogs.Count);
		foreach (AdvBacklog log in collectionLogs)
		{
			if (log == null)
			{
				writer.Write(0);
				continue;
			}

			writer.Write(log.DataList.Count);
			foreach (AdvBacklog.AdvBacklogDataInPage page in log.DataList)
			{
				writer.Write(page != null ? page.LogText ?? "" : "");
				writer.Write(page != null ? page.CharacterLabel ?? "" : "");
				writer.Write(page != null ? page.CharacterNameText ?? "" : "");
				writer.Write(page != null ? page.VoiceFileName ?? "" : "");
			}
		}
	}

	private void ReadPayload(BinaryReader reader)
	{
		EnsureData();
		collectionLogs.Clear();

		int version = reader.ReadInt32();
		if (version != VERSION)
		{
			Debug.LogWarning("Unknown voice collection data version: " + version);
			return;
		}

		int count = reader.ReadInt32();
		for (int i = 0; i < count; ++i)
		{
			AdvBacklog log = new AdvBacklog();
			int pageCount = reader.ReadInt32();
			for (int pageIndex = 0; pageIndex < pageCount; ++pageIndex)
			{
				AdvBacklog.AdvBacklogDataInPage page = new AdvBacklog.AdvBacklogDataInPage();
				SetBacklogPageValue(page, "LogText", reader.ReadString());
				SetBacklogPageValue(page, "CharacterLabel", reader.ReadString());
				SetBacklogPageValue(page, "CharacterNameText", reader.ReadString());
				SetBacklogPageValue(page, "VoiceFileName", reader.ReadString());
				log.DataList.Add(page);
			}
			AddLog(log);
		}
	}

	private static AdvBacklog CloneLog(AdvBacklog source)
	{
		AdvBacklog clone = new AdvBacklog();
		if (source == null) return clone;

		foreach (AdvBacklog.AdvBacklogDataInPage sourcePage in source.DataList)
		{
			AdvBacklog.AdvBacklogDataInPage page = new AdvBacklog.AdvBacklogDataInPage();
			SetBacklogPageValue(page, "LogText", sourcePage != null ? sourcePage.LogText : "");
			SetBacklogPageValue(page, "CharacterLabel", sourcePage != null ? sourcePage.CharacterLabel : "");
			SetBacklogPageValue(page, "CharacterNameText", sourcePage != null ? sourcePage.CharacterNameText : "");
			SetBacklogPageValue(page, "VoiceFileName", sourcePage != null ? sourcePage.VoiceFileName : "");
			clone.DataList.Add(page);
		}
		return clone;
	}

	private static void SetBacklogPageValue(AdvBacklog.AdvBacklogDataInPage page, string propertyName, string value)
	{
		if (page == null || string.IsNullOrEmpty(propertyName)) return;

		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		PropertyInfo property = typeof(AdvBacklog.AdvBacklogDataInPage).GetProperty(propertyName, flags);
		MethodInfo setter = property != null ? property.GetSetMethod(true) : null;
		if (setter != null)
		{
			setter.Invoke(page, new object[] { value ?? "" });
			return;
		}

		FieldInfo backingField = typeof(AdvBacklog.AdvBacklogDataInPage).GetField("<" + propertyName + ">k__BackingField", flags);
		if (backingField != null)
		{
			backingField.SetValue(page, value ?? "");
		}
	}

	private static string GetLogKey(AdvBacklog log)
	{
		return log != null ? log.MainVoiceFileName : "";
	}

	private static string NormalizeKey(string key)
	{
		return string.IsNullOrWhiteSpace(key) ? "" : key.Trim().Replace('\\', '/').ToLowerInvariant();
	}

	private static void EnsureSaveComponent(AdvEngine engine)
	{
		if (engine == null) return;
		if (engine.GetComponent<AdvVoiceCollectionSaveData>() == null)
		{
			engine.gameObject.AddComponent<AdvVoiceCollectionSaveData>();
		}
	}
}

public class AdvVoiceCollectionSaveData : MonoBehaviour, IAdvSystemSaveDataCustom
{
	public string SaveKey => "AdvVoiceCollectionData";

	private AdvEngine Engine => GetComponent<AdvEngine>() ?? GetComponentInParent<AdvEngine>(true);

	public void OnWrite(BinaryWriter writer)
	{
		AdvVoiceCollectionData.Get(Engine).OnWrite(writer);
	}

	public void OnRead(BinaryReader reader)
	{
		AdvVoiceCollectionData.Get(Engine).OnRead(reader);
	}
}
