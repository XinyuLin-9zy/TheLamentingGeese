using UnityEngine;
using UnityEngine.UI;
using Utage;

public class UI_PlotChapterElement : MonoBehaviour
{
	private const string RuntimeChapterLabelName = "__RuntimeChapterLabel";

	private const string RuntimeLockLabelBackgroundName = "__RuntimeLockLabelBg";

	private static Font displayLabelFont;

	private static Font lockLabelFont;

	private static Sprite lockLabelBackgroundSprite;

	public UI_PlotMap parent;

	public string tagName;

	public Button btn;

	public Transform unlockPanel;

	public Transform lockPanel;

	public Text text_unlock;

	private Text text_lock;

	private Text text_unlocked;

	private UguiButtonSe btnSe;

	public RectTransform pointLeft;

	public RectTransform pointMid;

	public RectTransform pointRight;

	[SerializeField]
	private Image img_bg;

	[SerializeField]
	private Image img_lock;

	[SerializeField]
	private Image img_highlight;

	private Sprite spi_lock;

	private Sprite spi_unlock;

	public bool IsLocking { get; private set; }

	private bool initialized;

	public void Init()
	{
		if (initialized)
		{
			InitState();
			return;
		}

		if (parent == null) parent = GetComponentInParent<UI_PlotMap>(true);
		if (btn == null) btn = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
		if (btn == null) btn = gameObject.AddComponent<Button>();
		if (btn.targetGraphic == null)
		{
			btn.targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
		}
		btn.onClick.RemoveListener(OnClickChapter);
		btn.onClick.AddListener(OnClickChapter);

		if (unlockPanel == null) unlockPanel = FindChildRecursive(transform, "UnLock") ?? FindChildRecursive(transform, "Unlock");
		if (lockPanel == null) lockPanel = FindChildRecursive(transform, "Lock");
		Text unlockedText = FindText(unlockPanel);
		Text lockedText = FindText(lockPanel);
		if (text_unlock == null) text_unlock = unlockedText ?? lockedText ?? GetComponentInChildren<Text>(true);
		text_lock = lockedText;
		text_unlocked = unlockedText;
		if (img_bg == null) img_bg = GetComponent<Image>();
		if (img_lock == null && lockPanel != null) img_lock = lockPanel.GetComponent<Image>() ?? lockPanel.GetComponentInChildren<Image>(true);
		if (img_highlight == null)
		{
			img_highlight = FindOwnedHighlight();
		}

		initialized = true;
		InitState();
	}

	public void InitState()
	{
		IsLocking = !string.IsNullOrEmpty(tagName) && !PlotMapProgressStore.IsUnlocked(tagName);

		if (unlockPanel != null) unlockPanel.gameObject.SetActive(!IsLocking);
		if (lockPanel != null) lockPanel.gameObject.SetActive(IsLocking);
		if (img_lock != null) img_lock.gameObject.SetActive(IsLocking);
		if (btn != null) btn.interactable = !IsLocking;

		if (!IsLocking)
		{
			ConfigureUnlockedTitleImages();
			HideRuntimeLockLabelBackground();
		}

		Text label = IsLocking
			? EnsureLabelText(lockPanel, ref text_lock)
			: ResolveUnlockedLabel();
		ConfigureLabel(label);

		if (!IsLocking && text_unlock != null && text_unlock != label && IsLabelInPanel(text_unlock, unlockPanel))
		{
			ConfigureLabel(text_unlock);
		}
		NormalizePanelTexts(lockPanel, IsLocking);
		NormalizePanelTexts(unlockPanel, !IsLocking);
	}

