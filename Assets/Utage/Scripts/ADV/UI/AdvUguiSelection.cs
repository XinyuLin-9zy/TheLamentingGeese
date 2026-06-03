// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utage
{
	/// 選択肢用UI
	[AddComponentMenu("Utage/ADV/AdvUguiSelection")]
	public class AdvUguiSelection : MonoBehaviour
	{
		static Font fallbackFont;

		[HideIfTMP] public Text text;
		[SerializeField, HideIfLegacyText] protected TextMeshProNovelText textMeshPro;

		public UnityEvent OnInit => onInit;
		[SerializeField] UnityEvent onInit = new();

		public AdvSelection Data { get { return data; } }
		protected AdvSelection data;

		public virtual void Init(AdvSelection data, Action<AdvUguiSelection> ButtonClickedEvent)
		{
			this.data = data;
			EnsureTextComponent();
			NovelTextComponentWrapper.SetText(text, textMeshPro, data.Text);

			Button button = GetComponent<Button>();
			if (button == null)
			{
				button = gameObject.AddComponent<Button>();
			}
			if (button.targetGraphic == null)
			{
				button.targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
			}
			button.onClick.RemoveAllListeners();
			if (ButtonClickedEvent != null)
			{
				button.onClick.AddListener(() => ButtonClickedEvent(this));
			}

			OnInit.Invoke();
		}

		public virtual void OnInitSelected(Color color)
		{
			EnsureTextComponent();
			NovelTextComponentWrapper.SetColor(text, textMeshPro, color);
		}

		void EnsureTextComponent()
		{
			if (text == null)
			{
				text = GetComponentInChildren<Text>(true);
			}

			if (text == null)
			{
				Transform label = FindLabelTransform();
				GameObject target = ResolveLegacyTextTarget(label != null ? label : transform);
				text = target.GetComponent<Text>() ?? target.AddComponent<Text>();
			}

			EnsureLegacyText(text);

			DisableTmpLabelComponents();
			textMeshPro = null;
		}

		Transform FindLabelTransform()
		{
			Transform label = transform.Find("Label");
			if (label != null) return label;
			return GetComponentInChildren<TextMeshProUGUI>(true) != null
				? GetComponentInChildren<TextMeshProUGUI>(true).transform
				: transform;
		}

		GameObject ResolveLegacyTextTarget(Transform preferredRoot)
		{
			if (preferredRoot == null) return gameObject;

			Text existingText = preferredRoot.GetComponent<Text>();
			if (existingText != null) return preferredRoot.gameObject;

			if (preferredRoot.GetComponent<Graphic>() == null)
			{
				return preferredRoot.gameObject;
			}

			Transform legacyLabel = preferredRoot.Find("__LegacyLabel");
			if (legacyLabel == null)
			{
				GameObject legacyObject = new GameObject("__LegacyLabel", typeof(RectTransform), typeof(CanvasRenderer));
				legacyObject.transform.SetParent(preferredRoot, false);
				legacyLabel = legacyObject.transform;
			}
			legacyLabel.gameObject.layer = preferredRoot.gameObject.layer;

			RectTransform rectTransform = legacyLabel as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
				rectTransform.localScale = Vector3.one;
			}

			return legacyLabel.gameObject;
		}

		void EnsureLegacyText(Text legacyText)
		{
			if (legacyText == null) return;

			if (fallbackFont == null)
			{
				fallbackFont = Font.CreateDynamicFontFromOSFont(
					new[] { "Source Han Serif CN", "Source Han Serif SC", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" },
					legacyText.fontSize > 0 ? legacyText.fontSize : 28);
			}
			if (fallbackFont != null)
			{
				legacyText.font = fallbackFont;
			}

			legacyText.alignment = TextAnchor.MiddleCenter;
			legacyText.fontSize = Mathf.Max(legacyText.fontSize, 28);
			legacyText.raycastTarget = false;
			legacyText.supportRichText = true;
			legacyText.horizontalOverflow = HorizontalWrapMode.Overflow;
			legacyText.verticalOverflow = VerticalWrapMode.Truncate;
		}

		void DisableTmpLabelComponents()
		{
			foreach (TMP_Text tmp in GetComponentsInChildren<TMP_Text>(true))
			{
				if (tmp == null) continue;
				tmp.enabled = false;
				tmp.raycastTarget = false;
			}

			foreach (TextMeshProNovelText tmpNovel in GetComponentsInChildren<TextMeshProNovelText>(true))
			{
				if (tmpNovel == null) continue;
				tmpNovel.enabled = false;
			}
		}
	}
}
