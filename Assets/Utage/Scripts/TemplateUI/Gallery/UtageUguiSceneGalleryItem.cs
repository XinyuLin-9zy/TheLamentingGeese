// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Utage;
using System;
using TMPro;


namespace Utage
{

	/// <summary>
	/// シーン回想用のUIのサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSceneGalleryItem")]
	public class UtageUguiSceneGalleryItem : MonoBehaviour
	{
		const float LegacyRootWidth = 352f;
		const float LegacyRootHeight = 234f;
		const float LegacyThumbnailWidth = 352f;
		const float LegacyThumbnailHeight = 198f;
		const float LegacyThumbnailY = 18f;
		const float LegacyTitleWidth = 328f;
		const float LegacyTitleHeight = 30f;
		const float CustomRootWidth = 160f;
		const float CustomRootHeight = 120f;
		const float CustomThumbnailWidth = 148f;
		const float CustomThumbnailHeight = 84f;
		const float CustomThumbnailY = 10f;
		const float CustomTitleWidth = 148f;
		const float CustomTitleHeight = 24f;
		const float CustomTitleBottom = 4f;

		public AdvUguiLoadGraphicFile texture;
		[HideIfTMP] public Text title;
		[HideIfLegacyText] public TextMeshProUGUI titleTmp;
		[SerializeField] bool keepTextureActive; //テクスチャのアクティブのオンオフを切り替えるか
		[SerializeField] GameObject lockOverlayRoot;
		[SerializeField] Text lockLabel;
		[SerializeField] TextMeshProUGUI lockLabelTmp;
		
		//初期化時に呼ばれるイベント
		public UnityEvent OnInit => onInit;
		[SerializeField] UnityEvent onInit = new();

		public AdvSceneGallerySettingData Data
		{
			get { return data; }
		}

