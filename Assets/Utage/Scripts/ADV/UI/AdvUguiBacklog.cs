// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UtageExtensions;

namespace Utage
{
	/// <summary>
	/// バックログ用UI
	/// </summary>
	[AddComponentMenu("Utage/ADV/AdvUguiBacklog")]
	public class AdvUguiBacklog : MonoBehaviour
	{
		const float MinTextHeight = 49f;
		static Font fallbackFont;

		/// <summary>テキスト</summary>
		[HideIfTMP] public UguiNovelText text;
		[SerializeField, HideIfLegacyText] protected TextMeshProNovelText textMeshProLogText;

		/// <summary>キャラ名</summary>
		[HideIfTMP] public Text characterName;
		[SerializeField, HideIfLegacyText] protected TextMeshProNovelText textMeshProCharacterName;
		
		//キャラ名のルート（背景オブジェクトなどを表示、非表示するときに）
		[SerializeField] protected GameObject characterNameRoot;


		/// <summary>ボイス再生アイコン</summary>
		public GameObject soundIcon;

		/// <summary>ボイス收藏アイコン</summary>
		public GameObject collectionIcon;

		/// ボイス再生ボタン
		public Button Button { get { return this.GetComponentCache(ref button); } }
		[SerializeField] protected Button button;
		[SerializeField] protected Button collectionButton;

		/// <summary>ページ内に複数行あるか（ログの長さにあわせて変えるたりする）</summary>
		public bool isMultiTextInPage;

		public AdvEngine Engine { get; set; }
		public AdvBacklog Data { get { return data; } }
		protected AdvBacklog data;
		protected UguiNovelTextEventTrigger textVoiceEventTrigger;
		protected UnityAction<UguiNovelTextHitArea> textVoiceClickListener;
		readonly List<Button> voiceReplayButtons = new List<Button>();
		UnityAction voiceButtonClickListener;
		readonly List<Button> collectionButtons = new List<Button>();
		UnityAction collectionButtonClickListener;
		bool collectionCollected;
		int backlogVoiceRequestId;

		//初期化時に呼ばれるイベント
		public UnityEvent OnInit => onInit;
		[SerializeField] UnityEvent onInit = new();

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">バックログのデータ</param>
		public virtual void Init(Utage.AdvBacklog data)
		{
			this.data = data;
			EnsureRuntimeReferences();

			if (this.data == null)
			{
				ClearDisplayedText();
				InitVoice();
				InitCollection();
				OnInit.Invoke();
				return;
			}

			if (isMultiTextInPage)
			{
				InitTextIfMulti();
			}
			else
			{
				InitTextIfSingle();
			}
			InitCharacterName();
			InitVoice();
			InitCollection();
			OnInit.Invoke();
		}

		protected virtual void Awake()
		{
			EnsureRuntimeReferences();
			if (data == null)
			{
				ClearDisplayedText();
				ResetVoiceButton(false);
				ResetCollectionButton(false);
			}
		}

		protected virtual void OnDestroy()
		{
			ClearTextVoiceClickListener();
			ClearVoiceButtonListeners();
			ClearCollectionButtonListeners();
		}

		//ページ内に複数テキストがある場合の初期化を行う
		protected virtual void InitTextIfMulti()
		{
			if (text == null && textMeshProLogText == null)
			{
				return;
			}

			RectTransform textRectTransform = NovelTextComponentWrapper.GetRectTransform(text,textMeshProLogText);
			if (textRectTransform == null)
			{
				return;
			}

			float defaultHeight = textRectTransform.rect.height;
			SetLogText(data.Text);
			float height = Mathf.Max(MinTextHeight, NovelTextComponentWrapper.GetPreferredHeight(text,textMeshProLogText));

			RectTransform r = (RectTransform)this.transform;
			textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			RefreshLayout(textRectTransform);
			float baseH = r.rect.height;
			float scale = textRectTransform.lossyScale.y / this.transform.lossyScale.y;
			baseH += (height - defaultHeight) * scale;
			r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseH);
			RefreshLayout(r);
		}