	public void SetFocusState(bool focus)
	{
		if (img_highlight == null)
		{
			img_highlight = FindOwnedHighlight();
		}
		if (img_highlight == null && focus)
		{
			img_highlight = CreateRuntimeHighlight();
		}
		if (img_highlight != null)
		{
			img_highlight.gameObject.SetActive(focus);
		}
		foreach (Image image in GetComponentsInChildren<Image>(true))
		{
			if (image == null || image == img_highlight) continue;
			if (image.GetComponentInParent<UI_PlotChapterElement>(true) != this) continue;
			if (image.name.IndexOf("Highlight", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				image.gameObject.SetActive(focus);
			}
		}
	}

	public RectTransform GetPoint(UI.ElementPointPos pos)
	{
		switch (pos)
		{
			case UI.ElementPointPos.Left:
				if (pointLeft != null) return pointLeft;
				break;
			case UI.ElementPointPos.Right:
				if (pointRight != null) return pointRight;
				break;
			default:
				if (pointMid != null) return pointMid;
				break;
		}

		if (pointMid != null) return pointMid;
		if (pointLeft != null) return pointLeft;
		if (pointRight != null) return pointRight;
		return transform as RectTransform;
	}

	private void OnClickChapter()
	{
		if (IsLocking) return;
		if (parent != null)
		{
			parent.ShowMap(false, tagName);
		}
	}

	private Text EnsureLabelText(Transform panel, ref Text label)
	{
		if (label != null) return label;

		label = FindText(panel);
		if (label != null) return label;

		Transform parentTransform = panel != null ? panel : transform;
		GameObject labelObject = new GameObject(RuntimeChapterLabelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
		labelObject.transform.SetParent(parentTransform, false);
		label = labelObject.GetComponent<Text>();
		label.raycastTarget = false;
		return label;
	}

	private Text ResolveUnlockedLabel()
	{
		if (HasTitleImage(unlockPanel))
		{
			HideUnlockPanelTextLabels();
			return null;
		}

		if (text_unlocked != null && IsLabelInPanel(text_unlocked, unlockPanel)) return text_unlocked;

		text_unlocked = FindText(unlockPanel);
		if (text_unlocked != null) return text_unlocked;

		return EnsureLabelText(unlockPanel, ref text_unlocked);
	}

	private void ConfigureUnlockedTitleImages()
	{
		if (unlockPanel == null) return;

		foreach (Image image in unlockPanel.GetComponentsInChildren<Image>(true))
		{
			if (!IsTitleImage(image)) continue;

			image.preserveAspect = true;
			image.raycastTarget = false;
			image.SetVerticesDirty();
		}
	}

	private bool HasTitleImage(Transform panel)
	{
		if (panel == null) return false;

		foreach (Image image in panel.GetComponentsInChildren<Image>(true))
		{
			if (IsTitleImage(image)) return true;
		}
		return false;
	}

	private bool IsTitleImage(Image image)
	{
		if (image == null || image.sprite == null) return false;
		if (image.GetComponentInParent<UI_PlotChapterElement>(true) != this) return false;

		string objectName = image.gameObject.name;
		return string.Equals(objectName, "Name", System.StringComparison.OrdinalIgnoreCase)
			|| string.Equals(objectName, "Title", System.StringComparison.OrdinalIgnoreCase);
	}

	private void HideUnlockPanelTextLabels()
	{
		if (unlockPanel == null) return;

		foreach (Text label in unlockPanel.GetComponentsInChildren<Text>(true))
		{
			if (label == null) continue;

			label.raycastTarget = false;
			label.gameObject.SetActive(false);
		}
	}

	private void HideRuntimeLockLabelBackground()
	{
		Transform root = lockPanel != null ? lockPanel : transform;
		Transform runtimeBackground = FindChildRecursive(root, RuntimeLockLabelBackgroundName);
		if (runtimeBackground == null) return;

		Image image = runtimeBackground.GetComponent<Image>();
		if (image != null)
		{
			image.enabled = false;
		}
		runtimeBackground.gameObject.SetActive(false);
	}

	private bool IsLabelInPanel(Text label, Transform panel)
	{
		return label != null && panel != null && label.transform.IsChildOf(panel);
	}

	private void ConfigureLabel(Text label)
	{
		if (label == null) return;

		bool useRuntimeLayout = IsRuntimeLabel(label) || IsGeneratedLabelText(label.text);
		string displayName = GetDisplayName();
		if (!string.IsNullOrEmpty(displayName) && ShouldReplaceLabelText(label))
		{
			label.text = displayName;
		}

		Font displayFont = ResolveDisplayFont(label);
		if (displayFont != null) label.font = displayFont;
		label.material = null;
		label.raycastTarget = false;
		if (IsLocking)
		{
			ConfigureLockLabel(label);
			return;
		}

		label.color = Color.white;
		if (label.font != null && !string.IsNullOrEmpty(label.text))
		{
			label.font.RequestCharactersInTexture(label.text, label.fontSize, label.fontStyle);
		}
		label.SetLayoutDirty();
		label.SetVerticesDirty();

		if (!useRuntimeLayout) return;

		label.fontSize = Mathf.Max(label.fontSize, 22);
		label.alignment = TextAnchor.MiddleCenter;
		label.horizontalOverflow = HorizontalWrapMode.Wrap;
		label.verticalOverflow = VerticalWrapMode.Overflow;
		label.resizeTextForBestFit = true;
		label.resizeTextMinSize = 14;
		label.resizeTextMaxSize = Mathf.Max(label.fontSize, 26);

		RectTransform rect = label.transform as RectTransform;
		if (rect != null)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(18f, 10f);
			rect.offsetMax = new Vector2(-18f, -10f);
			rect.localScale = Vector3.one;
		}
	}

	private bool ShouldReplaceLabelText(Text label)
	{
		if (label == null) return false;
		if (IsRuntimeLabel(label)) return true;
		if (!IsLocking) return IsGeneratedLabelText(label.text);
		return IsGeneratedLabelText(label.text);
	}

	private bool IsRuntimeLabel(Text label)
	{
		return label != null && string.Equals(label.gameObject.name, RuntimeChapterLabelName, System.StringComparison.Ordinal);
	}

	private bool IsGeneratedLabelText(string value)
	{
		string text = value == null ? "" : value.Trim();
		if (string.IsNullOrEmpty(text)) return true;
		if (string.Equals(text, "Text", System.StringComparison.OrdinalIgnoreCase)) return true;
		if (string.Equals(text, "New Text", System.StringComparison.OrdinalIgnoreCase)) return true;
		if (LooksLikeLockConditionText(text)) return true;
		if (LooksLikePlaceholderText(text)) return true;
		if (!string.IsNullOrEmpty(tagName) && string.Equals(text, tagName, System.StringComparison.OrdinalIgnoreCase)) return true;
		return string.Equals(text, gameObject.name, System.StringComparison.OrdinalIgnoreCase);
	}

	private void ConfigureLockLabel(Text label)
	{
		if (label == null) return;

		string displayName = GetDisplayName();
		string lockLabel = ResolveLockLabel(displayName);
		if (!string.IsNullOrEmpty(lockLabel))
		{
			label.text = lockLabel;
		}

		label.font = ResolveLockFont();
		label.material = null;
		label.alignment = TextAnchor.MiddleCenter;
		label.horizontalOverflow = HorizontalWrapMode.Overflow;
		label.verticalOverflow = VerticalWrapMode.Overflow;
		label.resizeTextForBestFit = true;
		label.resizeTextMinSize = 12;
		label.fontSize = Mathf.Clamp(label.fontSize > 0 ? label.fontSize : 16, 15, 18);
		label.resizeTextMaxSize = label.fontSize;
		label.color = Color.black;
		label.raycastTarget = false;
		if (label.font != null && !string.IsNullOrEmpty(label.text))
		{
			label.font.RequestCharactersInTexture(label.text, label.fontSize, label.fontStyle);
		}
		label.SetLayoutDirty();
		label.SetVerticesDirty();
		Canvas.ForceUpdateCanvases();

		RectTransform background = EnsureLockLabelBackground(label);
		FitLockLabelBackground(background, label);

		RectTransform rect = label.transform as RectTransform;
		if (rect != null)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(8f, 2f);
			rect.offsetMax = new Vector2(-8f, -2f);
			rect.localScale = Vector3.one;
		}
		label.SetLayoutDirty();
		label.SetVerticesDirty();
	}

