// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using System;
using TMPro;

namespace Utage
{

	/// <summary>
	/// サウンドルーム用のUIのサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSoundRoomItem")]
	public class UtageUguiSoundRoomItem : MonoBehaviour
	{
		/// <summary>本文</summary>
		[HideIfTMP] public Text title;

		[HideIfLegacyText] public TextMeshProUGUI titleTmp;

		[SerializeField] protected Sprite activeSprite;
		[SerializeField] protected Sprite normalSprite;
		[SerializeField] protected Sprite unlockSprite;
		[SerializeField] protected Toggle toggle;
		
		//初期化時に呼ばれるイベント
		public UnityEvent OnInit => onInit;
		[SerializeField] UnityEvent onInit = new();

		public AdvSoundSettingData Data
		{
			get { return data; }
		}

		protected AdvSoundSettingData data;
		protected Image backgroundImage;
		protected Button button;
		protected Selectable selectable;
		protected bool isPlayable;
		protected bool displayAsLocked;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">セーブデータ</param>
		/// <param name="index">インデックス</param>
		public virtual void Init(AdvSoundSettingData data, Action<UtageUguiSoundRoomItem> ButtonClickedEvent, int index)
		{
			this.data = data;
			EnsureRuntimeReferences();
			NormalizeLayout();
			isPlayable = CheckPlayable(data);
			displayAsLocked = ShouldDisplayAsLocked(data);
			SetTextTitle(GetDisplayTitle(data));
			BindPrimaryInteraction(ButtonClickedEvent);
			NormalizeTitleLayout();
			ApplyVisualState(false);
			OnInit.Invoke();
		}

		public virtual void SetTextTitle(string text)
		{
			if (title != null) title.gameObject.SetActive(true);
			if (titleTmp != null) titleTmp.gameObject.SetActive(true);
			TextComponentWrapper.SetText(title, titleTmp, text);
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (title == null && titleTmp == null)
			{
				title = FindComponentByName<Text>("title")
					?? FindComponentByName<Text>("Title")
					?? FindComponentByName<Text>("text")
					?? FindComponentByName<Text>("Text");
				titleTmp = FindComponentByName<TextMeshProUGUI>("title")
					?? FindComponentByName<TextMeshProUGUI>("Title")
					?? FindComponentByName<TextMeshProUGUI>("text")
					?? FindComponentByName<TextMeshProUGUI>("Text");
			}

			if (toggle == null)
			{
				toggle = GetComponent<Toggle>();
			}

			button = GetComponent<Button>();
			selectable = toggle != null ? toggle : button != null ? button : GetComponent<Selectable>();
			backgroundImage = GetComponent<Image>() ?? GetComponentInChildren<Image>(true);

			Graphic titleGraphic = titleTmp != null ? (Graphic)titleTmp : title;
			if (titleGraphic != null)
			{
				titleGraphic.raycastTarget = false;
			}

			if (selectable != null)
			{
				Graphic targetGraphic = EnsureTargetGraphic(selectable, backgroundImage);
				backgroundImage = targetGraphic as Image ?? backgroundImage;
				ApplySelectableSpriteState(selectable);
			}
		}

		protected virtual void NormalizeLayout()
		{
			RectTransform rectTransform = transform as RectTransform;
			if (rectTransform == null) return;

			RectTransform parentRect = rectTransform.parent as RectTransform;
			float targetWidth = parentRect != null && parentRect.rect.width > 0
				? parentRect.rect.width
				: 480f;

			if (targetWidth > 0 && Mathf.Abs(rectTransform.rect.width - targetWidth) > 8f)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
			}