		protected AdvSceneGallerySettingData data;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">セーブデータ</param>
		/// <param name="index">インデックス</param>
		public virtual void Init(AdvSceneGallerySettingData data, Action<UtageUguiSceneGalleryItem> ButtonClickedEvent,
			AdvSystemSaveData saveData)
		{
			Init(data, saveData);
			UnityEngine.UI.Button button = EnsureButton();
			if (button != null && ButtonClickedEvent != null)
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() => ButtonClickedEvent(this));
				bool isOpened = IsOpened(data, saveData);
				button.interactable = isOpened;
			}
		}

		public virtual void Init(AdvSceneGallerySettingData data, AdvSystemSaveData saveData)
		{
			this.data = data;
			EnsureRuntimeReferences();
			NormalizeRuntimeLayout();
			BindDefaultClick();

			bool isOpened = IsOpened(data, saveData);
			if (!isOpened)
			{
				SetLockState(true);
				if(!keepTextureActive && texture != null) texture.gameObject.SetActive(false);
				SetTextTitle(string.Empty);
			}
			else
			{
				SetLockState(false);
				if (!keepTextureActive && texture != null) texture.gameObject.SetActive(true);
				if (texture != null) texture.LoadTextureFile(data.ThumbnailPath);
				SetTextTitle(data.LocalizedTitle);
			}
			OnInit.Invoke();
		}

		//クリックイベントを登録しない場合はこちら経由で
		//プレハブ上で、Buttonコンポーネントのインスペクターから登録しておく想定
		public virtual void OnClicked()
		{
			UtageUguiSceneGallery gallery = this.GetComponentInParent<UtageUguiSceneGallery>();
			if (gallery != null)
			{
				gallery.OnClickedButton(this);
			}
		}


		public virtual void SetTextTitle(string text)
		{
			ActivateTextObject(title, titleTmp);
			TextComponentWrapper.SetText(title, titleTmp, text);
		}

		protected virtual bool IsOpened(AdvSceneGallerySettingData data, AdvSystemSaveData saveData)
		{
			return data != null
				&& saveData != null
				&& saveData.GalleryData != null
				&& saveData.GalleryData.CheckSceneLabels(data.ScenarioLabel);
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (texture == null)
			{
				texture = GetComponentInChildren<AdvUguiLoadGraphicFile>(true);
			}
			if (title == null && titleTmp == null)
			{
				title = FindComponentByName<Text>("Title");
				titleTmp = FindComponentByName<TextMeshProUGUI>("Title");
				if (title == null && titleTmp == null)
				{
					title = FindComponentByName<Text>("text") ?? FindComponentByName<Text>("Text");
					titleTmp = FindComponentByName<TextMeshProUGUI>("text") ?? FindComponentByName<TextMeshProUGUI>("Text");
				}
			}
			if (lockOverlayRoot == null)
			{
				Transform root = FindChildRecursive(transform, "__RuntimeLockOverlay") ?? FindChildRecursive(transform, "LockOverlay");
				if (root != null) lockOverlayRoot = root.gameObject;
			}
			if (lockOverlayRoot == null)
			{
				lockOverlayRoot = CreateLockOverlay();
			}
			if (lockLabel == null && lockLabelTmp == null && lockOverlayRoot != null)
			{
				lockLabel = lockOverlayRoot.GetComponentInChildren<Text>(true);
				lockLabelTmp = lockOverlayRoot.GetComponentInChildren<TextMeshProUGUI>(true);
			}
		}

		protected virtual UnityEngine.UI.Button EnsureButton()
		{
			UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
			if (button == null)
			{
				button = gameObject.AddComponent<UnityEngine.UI.Button>();
			}
			if (button.targetGraphic == null || IsNullSpriteGraphic(button.targetGraphic))
			{
				Graphic targetGraphic = texture != null ? texture.GetComponent<Graphic>() : null;
				if (targetGraphic == null || IsNullSpriteGraphic(targetGraphic))
				{
					targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
				}
				if (!IsNullSpriteGraphic(targetGraphic))
				{
					button.targetGraphic = targetGraphic;
				}
			}
			return button;
		}

		protected virtual void BindDefaultClick()
		{
			UnityEngine.UI.Button button = EnsureButton();
			if (button == null) return;

			button.onClick.RemoveListener(OnClicked);
			button.onClick.AddListener(OnClicked);
		}

		protected virtual void SetLockState(bool locked)
		{
			if (lockOverlayRoot != null)
			{
				lockOverlayRoot.SetActive(locked);
			}
			TextComponentWrapper.SetText(lockLabel, lockLabelTmp, locked ? "LOCK" : "");
		}

		protected virtual void NormalizeRuntimeLayout()
		{
			RectTransform rootRect = transform as RectTransform;
			RectTransform textureRect = texture != null ? texture.transform as RectTransform : null;
			RectTransform titleRect = ResolveTextRect();

			if (UsesLegacyTemplateLayout())
			{
				ApplyLegacyTemplateLayout(rootRect, textureRect, titleRect);
			}
			else
			{
				ApplyCustomRuntimeLayout(rootRect, textureRect, titleRect);
			}

			SanitizeNullSpriteImages(transform);
		}

		protected virtual bool UsesLegacyTemplateLayout()
		{
			return FindChildRecursive(transform, "BG") != null || titleTmp != null;
		}

		protected virtual void ApplyLegacyTemplateLayout(RectTransform rootRect, RectTransform textureRect, RectTransform titleRect)
		{
			if (rootRect != null && (rootRect.sizeDelta.x <= 1f || rootRect.sizeDelta.y <= 1f))
			{
				rootRect.anchorMin = new Vector2(0.5f, 0.5f);
				rootRect.anchorMax = new Vector2(0.5f, 0.5f);
				rootRect.pivot = new Vector2(0.5f, 0.5f);
				rootRect.sizeDelta = new Vector2(LegacyRootWidth, LegacyRootHeight);
				rootRect.localScale = Vector3.one;
			}

			if (textureRect != null)
			{
				textureRect.anchorMin = new Vector2(0.5f, 0.5f);
				textureRect.anchorMax = new Vector2(0.5f, 0.5f);
				textureRect.pivot = new Vector2(0.5f, 0.5f);
				if (textureRect.sizeDelta.x <= 1f || textureRect.sizeDelta.y <= 1f)
				{
					textureRect.sizeDelta = new Vector2(LegacyThumbnailWidth, LegacyThumbnailHeight);
					textureRect.anchoredPosition = new Vector2(0f, LegacyThumbnailY);
				}
				textureRect.localScale = Vector3.one;
			}

			if (titleRect != null)
			{
				titleRect.anchorMin = new Vector2(0.5f, 0f);
				titleRect.anchorMax = new Vector2(0.5f, 0f);
				titleRect.pivot = new Vector2(0.5f, 0f);
				titleRect.anchoredPosition = Vector2.zero;
				titleRect.sizeDelta = new Vector2(LegacyTitleWidth, LegacyTitleHeight);
				titleRect.localScale = Vector3.one;
			}
		}

		protected virtual void ApplyCustomRuntimeLayout(RectTransform rootRect, RectTransform textureRect, RectTransform titleRect)
		{
			if (rootRect != null && rootRect.sizeDelta.x <= 1f && rootRect.sizeDelta.y <= 1f)
			{
				rootRect.sizeDelta = new Vector2(CustomRootWidth, CustomRootHeight);
				rootRect.localScale = Vector3.one;
			}

			if (textureRect != null)
			{
				textureRect.anchorMin = new Vector2(0.5f, 0.5f);
				textureRect.anchorMax = new Vector2(0.5f, 0.5f);
				textureRect.pivot = new Vector2(0.5f, 0.5f);
				textureRect.sizeDelta = new Vector2(CustomThumbnailWidth, CustomThumbnailHeight);
				textureRect.anchoredPosition = new Vector2(0f, CustomThumbnailY);
				textureRect.localScale = Vector3.one;
			}

			if (titleRect != null)
			{
				titleRect.anchorMin = new Vector2(0.5f, 0f);
				titleRect.anchorMax = new Vector2(0.5f, 0f);
				titleRect.pivot = new Vector2(0.5f, 0f);
				titleRect.anchoredPosition = new Vector2(0f, CustomTitleBottom);
				titleRect.sizeDelta = new Vector2(CustomTitleWidth, CustomTitleHeight);
				titleRect.localScale = Vector3.one;
			}

			if (title != null)
			{
				title.alignment = TextAnchor.MiddleCenter;
			}
			if (titleTmp != null)
			{
				titleTmp.alignment = TextAlignmentOptions.Center;
			}
		}

		protected virtual RectTransform ResolveTextRect()
		{
			if (titleTmp != null) return titleTmp.transform as RectTransform;
			return title != null ? title.transform as RectTransform : null;
		}

		protected virtual GameObject CreateLockOverlay()
		{
			GameObject overlay = new GameObject("__RuntimeLockOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			overlay.transform.SetParent(transform, false);

			RectTransform rect = overlay.transform as RectTransform;
			if (rect != null)
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.offsetMin = Vector2.zero;
				rect.offsetMax = Vector2.zero;
				rect.localScale = Vector3.one;
			}

			Image bg = overlay.GetComponent<Image>();
			bg.color = new Color(0f, 0f, 0f, 0.55f);
			bg.raycastTarget = false;

			GameObject labelObject = new GameObject("LockLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			labelObject.transform.SetParent(overlay.transform, false);
			Text label = labelObject.GetComponent<Text>();
			label.text = "LOCK";
			label.font = ResolveFont();
			label.fontSize = 28;
			label.alignment = TextAnchor.MiddleCenter;
			label.color = Color.white;
			label.raycastTarget = false;

			RectTransform labelRect = labelObject.transform as RectTransform;
			labelRect.anchorMin = Vector2.zero;
			labelRect.anchorMax = Vector2.one;
			labelRect.offsetMin = Vector2.zero;
			labelRect.offsetMax = Vector2.zero;
			labelRect.localScale = Vector3.one;

			overlay.SetActive(false);
			return overlay;
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

		protected virtual Font ResolveFont()
		{
			Text source = title ?? lockLabel ?? GetComponentInChildren<Text>(true);
			if (source != null && source.font != null) return source.font;
			return Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 24);
		}

		protected virtual void ActivateTextObject(Text legacyText, TextMeshProUGUI textMeshPro)
		{
			if (legacyText != null) legacyText.gameObject.SetActive(true);
			if (textMeshPro != null) textMeshPro.gameObject.SetActive(true);
		}

		protected virtual void SanitizeNullSpriteImages(Transform root)
		{
			if (root == null) return;

			foreach (Image image in root.GetComponentsInChildren<Image>(true))
			{
				if (image == null) continue;
				if (image.sprite != null) continue;

				image.enabled = false;
				image.raycastTarget = false;
			}
		}

		protected virtual bool IsNullSpriteGraphic(Graphic graphic)
		{
			if (graphic == null) return true;
			Image image = graphic as Image;
			return image != null && image.sprite == null;
		}
	}
}