	private void NormalizePanelTexts(Transform panel, bool panelActive)
	{
		if (panel == null) return;

		string displayName = GetDisplayName();
		bool panelUsesImageTitle = panelActive && panel == unlockPanel && HasTitleImage(panel);
		foreach (Text label in panel.GetComponentsInChildren<Text>(true))
		{
			if (label == null) continue;
			label.raycastTarget = false;

			if (panelUsesImageTitle)
			{
				label.gameObject.SetActive(false);
				continue;
			}

			if (panelActive && LooksLikePlaceholderText(label.text))
			{
				label.text = IsLocking ? ResolveLockLabel(displayName) : displayName;
			}
			else if (!panelActive && IsGeneratedLabelText(label.text))
			{
				label.text = "";
			}

			if (!panelActive)
			{
				if (IsRuntimeLabel(label))
				{
					label.gameObject.SetActive(false);
				}
				continue;
			}

			if (!label.gameObject.activeSelf)
			{
				label.gameObject.SetActive(true);
			}

			if (panel == lockPanel)
			{
				ConfigureLockLabel(label);
			}
			else
			{
				ConfigureLabel(label);
			}
		}
	}

	private string ResolveLockLabel(string displayName)
	{
		string key = "";
		if (!string.IsNullOrEmpty(tagName))
		{
			key = "unlock_" + PlotMapProgressStore.Normalize(tagName);
		}

		if (!string.IsNullOrEmpty(key) && LanguageManagerBase.Instance != null)
		{
			string localized;
			if (LanguageManagerBase.Instance.TryLocalizeText(key, out localized)
				&& !string.IsNullOrEmpty(localized)
				&& !string.Equals(localized, key, System.StringComparison.Ordinal))
			{
				return localized.Trim();
			}
		}

		return BuildLockLabel(displayName);
	}

