using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
	[CreateAssetMenu(fileName = "SteamAchievement", menuName = "成就/Steam", order = 0)]
	public class SteamAchievementData : ScriptableObject, ISerializationCallbackReceiver
	{
		public string version;

		[SerializeField]
		private List<SteamAchievementEntry> entries = new List<SteamAchievementEntry>();

		[NonSerialized]
		public Dictionary<string, string[]> achievementDict;

		private void OnEnable()
		{
			RebuildLookup();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			RebuildLookup();
		}
#endif

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			RebuildLookup();
		}

		public void RebuildLookup()
		{
			if (achievementDict == null)
			{
				achievementDict = new Dictionary<string, string[]>();
			}
			else
			{
				achievementDict.Clear();
			}

			if (entries == null) return;
			foreach (SteamAchievementEntry entry in entries)
			{
				if (entry == null || string.IsNullOrEmpty(entry.steamId)) continue;
				achievementDict[entry.steamId] = entry.achievementIds != null
					? entry.achievementIds.ToArray()
					: new string[0];
			}
		}

		[Serializable]
		public class SteamAchievementEntry
		{
			public string steamId;
			public List<string> achievementIds;
		}
	}
}
