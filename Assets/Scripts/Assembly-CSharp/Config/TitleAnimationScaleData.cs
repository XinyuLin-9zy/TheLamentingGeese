using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
	[CreateAssetMenu(fileName = "TitleAnimationScaleData", menuName = "配置/动画缩放数据", order = 1)]
	public class TitleAnimationScaleData : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TitleAnimationScaleEntry> entries = new List<TitleAnimationScaleEntry>();

		[NonSerialized]
		public Dictionary<string, TitleRateData> allTitleAnimationData;

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
			if (allTitleAnimationData == null)
			{
				allTitleAnimationData = new Dictionary<string, TitleRateData>();
			}
			else
			{
				allTitleAnimationData.Clear();
			}

			if (entries == null) return;
			foreach (TitleAnimationScaleEntry entry in entries)
			{
				if (entry == null || string.IsNullOrEmpty(entry.titleName)) continue;
				allTitleAnimationData[entry.titleName] = entry.data;
			}
		}

		[Serializable]
		public class TitleAnimationScaleEntry
		{
			public string titleName;
			public TitleRateData data;
		}
	}
}
