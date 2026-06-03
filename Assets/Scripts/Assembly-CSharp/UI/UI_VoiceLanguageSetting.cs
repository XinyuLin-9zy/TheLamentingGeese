using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace UI
{
	public class UI_VoiceLanguageSetting : MonoBehaviour
	{
		public UtageUguiConfig config;

		public Button btn_next;

		public Button btn_pre;

		public Text text_curLanguage;

		private List<string> languages;

		private bool isInitialized;

		private LanguageManagerBase subscribedLanguageManager;

		private void Start()
		{
			CheckVoiceLanguageSetting();
		}

		private void OnEnable()
		{
			CheckVoiceLanguageSetting();
			SubscribeConfig();
			SubscribeLanguageChanged();
		}

		private void OnDisable()
		{
			UnsubscribeConfig();
			UnsubscribeLanguageChanged();
		}

		private void OnLanguageChange(string obj, GameLanguageType type)
		{
			if (type == GameLanguageType.Voice)
			{
				InitText();
			}
		}

		public void CheckVoiceLanguageSetting()
		{
			Initialize();
			InitText();
		}

		private void InitText()
		{
			LanguageManagerBase languageManager = LanguageManagerBase.Instance;
			if (text_curLanguage == null || languageManager == null) return;

			text_curLanguage.text = ToDisplayName(languageManager.CurrentVoiceLanguage);
		}

		private void ChangeLanguage(string language)
		{
			if (string.IsNullOrEmpty(language)) return;

			LanguageManagerBase languageManager = LanguageManagerBase.Instance;
			if (languageManager == null) return;

			languageManager.VoiceLanguage = language;
			InitText();
		}

		private void OnClickBtn(bool isNext)
		{
			InitializeLanguages();
			if (languages == null || languages.Count <= 0) return;

			LanguageManagerBase languageManager = LanguageManagerBase.Instance;
			string current = languageManager != null ? languageManager.CurrentVoiceLanguage : "";
			int index = languages.IndexOf(current);
			if (index < 0) index = 0;

			index += isNext ? 1 : -1;
			if (index < 0) index = languages.Count - 1;
			if (index >= languages.Count) index = 0;

			ChangeLanguage(languages[index]);
		}

		private void Initialize()
		{
			if (isInitialized) return;

			if (config == null)
			{
				config = GetComponentInParent<UtageUguiConfig>(true);
			}
			if (btn_next == null) btn_next = FindButton("BtnNext", "Next");
			if (btn_pre == null) btn_pre = FindButton("BtnPre", "BtnPrev", "Previous");
			if (text_curLanguage == null || text_curLanguage is UguiNovelText)
			{
				text_curLanguage = FindDisplayText();
			}
			NormalizeDisplayText();

			Bind(btn_next, () => OnClickBtn(true));
			Bind(btn_pre, () => OnClickBtn(false));
			InitializeLanguages();

			isInitialized = true;
		}

		private void InitializeLanguages()
		{
			if (languages == null)
			{
				languages = new List<string>();
			}
			languages.Clear();

			LanguageManagerBase languageManager = LanguageManagerBase.Instance;
			if (languageManager != null)
			{
				foreach (string language in languageManager.VoiceLanguages)
				{
					AddLanguage(language);
				}
				if (languages.Count <= 0)
				{
					foreach (string language in languageManager.Languages)
					{
						AddLanguage(language);
					}
				}
				AddLanguage(languageManager.CurrentVoiceLanguage);
			}

			if (languages.Count <= 0)
			{
				AddLanguage("SC");
				AddLanguage("TC");
				AddLanguage("English");
				AddLanguage("Japanese");
				AddLanguage("Russian");
			}
		}

		private void AddLanguage(string language)
		{
			if (string.IsNullOrEmpty(language)) return;
			if (!languages.Contains(language))
			{
				languages.Add(language);
			}
		}

		private void Bind(Button button, UnityEngine.Events.UnityAction action)
		{
			if (button == null || action == null) return;
			if (button.targetGraphic == null)
			{
				button.targetGraphic = button.GetComponent<Graphic>() ?? button.GetComponentInChildren<Graphic>(true);
			}
			button.onClick.RemoveListener(action);
			button.onClick.AddListener(action);
		}

		private Text FindDisplayText()
		{
			Transform bg = FindChildRecursive(transform, "BG");
			Transform textRoot = bg != null ? FindDirectChild(bg, "Text") : null;
			textRoot = textRoot ?? FindChildRecursive(transform, "Text");
			if (textRoot != null)
			{
				Text text = textRoot.GetComponent<Text>();
				if (text != null) return text;
			}
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				if (text != null && !(text is UguiNovelText)) return text;
			}
			return null;
		}

		private void NormalizeDisplayText()
		{
			if (text_curLanguage == null) return;

			DisableNovelTextOverlays(text_curLanguage);
			text_curLanguage.gameObject.SetActive(true);
			text_curLanguage.enabled = true;
			text_curLanguage.color = new Color(0.08f, 0.08f, 0.09f, 1f);
			text_curLanguage.alignment = TextAnchor.MiddleCenter;
			text_curLanguage.raycastTarget = false;

			RectTransform rectTransform = text_curLanguage.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
				rectTransform.localScale = Vector3.one;
			}
		}

		private void DisableNovelTextOverlays(Text keep)
		{
			foreach (UguiNovelText novelText in GetComponentsInChildren<UguiNovelText>(true))
			{
				if (novelText == null || novelText == keep) continue;
				RawImage rawImage = novelText.GetComponent<RawImage>();
				if (rawImage != null)
				{
					Color color = rawImage.color;
					color.a = 0f;
					rawImage.color = color;
					rawImage.raycastTarget = false;
				}
				novelText.enabled = false;
				novelText.text = "";
				novelText.raycastTarget = false;
			}
		}

		private void SubscribeLanguageChanged()
		{
			LanguageManagerBase languageManager = LanguageManagerBase.Instance;
			if (languageManager == null || subscribedLanguageManager == languageManager) return;

			UnsubscribeLanguageChanged();
			subscribedLanguageManager = languageManager;
			subscribedLanguageManager.OnChangeLanguage += InitText;
		}

		private void SubscribeConfig()
		{
			if (config == null) return;
			config.OnLoadValues.RemoveListener(CheckVoiceLanguageSetting);
			config.OnLoadValues.AddListener(CheckVoiceLanguageSetting);
		}

		private void UnsubscribeConfig()
		{
			if (config == null) return;
			config.OnLoadValues.RemoveListener(CheckVoiceLanguageSetting);
		}

		private void UnsubscribeLanguageChanged()
		{
			if (subscribedLanguageManager == null) return;
			subscribedLanguageManager.OnChangeLanguage -= InitText;
			subscribedLanguageManager = null;
		}

		private Button FindButton(params string[] names)
		{
			foreach (string name in names)
			{
				Transform target = FindChildRecursive(transform, name);
				if (target == null) continue;

				Button button = target.GetComponent<Button>() ?? target.GetComponentInChildren<Button>(true);
				if (button != null) return button;
			}
			return null;
		}

		private static Transform FindDirectChild(Transform root, string targetName)
		{
			if (root == null) return null;

			foreach (Transform child in root)
			{
				if (child.name == targetName) return child;
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

		private static string ToDisplayName(string language)
		{
			if (string.IsNullOrEmpty(language)) return "";
			switch (language.Trim().ToLowerInvariant())
			{
				case "sc":
				case "simplifiedchinese":
				case "chinesesimplified":
					return "中文";
				case "tc":
				case "traditionalchinese":
				case "chinesetraditional":
					return "中文";
				case "en":
				case "english":
					return "English";
				case "ja":
				case "japanese":
					return "Japanese";
				case "ru":
				case "russian":
					return "Russian";
				default:
					return language;
			}
		}
	}
}
