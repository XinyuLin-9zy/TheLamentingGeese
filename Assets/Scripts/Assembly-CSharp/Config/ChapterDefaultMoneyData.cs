using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
	[CreateAssetMenu(fileName = "ChapterDefaultMoneyData", menuName = "钱/默认", order = 0)]
	public class ChapterDefaultMoneyData : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<ChapterDefaultMoneyEntry> entries = new List<ChapterDefaultMoneyEntry>();

		[NonSerialized]
		public Dictionary<string, int> defaultMoneyDict;

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
			if (defaultMoneyDict == null)
			{
				defaultMoneyDict = new Dictionary<string, int>();
			}
			else
			{
				defaultMoneyDict.Clear();
			}

			if (entries == null) return;
			foreach (ChapterDefaultMoneyEntry entry in entries)
			{
				if (entry == null || string.IsNullOrEmpty(entry.tagName)) continue;
				defaultMoneyDict[entry.tagName] = entry.money;
			}
		}

		[Serializable]
		public class ChapterDefaultMoneyEntry
		{
			public string tagName;
			public int money;
		}
	}
}
