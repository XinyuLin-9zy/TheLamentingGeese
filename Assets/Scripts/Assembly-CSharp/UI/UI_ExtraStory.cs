using System;
using Config;
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace UI
{
	public class UI_ExtraStory : MonoBehaviour
	{
		private static readonly string[] DefaultEndingUnlockKeys =
		{
			"map_commonEnd_1",
			"map_commonEnd_2",
			"map_commonEnd_3",
			"map_commonEnd_4",
			"map_commonEnd_5",
			"map_commonEnd_6",
			"map_commonEnd_7",
			"map_commonEnd_8",
			"map_commonEnd_9",
			"map_commonEnd_10"
		};
		private const string LockedMessageKey = "extra_story_locked";
		private const string LockedMessageFallback = "通关任意结局后解锁良田满穗";
		private const string OkTextKey = "ok";
		private const string OkTextFallback = "确定";

		public AdvEngine engine;

		public UtageUguiTitle title;

		public ExtraKeyData extraKeyData;

		public Button btn_extra;

		public string extraStoryTag;

		private bool extraStoryUnlock;

		public bool IsUnlocked => extraStoryUnlock;

		private void Awake()
		{
			Init();
		}

		private void OnEnable()
		{
			Init();
		}

		private void Init()
		{
			if (engine == null) engine = WrapperFindObject.FindObjectOfTypeIncludeInactive<AdvEngine>();
			if (title == null) title = GetComponentInParent<UtageUguiTitle>(true) ?? WrapperFindObject.FindObjectOfTypeIncludeInactive<UtageUguiTitle>();
			if (btn_extra == null) btn_extra = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
			if (string.IsNullOrEmpty(extraStoryTag)) extraStoryTag = "map_other_1";

			if (btn_extra != null)
			{
				btn_extra.onClick.RemoveListener(OnClickExtraStory);
				btn_extra.onClick.AddListener(OnClickExtraStory);
			}

			SetExtraStoryState();
		}

		private void SetExtraStoryState()
		{
			extraStoryUnlock = PlotMapProgressStore.AnyUnlocked(GetUnlockKeys());
			if (btn_extra != null)
			{
				btn_extra.interactable = true;
			}
			SetLockVisual(!extraStoryUnlock);
		}

		public void RefreshState()
		{
			Init();
		}

		private System.Collections.Generic.IList<string> GetUnlockKeys()
		{
			return extraKeyData != null && extraKeyData.keys != null && extraKeyData.keys.Count > 0
				? extraKeyData.keys
				: DefaultEndingUnlockKeys;
		}

		private void SetLockVisual(bool locked)
		{
			Transform lockVisual = FindChildRecursive(transform, "Lock");
			if (lockVisual != null)
			{
				lockVisual.gameObject.SetActive(locked);
			}
		}

		public void OnClickExtraStory()
		{
			Init();
			if (!extraStoryUnlock)
			{
				ShowLockedMessage();
				return;
			}

			if (title != null)
			{
				title.OnTapStartLabel(extraStoryTag);
				return;
			}

			UtageUguiMainGame mainGame = WrapperFindObject.FindObjectOfTypeIncludeInactive<UtageUguiMainGame>();
			if (mainGame != null)
			{
				mainGame.OpenStartLabel(extraStoryTag);
			}
		}

		public void ShowLockedMessage()
		{
			string message = Localize(LockedMessageKey, LockedMessageFallback);
			SystemUi systemUi = SystemUi.GetInstance();
			if (systemUi != null)
			{
				try
				{
					systemUi.OpenDialog1Button(message, Localize(OkTextKey, OkTextFallback), () => { });
					return;
				}
				catch (Exception exception)
				{
					Debug.LogWarning("Failed to open extra story locked dialog: " + exception.Message, this);
				}
			}

			SystemUiGuideMessage guideMessage = FindSceneObject<SystemUiGuideMessage>();
			if (guideMessage != null)
			{
				guideMessage.Open(message);
				return;
			}

			Debug.Log(message, this);
		}

		private static string Localize(string key, string fallback)
		{
			if (LanguageManagerBase.Instance != null)
			{
				string text;
				if (LanguageManagerBase.Instance.TryLocalizeText(key, out text) && !string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
			return fallback;
		}

		private static T FindSceneObject<T>() where T : Component
		{
			foreach (T item in Resources.FindObjectsOfTypeAll<T>())
			{
				if (item != null && item.gameObject.scene.IsValid())
				{
					return item;
				}
			}
			return null;
		}

		private static Transform FindChildRecursive(Transform root, string targetName)
		{
			if (root == null) return null;
			if (root.name == targetName) return root;

			foreach (Transform child in root)
			{
				Transform found = FindChildRecursive(child, targetName);
				if (found != null) return found;
			}

			return null;
		}
	}
}