	private RectTransform EnsureLockLabelBackground(Text label)
	{
		if (label == null) return null;
		if (lockPanel != null && !label.transform.IsChildOf(lockPanel)) return null;

		Transform container = lockPanel != null ? lockPanel : transform;
		RectTransform reusableBackground = FindReusableLockLabelBackground(label);
		Transform runtimeBackground = FindChildRecursive(container, RuntimeLockLabelBackgroundName);
		Transform existing = reusableBackground != null ? reusableBackground : runtimeBackground;
		if (existing == null)
		{
			GameObject backgroundObject = new GameObject(RuntimeLockLabelBackgroundName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			backgroundObject.transform.SetParent(container, false);
			existing = backgroundObject.transform;
		}
		else if (runtimeBackground != null && runtimeBackground != existing && !label.transform.IsChildOf(runtimeBackground))
		{
			runtimeBackground.gameObject.SetActive(false);
		}

		RectTransform backgroundRect = existing as RectTransform;
		if (backgroundRect == null) return null;
		backgroundRect.gameObject.SetActive(true);

		Image backgroundImage = existing.GetComponent<Image>();
		if (backgroundImage == null)
		{
			backgroundImage = existing.gameObject.AddComponent<Image>();
		}

		if (label.transform.parent != backgroundRect)
		{
			RectTransform sourceRect = null;
			Transform parentTransform = label.transform.parent;
			if (parentTransform != null
				&& parentTransform != transform
				&& parentTransform != lockPanel
				&& parentTransform.GetComponentInParent<UI_PlotChapterElement>(true) == this)
			{
				sourceRect = parentTransform as RectTransform;
			}
			if (sourceRect == null)
			{
				sourceRect = label.transform as RectTransform;
			}
			CopyRectPlacement(backgroundRect, sourceRect);
			label.transform.SetParent(backgroundRect, false);
		}

		Sprite backgroundSprite = ResolveLockLabelBackgroundSprite();
		if (backgroundImage.sprite == null && backgroundSprite != null)
		{
			backgroundImage.sprite = backgroundSprite;
		}
		backgroundImage.enabled = true;
		backgroundImage.raycastTarget = false;
		if (backgroundImage.sprite != null && backgroundImage.sprite.border != Vector4.zero)
		{
			backgroundImage.type = Image.Type.Sliced;
		}

		return backgroundRect;
	}

	private RectTransform FindReusableLockLabelBackground(Text label)
	{
		if (label == null) return null;

		RectTransform parentRect = label.transform.parent as RectTransform;
		if (parentRect == null || parentRect == lockPanel) return null;
		if (lockPanel != null && !parentRect.IsChildOf(lockPanel)) return null;
		if (parentRect.GetComponentInParent<UI_PlotChapterElement>(true) != this) return null;

		Image image = parentRect.GetComponent<Image>();
		return image != null ? parentRect : null;
	}

	private void FitLockLabelBackground(RectTransform background, Text label)
	{
		if (background == null || label == null) return;

		label.SetLayoutDirty();
		label.SetVerticesDirty();
		Canvas.ForceUpdateCanvases();

		float preferredWidth = Mathf.Clamp(GetPreferredTextWidth(label) + 44f, 220f, 760f);
		float preferredHeight = Mathf.Clamp(GetPreferredTextHeight(label) + 14f, 34f, 56f);
		if (float.IsNaN(preferredWidth) || preferredWidth <= 0f) preferredWidth = 372f;
		if (float.IsNaN(preferredHeight) || preferredHeight <= 0f) preferredHeight = 40f;

		bool needsLayoutRepair = background.rect.width < 8f || background.rect.height < 8f;
		if (needsLayoutRepair)
		{
			background.anchorMin = new Vector2(0.5f, 0f);
			background.anchorMax = new Vector2(0.5f, 0f);
			background.pivot = new Vector2(0.5f, 0f);
		}

		background.sizeDelta = new Vector2(preferredWidth, preferredHeight);
		background.localScale = Vector3.one;
	}

	private void CopyRectPlacement(RectTransform target, RectTransform source)
	{
		if (target == null || source == null) return;

		target.anchorMin = source.anchorMin;
		target.anchorMax = source.anchorMax;
		target.pivot = source.pivot;
		target.anchoredPosition = source.anchoredPosition;
		target.sizeDelta = source.sizeDelta;
		target.localRotation = source.localRotation;
		target.localScale = Vector3.one;
	}

	private float GetPreferredTextWidth(Text label)
	{
		if (label == null) return 0f;

		float width = label.preferredWidth;
		if (!float.IsNaN(width) && width > 0f) return width;

		TextGenerationSettings settings = label.GetGenerationSettings(new Vector2(2000f, 200f));
		return label.cachedTextGeneratorForLayout.GetPreferredWidth(label.text, settings) / Mathf.Max(0.01f, label.pixelsPerUnit);
	}

	private float GetPreferredTextHeight(Text label)
	{
		if (label == null) return 0f;

		float height = label.preferredHeight;
		if (!float.IsNaN(height) && height > 0f) return height;

		TextGenerationSettings settings = label.GetGenerationSettings(new Vector2(760f, 200f));
		return label.cachedTextGeneratorForLayout.GetPreferredHeight(label.text, settings) / Mathf.Max(0.01f, label.pixelsPerUnit);
	}

	private string BuildLockLabel(string displayName)
	{
		return string.IsNullOrEmpty(displayName)
			? ""
			: "\u89e3\u9501\u6761\u4ef6\uff1a" + displayName + " \u9009\u9879\u89e3\u9501";
	}

	private bool LooksLikePlaceholderText(string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return true;

		string text = value.Trim();
		if (text.IndexOf("dsf", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text.Length >= 18 && text.IndexOf(' ') < 0 && text.IndexOf('，') < 0 && text.IndexOf('。') < 0)
		{
			int latinLetters = 0;
			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) ++latinLetters;
			}
			return latinLetters >= text.Length * 0.7f;
		}
		return false;
	}