			float currentHeight = rectTransform.rect.height > 0 ? rectTransform.rect.height : rectTransform.sizeDelta.y;
			if (currentHeight < 84f || currentHeight > 112f)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 96f);
			}
		}

		protected virtual string GetDisplayTitle(AdvSoundSettingData soundData)
		{
			if (soundData == null) return string.Empty;
			if (ShouldDisplayAsLocked(soundData)) return "未解锁";
			if (!string.IsNullOrEmpty(soundData.LocalizedTitle)) return soundData.LocalizedTitle;
			if (!string.IsNullOrEmpty(soundData.Title)) return soundData.Title;
			return soundData.Key;
		}

		protected virtual bool ShouldDisplayAsLocked(AdvSoundSettingData soundData)
		{
			return soundData != null;
		}

		protected virtual bool CheckPlayable(AdvSoundSettingData soundData)
		{
			return soundData != null
				&& !string.IsNullOrEmpty(soundData.Key)
				&& !string.IsNullOrEmpty(soundData.FilePath);
		}

		protected virtual void BindPrimaryInteraction(Action<UtageUguiSoundRoomItem> buttonClickedEvent)
		{
			if (toggle != null)
			{
				EnsureTargetGraphic(toggle, backgroundImage);
				toggle.onValueChanged.RemoveAllListeners();
				toggle.interactable = isPlayable;
				toggle.SetIsOnWithoutNotify(false);
				if (!isPlayable)
				{
					return;
				}

				toggle.onValueChanged.AddListener(isOn =>
				{
					ApplyVisualState(isOn);
					if (!isOn) return;
					buttonClickedEvent?.Invoke(this);
					toggle.SetIsOnWithoutNotify(false);
					ApplyVisualState(false);
				});
				return;
			}

			if (button == null)
			{
				if (selectable != null && !(selectable is Button))
				{
					Graphic proxyGraphic = EnsureTargetGraphic(selectable, backgroundImage);
					Button proxyButton = FindInteractionProxyButton();
					EnsureTargetGraphic(proxyButton, proxyGraphic);
					proxyButton.interactable = isPlayable;
					proxyButton.onClick.RemoveAllListeners();
					if (isPlayable)
					{
						proxyButton.onClick.AddListener(() => buttonClickedEvent?.Invoke(this));
					}
					return;
				}

				button = gameObject.AddComponent<Button>();
				selectable = button;
				ApplySelectableSpriteState(button);
			}

			EnsureTargetGraphic(button, backgroundImage);
			button.interactable = isPlayable;
			button.onClick.RemoveAllListeners();
			if (isPlayable)
			{
				button.onClick.AddListener(() => buttonClickedEvent?.Invoke(this));
			}
		}

		protected virtual void ApplySelectableSpriteState(Selectable currentSelectable)
		{
			if (currentSelectable == null) return;
			if (activeSprite != null)
			{
				currentSelectable.transition = Selectable.Transition.SpriteSwap;
			}

			SpriteState spriteState = currentSelectable.spriteState;
			if (activeSprite != null)
			{
				if (spriteState.highlightedSprite == null) spriteState.highlightedSprite = activeSprite;
				if (spriteState.pressedSprite == null) spriteState.pressedSprite = activeSprite;
				if (spriteState.selectedSprite == null) spriteState.selectedSprite = activeSprite;
			}
			if (unlockSprite != null)
			{
				if (spriteState.disabledSprite == null) spriteState.disabledSprite = unlockSprite;
			}
			currentSelectable.spriteState = spriteState;
		}

		protected virtual void ApplyVisualState(bool isActive)
		{
			if (backgroundImage != null)
			{
				if ((displayAsLocked || !isPlayable) && unlockSprite != null)
				{
					backgroundImage.sprite = unlockSprite;
				}
				else if (isActive && activeSprite != null)
				{
					backgroundImage.sprite = activeSprite;
				}
				else if (normalSprite != null)
				{
					backgroundImage.sprite = normalSprite;
				}
			}

			Graphic titleGraphic = titleTmp != null ? (Graphic)titleTmp : title;
			if (titleGraphic != null)
			{
				titleGraphic.color = Color.black;
			}
		}

		protected virtual void NormalizeTitleLayout()
		{
			if (title != null)
			{
				title.alignment = TextAnchor.MiddleCenter;
				if (title.font == null) title.font = ResolveFont();
				title.color = Color.black;
				title.fontSize = Mathf.Max(title.fontSize, 24);
				title.raycastTarget = false;
				StretchTitleRect(title.transform as RectTransform);
				title.transform.SetAsLastSibling();
			}
			if (titleTmp != null)
			{
				titleTmp.alignment = TextAlignmentOptions.Center;
				titleTmp.color = Color.black;
				titleTmp.fontSize = Mathf.Max(titleTmp.fontSize, 24);
				titleTmp.raycastTarget = false;
				StretchTitleRect(titleTmp.transform as RectTransform);
				titleTmp.transform.SetAsLastSibling();
			}
		}

		protected static void StretchTitleRect(RectTransform rect)
		{
			if (rect == null) return;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;
		}

		protected virtual Font ResolveFont()
		{
			Text source = title ?? GetComponentInChildren<Text>(true);
			if (source != null && source.font != null) return source.font;
			return Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 24);
		}

		protected virtual Button FindInteractionProxyButton()
		{
			Transform proxy = FindChildRecursive(transform, "__RuntimeInteractionProxy");
			if (proxy == null)
			{
				GameObject proxyObject = new GameObject("__RuntimeInteractionProxy", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
				proxyObject.transform.SetParent(transform, false);
				proxy = proxyObject.transform;

				RectTransform rectTransform = proxy as RectTransform;
				if (rectTransform != null)
				{
					rectTransform.anchorMin = Vector2.zero;
					rectTransform.anchorMax = Vector2.one;
					rectTransform.offsetMin = Vector2.zero;
					rectTransform.offsetMax = Vector2.zero;
					rectTransform.localScale = Vector3.one;
				}

				Image image = proxyObject.GetComponent<Image>();
				image.color = new Color(1f, 1f, 1f, 0f);
				image.raycastTarget = true;
			}

			Button button = proxy.GetComponent<Button>();
			if (button == null)
			{
				button = proxy.gameObject.AddComponent<Button>();
			}
			return button;
		}

		protected virtual Graphic EnsureTargetGraphic(Selectable selectable, Graphic preferredGraphic = null)
		{
			if (selectable == null) return null;

			Graphic graphic = preferredGraphic;
			if (graphic == null)
			{
				graphic = selectable.targetGraphic;
			}
			if (graphic == null)
			{
				graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
			}
			if (graphic == null)
			{
				Image fallbackImage = gameObject.GetComponent<Image>();
				if (fallbackImage == null)
				{
					fallbackImage = gameObject.AddComponent<Image>();
					fallbackImage.color = new Color(1f, 1f, 1f, 0f);
				}
				fallbackImage.raycastTarget = true;
				graphic = fallbackImage;
			}

			selectable.targetGraphic = graphic;
			return graphic;
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
	}
}