		//ページ内に1つのテキストがある場合の初期化を行う
		protected virtual void InitTextIfSingle()
		{
			if (text == null && textMeshProLogText == null)
			{
				return;
			}
			SetLogText(data.Text);

			RectTransform textRectTransform = NovelTextComponentWrapper.GetRectTransform(text, textMeshProLogText);
			if (textRectTransform != null)
			{
				float preferredHeight = NovelTextComponentWrapper.GetPreferredHeight(text, textMeshProLogText);
				textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(MinTextHeight, preferredHeight));
				RefreshLayout(textRectTransform);
			}
		}

		//キャラ名に関しての初期化を行う
		protected virtual void InitCharacterName()
		{
			if (characterName == null && textMeshProCharacterName == null)
			{
				if (characterNameRoot != null)
				{
					characterNameRoot.SetActive(false);
				}
				return;
			}
			NovelTextComponentWrapper.SetText(characterName, textMeshProCharacterName, data.MainCharacterNameText);
			if (characterNameRoot!=null)
			{
				characterNameRoot.SetActive(!string.IsNullOrEmpty(data.MainCharacterNameText));
			}
		}

		//ボイスに関しての初期化を行う
		protected virtual void InitVoice()
		{
			ClearTextVoiceClickListener();
			ClearVoiceButtonListeners();

			int countVoice = data != null ? data.CountVoice : 0;
			string mainVoiceFileName = data != null ? data.MainVoiceFileName : "";

			if (countVoice <= 0 || string.IsNullOrEmpty(mainVoiceFileName))
			{
				ResetVoiceButton(false);
			}
			else
			{
				ResetVoiceButton(true);
				voiceButtonClickListener = () => OnClicked(mainVoiceFileName);
				RegisterVoiceReplayButton(Button);
				RegisterVoiceReplayButton(GetSoundIconButton());

				if (countVoice >= 2 || isMultiTextInPage)
				{
					InitVoiceIfMulti();
				}
			}
		}

		protected virtual void InitCollection()
		{
			ClearCollectionButtonListeners();

			int countVoice = data != null ? data.CountVoice : 0;
			string mainVoiceFileName = data != null ? data.MainVoiceFileName : "";
			bool isActive = countVoice > 0 && !string.IsNullOrEmpty(mainVoiceFileName);
			if (!isActive)
			{
				ResetCollectionButton(false);
				return;
			}

			ResetCollectionButton(true);
			collectionCollected = CheckVoiceCollected();
			ApplyCollectionVisualState();
			collectionButtonClickListener = OnClickedCollection;
			RegisterCollectionButton(collectionButton);
			RegisterCollectionButton(GetCollectionIconButton());
		}

		protected virtual void SetLogText(string logText)
		{
			NovelTextComponentWrapper.SetText(text, textMeshProLogText, logText);
			if (text != null)
			{
				text.LengthOfView = -1;
			}
		}

		//ボイスが複数ある場合の初期化を行う
		protected virtual void InitVoiceIfMulti()
		{
			if (text == null)
			{
				return;
			}

			textVoiceEventTrigger = text.gameObject.GetComponentCreateIfMissing<UguiNovelTextEventTrigger>();
			textVoiceEventTrigger.enabled = true;
			textVoiceClickListener = (x) => OnClickHitArea(x, OnClicked);
			textVoiceEventTrigger.OnClick.AddListener(textVoiceClickListener);
		}

		protected virtual void ClearDisplayedText()
		{
			NovelTextComponentWrapper.Clear(text, textMeshProLogText);
			NovelTextComponentWrapper.Clear(characterName, textMeshProCharacterName);
			if (characterNameRoot != null)
			{
				characterNameRoot.SetActive(false);
			}
		}

		protected virtual void ResetVoiceButton(bool isActive)
		{
			SetSoundIconActive(isActive);
			Button button = Button;
			if (button != null)
			{
				button.interactable = isActive;
			}
		}

		protected virtual void ResetCollectionButton(bool isActive)
		{
			SetCollectionIconActive(isActive);
			Button targetButton = collectionButton;
			if (targetButton != null)
			{
				targetButton.interactable = isActive;
			}
		}

		protected virtual void ClearTextVoiceClickListener()
		{
			UguiNovelTextEventTrigger trigger = textVoiceEventTrigger;
			if (trigger != null && textVoiceClickListener != null)
			{
				trigger.OnClick.RemoveListener(textVoiceClickListener);
			}
			if (trigger != null)
			{
				trigger.enabled = false;
			}
			textVoiceEventTrigger = null;
			textVoiceClickListener = null;
		}

		protected virtual void ClearVoiceButtonListeners()
		{
			if (voiceButtonClickListener != null)
			{
				foreach (Button replayButton in voiceReplayButtons)
				{
					if (replayButton != null)
					{
						replayButton.onClick.RemoveListener(voiceButtonClickListener);
					}
				}
			}
			voiceReplayButtons.Clear();
			voiceButtonClickListener = null;
		}

		protected virtual void ClearCollectionButtonListeners()
		{
			if (collectionButtonClickListener != null)
			{
				foreach (Button targetButton in collectionButtons)
				{
					if (targetButton != null)
					{
						targetButton.onClick.RemoveListener(collectionButtonClickListener);
					}
				}
			}
			collectionButtons.Clear();
			collectionButtonClickListener = null;
		}

		protected virtual void RegisterVoiceReplayButton(Button replayButton)
		{
			if (replayButton == null || voiceButtonClickListener == null)
			{
				return;
			}
			foreach (Button registeredButton in voiceReplayButtons)
			{
				if (registeredButton == replayButton)
				{
					return;
				}
				if (registeredButton != null && registeredButton.gameObject == replayButton.gameObject)
				{
					return;
				}
			}

			replayButton.interactable = true;
			replayButton.onClick.AddListener(voiceButtonClickListener);
			voiceReplayButtons.Add(replayButton);
		}

		protected virtual void RegisterCollectionButton(Button targetButton)
		{
			if (targetButton == null || collectionButtonClickListener == null)
			{
				return;
			}
			foreach (Button registeredButton in collectionButtons)
			{
				if (registeredButton == targetButton)
				{
					return;
				}
				if (registeredButton != null && registeredButton.gameObject == targetButton.gameObject)
				{
					return;
				}
			}

			targetButton.interactable = true;
			targetButton.onClick.AddListener(collectionButtonClickListener);
			collectionButtons.Add(targetButton);
		}

		protected virtual Button GetSoundIconButton()
		{
			return soundIcon != null ? soundIcon.GetComponent<Button>() : null;
		}

		protected virtual Button GetCollectionIconButton()
		{
			return collectionIcon != null ? collectionIcon.GetComponent<Button>() : null;
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (button == null)
			{
				button = GetComponent<Button>();
			}
			if (button != null && button.targetGraphic == null)
			{
				button.targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
			}

			if (text == null && textMeshProLogText == null)
			{
				text = FindComponentByName<UguiNovelText>("text") ?? GetComponentInChildren<UguiNovelText>(true);
				textMeshProLogText = FindComponentByName<TextMeshProNovelText>("text") ?? GetComponentInChildren<TextMeshProNovelText>(true);
			}
			if (characterName == null && textMeshProCharacterName == null)
			{
				characterName = FindComponentByName<Text>("name") ?? FindComponentByName<Text>("characterName");
				textMeshProCharacterName = FindComponentByName<TextMeshProNovelText>("name") ?? FindComponentByName<TextMeshProNovelText>("characterName");
			}
			if (characterNameRoot == null)
			{
				Transform root = FindChildRecursive(transform, "name") ?? FindChildRecursive(transform, "Name");
				if (root != null) characterNameRoot = root.gameObject;
			}
			if (soundIcon == null)
			{
				Transform target = FindChildRecursive(transform, "sound")
					?? FindChildRecursive(transform, "Sound")
					?? FindChildRecursive(transform, "Voice")
					?? FindChildRecursive(transform, "voice");
				if (target != null) soundIcon = target.gameObject;
			}
			if (collectionIcon == null)
			{
				Transform target = FindChildRecursive(transform, "collection")
					?? FindChildRecursive(transform, "Collection")
					?? FindChildRecursive(transform, "Favorite")
					?? FindChildRecursive(transform, "favorite");
				if (target != null) collectionIcon = target.gameObject;
			}
			if (collectionButton == null && collectionIcon != null)
			{
				collectionButton = collectionIcon.GetComponent<Button>() ?? collectionIcon.GetComponentInChildren<Button>(true);
				if (collectionButton == null)
				{
					collectionButton = collectionIcon.AddComponent<Button>();
				}
			}
			if (collectionButton != null && collectionButton.targetGraphic == null)
			{
				collectionButton.targetGraphic = collectionButton.GetComponent<Graphic>() ?? collectionButton.GetComponentInChildren<Graphic>(true);
			}

			EnsureLegacyTextFont(text, 30);
			EnsureLegacyTextFont(characterName, 32);
		}

		protected virtual void EnsureLegacyTextFont(Text legacyText, int fontSize)
		{
			if (legacyText == null || legacyText.font != null) return;

			if (fallbackFont == null)
			{
				fallbackFont = Font.CreateDynamicFontFromOSFont(
					new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" },
					fontSize);
			}
			if (fallbackFont != null)
			{
				legacyText.font = fallbackFont;
			}
		}

		protected virtual T FindComponentByName<T>(string targetName) where T : Component
		{
			Transform target = FindChildRecursive(transform, targetName);
			if (target == null) return null;
			return target.GetComponent<T>() ?? target.GetComponentInChildren<T>(true);
		}

		protected static Transform FindChildRecursive(Transform root, string targetName)
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

		protected virtual void SetSoundIconActive(bool isActive)
		{
			if (soundIcon != null)
			{
				soundIcon.SetActive(isActive);
			}
		}

		protected virtual void SetCollectionIconActive(bool isActive)
		{
			if (collectionIcon != null)
			{
				collectionIcon.SetActive(isActive);
			}
		}

		protected virtual void ApplyCollectionVisualState()
		{
			if (collectionIcon == null) return;

			foreach (Graphic graphic in collectionIcon.GetComponentsInChildren<Graphic>(true))
			{
				if (graphic == null) continue;
				Color color = graphic.color;
				color.a = collectionCollected ? 1f : 0.55f;
				graphic.color = color;
			}
		}

		protected virtual void RefreshLayout(RectTransform rectTransform)
		{
			if (rectTransform == null) return;

			PrepareNovelTextForCanvasRebuild(rectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			PrepareNovelTextForCanvasRebuild(rectTransform);
			Canvas.ForceUpdateCanvases();
		}

		protected virtual void PrepareNovelTextForCanvasRebuild(RectTransform root)
		{
			if (root == null) return;

			foreach (UguiNovelText novelText in root.GetComponentsInChildren<UguiNovelText>(true))
			{
				if (novelText == null) continue;
				_ = novelText.preferredHeight;
			}
		}

		protected virtual void OnClickHitArea(UguiNovelTextHitArea hitGroup, Action<string> OnClicked)
		{
			if (hitGroup == null) return;

			switch (hitGroup.HitEventType)
			{
				case CharData.HitEventType.Sound:
					OnClicked(hitGroup.Arg);
					break;
			}
		}

		protected virtual void OnClickedCollection()
		{
			if (Data == null || string.IsNullOrEmpty(Data.MainVoiceFileName))
			{
				return;
			}

			collectionCollected = ToggleVoiceCollection();
			ApplyCollectionVisualState();
			ShowCollectionHud(collectionCollected);
		}

		protected virtual bool ToggleVoiceCollection()
		{
			Type type = FindRuntimeType("AdvVoiceCollectionData");
			if (type == null) return false;

			MethodInfo method = type.GetMethod(
				"ToggleCollection",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				new[] { typeof(AdvEngine), typeof(AdvBacklog) },
				null);
			if (method == null) return false;

			try
			{
				object result = method.Invoke(null, new object[] { Engine, Data });
				return result is bool && (bool)result;
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Failed to toggle voice collection: " + exception.Message, this);
				return false;
			}
		}

		protected virtual bool CheckVoiceCollected()
		{
			Type type = FindRuntimeType("AdvVoiceCollectionData");
			if (type == null) return false;

			MethodInfo method = type.GetMethod(
				"CheckCollected",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				new[] { typeof(AdvEngine), typeof(AdvBacklog) },
				null);
			if (method == null) return false;

			try
			{
				object result = method.Invoke(null, new object[] { Engine, Data });
				return result is bool && (bool)result;
			}
			catch
			{
				return false;
			}
		}

		protected virtual void ShowCollectionHud(bool isCollect)
		{
			MonoBehaviour target = FindSceneMonoBehaviour("UI_DialogMsg");
			if (target == null) return;

			MethodInfo method = target.GetType().GetMethod("SetCollectVoiceState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null) return;

			try
			{
				method.Invoke(target, new object[] { isCollect });
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Failed to show voice collection HUD: " + exception.Message, this);
			}
		}

		protected virtual Type FindRuntimeType(string typeName)
		{
			if (string.IsNullOrEmpty(typeName)) return null;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(typeName);
				if (type != null) return type;
			}
			return null;
		}

		protected virtual MonoBehaviour FindSceneMonoBehaviour(string typeName)
		{
			if (string.IsNullOrEmpty(typeName)) return null;

			foreach (MonoBehaviour component in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
			{
				if (component == null || !component.gameObject.scene.IsValid()) continue;
				Type type = component.GetType();
				if (type.Name == typeName || type.FullName == typeName)
				{
					return component;
				}
			}
			return null;
		}


		/// <summary>
		/// 音声再生ボタンが押された
		/// </summary>
		/// <param name="button">押されたボタン</param>
		protected virtual void OnClicked(string voiceFileName)
		{
			if (string.IsNullOrEmpty(voiceFileName))
			{
				return;
			}

			int requestId = ++backlogVoiceRequestId;
			SoundManager manager = SoundManager.GetInstance();
			if (manager)
			{
				manager.StopVoice(0);
			}

			StartCoroutine(CoPlayVoice(voiceFileName, Data != null ? Data.FindCharacerLabel(voiceFileName) : null, requestId));
		}

		//ボイスの再生
		protected virtual IEnumerator CoPlayVoice(string voiceFileName, string characterLabel, int requestId)
		{
			AssetFile file = AssetFileManager.Load(voiceFileName, this);
			if (file == null)
			{
				Debug.LogError("Backlog voiceFile is NULL");
				yield break;
			}
			while (!file.IsLoadEnd)
			{
				yield return null;
			}
			if (requestId != backlogVoiceRequestId)
			{
				file.Unuse(this);
				yield break;
			}
			SoundManager manager = SoundManager.GetInstance();
			if (manager)
			{
				manager.StopVoice(0);
				manager.PlayVoice(characterLabel, file);
				if (Engine != null && Engine.ScenarioSound != null)
				{
					Engine.ScenarioSound.ClearVoiceInScenario(characterLabel);
				}
			}
			file.Unuse(this);
		}

	}
}