	private bool LooksLikeLockConditionText(string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return false;

		string text = value.Trim();
		if (text.StartsWith("unlock_", System.StringComparison.OrdinalIgnoreCase)) return true;
		if (text.IndexOf("\u89e3\u9501\u6761\u4ef6", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text.IndexOf("\u89e3\u9396\u689d\u4ef6", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text.IndexOf("\u9009\u9879\u89e3\u9501", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text.IndexOf("\u9078\u9805\u89e3\u9396", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
		return text.IndexOf("unlock condition", System.StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private string GetDisplayName()
	{
		if (!string.IsNullOrEmpty(gameObject.name)
			&& gameObject.name != "Btn"
			&& !gameObject.name.StartsWith("UI_", System.StringComparison.Ordinal))
		{
			return gameObject.name;
		}

		return string.IsNullOrEmpty(tagName) ? "" : tagName;
	}

	private Font ResolveFont()
	{
		Text source = text_unlock ?? text_lock ?? text_unlocked ?? GetComponentInChildren<Text>(true);
		if (source != null && source.font != null) return source.font;

		return Font.CreateDynamicFontFromOSFont(
			new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" },
			24);
	}

	private Font ResolveDisplayFont(Text label)
	{
		if (displayLabelFont != null) return displayLabelFont;

		Font preferred = FindPreferredLoadedLockFont();
		if (preferred != null)
		{
			displayLabelFont = preferred;
			return displayLabelFont;
		}

		Font sourceFont = label != null ? label.font : null;
		if (sourceFont != null && GetLockFontScore(sourceFont.name) > 0)
		{
			displayLabelFont = sourceFont;
			return displayLabelFont;
		}

		displayLabelFont = Font.CreateDynamicFontFromOSFont(
			new[]
			{
				"Source Han Serif SC",
				"Source Han Serif CN",
				"Noto Sans CJK SC",
				"Noto Sans SC",
				"Microsoft YaHei UI",
				"Microsoft YaHei",
				"\u5fae\u8f6f\u96c5\u9ed1",
				"SimHei",
				"\u9ed1\u4f53",
				"Arial Unicode MS",
				"Arial"
			},
			24);
		if (displayLabelFont != null)
		{
			return displayLabelFont;
		}

		return ResolveFont();
	}

	private Font ResolveLockFont()
	{
		if (lockLabelFont != null) return lockLabelFont;

		lockLabelFont = Font.CreateDynamicFontFromOSFont(
			new[]
			{
				"Microsoft YaHei UI",
				"Microsoft YaHei",
				"\u5fae\u8f6f\u96c5\u9ed1",
				"SimHei",
				"\u9ed1\u4f53",
				"Arial Unicode MS",
				"Arial"
			},
			18);
		if (lockLabelFont != null) return lockLabelFont;

		lockLabelFont = FindPreferredLoadedLockFont();
		if (lockLabelFont != null) return lockLabelFont;

		Font sourceFont = ResolveFont();
		if (sourceFont != null && GetLockFontScore(sourceFont.name) > 0)
		{
			lockLabelFont = sourceFont;
			return lockLabelFont;
		}

		lockLabelFont = Font.CreateDynamicFontFromOSFont(
			new[]
			{
				"Source Han Serif SC",
				"Source Han Serif CN",
				"Noto Sans CJK SC",
				"Noto Sans SC",
				"Microsoft YaHei UI",
				"Microsoft YaHei",
				"\u5fae\u8f6f\u96c5\u9ed1",
				"SimHei",
				"\u9ed1\u4f53",
				"Arial Unicode MS",
				"Arial"
			},
			18);

		return lockLabelFont != null ? lockLabelFont : ResolveFont();
	}

	private Font FindPreferredLoadedLockFont()
	{
		Font bestFont = null;
		int bestScore = 0;
		foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
		{
			if (font == null) continue;
			int score = GetLockFontScore(font.name);
			if (score <= bestScore) continue;
			bestFont = font;
			bestScore = score;
		}
		return bestFont;
	}

	private int GetLockFontScore(string fontName)
	{
		if (string.IsNullOrEmpty(fontName)) return 0;
		if (fontName.IndexOf("SourceHanSerifSC", System.StringComparison.OrdinalIgnoreCase) >= 0) return 100;
		if (fontName.IndexOf("SourceHanSerifCN", System.StringComparison.OrdinalIgnoreCase) >= 0) return 96;
		if (fontName.IndexOf("Source Han Serif SC", System.StringComparison.OrdinalIgnoreCase) >= 0) return 94;
		if (fontName.IndexOf("Source Han Serif CN", System.StringComparison.OrdinalIgnoreCase) >= 0) return 92;
		if (fontName.IndexOf("SourceHanSerif", System.StringComparison.OrdinalIgnoreCase) >= 0) return 90;
		if (fontName.IndexOf("Noto Sans CJK", System.StringComparison.OrdinalIgnoreCase) >= 0) return 82;
		if (fontName.IndexOf("Noto Sans SC", System.StringComparison.OrdinalIgnoreCase) >= 0) return 80;
		if (fontName.IndexOf("Microsoft YaHei", System.StringComparison.OrdinalIgnoreCase) >= 0) return 70;
		if (fontName.IndexOf("SimHei", System.StringComparison.OrdinalIgnoreCase) >= 0) return 64;
		if (fontName.IndexOf("\u5fae\u8f6f\u96c5\u9ed1", System.StringComparison.OrdinalIgnoreCase) >= 0) return 70;
		if (fontName.IndexOf("\u9ed1\u4f53", System.StringComparison.OrdinalIgnoreCase) >= 0) return 64;
		if (fontName.IndexOf("\u559c\u9e4a", System.StringComparison.OrdinalIgnoreCase) >= 0) return 58;
		return 0;
	}

	private Sprite ResolveLockLabelBackgroundSprite()
	{
		if (lockLabelBackgroundSprite != null) return lockLabelBackgroundSprite;

		foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
		{
			if (sprite == null) continue;
			if (string.Equals(sprite.name, "flowchart_unlockbg", System.StringComparison.OrdinalIgnoreCase))
			{
				lockLabelBackgroundSprite = sprite;
				break;
			}
		}
		return lockLabelBackgroundSprite;
	}

	private Image FindOwnedHighlight()
	{
		foreach (Image image in GetComponentsInChildren<Image>(true))
		{
			if (image == null) continue;
			if (image.name.IndexOf("Highlight", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
			if (image.GetComponentInParent<UI_PlotChapterElement>(true) != this) continue;
			return image;
		}
		return null;
	}

	private Image CreateRuntimeHighlight()
	{
		GameObject highlightObject = new GameObject("Highlight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		highlightObject.transform.SetParent(transform, false);
		highlightObject.transform.SetAsLastSibling();

		RectTransform rect = highlightObject.transform as RectTransform;
		if (rect != null)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;
		}

		Image image = highlightObject.GetComponent<Image>();
		image.color = new Color(1f, 0.86f, 0.34f, 0.22f);
		image.raycastTarget = false;
		return image;
	}

	private static Text FindText(Transform root)
	{
		return root != null ? root.GetComponentInChildren<Text>(true) : null;
	}

	private static Transform FindChildRecursive(Transform root, string name)
	{
		if (root == null) return null;
		if (root.name == name) return root;

		foreach (Transform child in root)
		{
			Transform found = FindChildRecursive(child, name);
			if (found != null) return found;
		}
		return null;
	}
}
