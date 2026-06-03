using System.Collections.Generic;
using System.Globalization;
using Config;
using DG.Tweening;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

public class UI_PlotMap : UguiView, IMediator
{
	public Button btn_unlockAll;

	public Button btn_resetSteamAchievement;

	public Button btn_resetPlotProcess;

	[SerializeField]
	protected AdvEngine engine;

	public UtageUguiMainGame mainGame;

	public Button btnBack;

	public Text text_processTitle;

	public Text text_processValue;

	private LayoutGroup[] layoutGroups;

	private ContentSizeFitter[] sizeFitters;

	private List<Image> chapterElements;

	public Scrollbar scrollBar;

	public Camera uiCamera;

	private GamepadElement[] gamepadElements;

	private UI_PlotChapterElement[] plotChapterElements;

	private UI.UI_PlotMapElementLine[] plotElementLines;

	public UAP_BaseElement enterAccessibility;

	private GamepadRoot gamepadRoot;

	public ChapterDefaultMoneyData defaultMoneyData;

	private Tween _tween;

	private float contentLength;

	private bool inTitle;

	private string chapterName;

	private RectTransform scrollViewRect;

	private RectTransform viewportRect;

	private RectTransform contentRect;

	private ScrollRect scrollRect;

	private bool runtimeReferencesInitialized;

	private bool suppressScrollbarCallback;

	public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);

	private GamepadElement[] GamepadElements
	{
		get
		{
			if (gamepadElements == null) gamepadElements = GetComponentsInChildren<GamepadElement>(true);
			return gamepadElements;
		}
	}

	private GamepadRoot GamepadRoot
	{
		get
		{
			if (gamepadRoot == null) gamepadRoot = GetComponentInChildren<GamepadRoot>(true);
			return gamepadRoot;
		}
	}

	public string MediatorName => nameof(UI_PlotMap);

	public object ViewComponent { get; set; }

	public void UnlockAllPlotMap()
	{
		EnsureRuntimeReferences();
		foreach (UI_PlotChapterElement element in plotChapterElements)
		{
			if (element != null && !string.IsNullOrEmpty(element.tagName))
			{
				PlotMapProgressStore.Unlock(element.tagName);
			}
		}
		RefreshUI();
	}

	public void ResetSteamAchievement()
	{
	}

	public void ResetPlotProcess()
	{
		ClearArchive();
	}

	public void ClearArchive()
	{
		PlotMapProgressStore.ClearAll();
		RefreshUI();
	}

	private void Awake()
	{
		EnsureRuntimeReferences();
	}

	private void OnEnable()
	{
		runtimeReferencesInitialized = false;
		EnsureRuntimeReferences();
		RefreshUI();
	}

	private void OnSelectLabel(GamepadElement gamepadElement)
	{
	}

	private void OnDeselecting(GamepadElement gamepadElement)
	{
	}

	private void FocusTarget()
	{
		EnsureRuntimeReferences();
		if (plotChapterElements == null || plotChapterElements.Length == 0) return;

		string targetName = PlotMapProgressStore.Normalize(!string.IsNullOrEmpty(chapterName)
			? chapterName
			: PlotMapProgressStore.CurrentChapterName);

		UI_PlotChapterElement target = null;
		UI_PlotChapterElement firstAvailable = null;
		UI_PlotChapterElement firstElement = null;
		foreach (UI_PlotChapterElement element in plotChapterElements)
		{
			if (element == null) continue;
			if (firstElement == null) firstElement = element;
			if (firstAvailable == null && !element.IsLocking) firstAvailable = element;
			element.SetFocusState(false);

			if (target == null && MatchesChapter(element, targetName))
			{
				target = element;
			}
		}

		target = target ?? firstAvailable ?? firstElement;
		if (target == null) return;

		target.SetFocusState(true);
		ScrollToElement(target);
	}

	private void RefreshUI()
	{
		EnsureRuntimeReferences();

		if (plotChapterElements != null)
		{
			foreach (UI_PlotChapterElement element in plotChapterElements)
			{
				if (element == null) continue;
				element.parent = this;
				element.Init();
			}
		}

		Canvas.ForceUpdateCanvases();
		RefreshPlotElementLines();
		RefreshStaticFlowchartLines();
		UpdateProcessText();
		UpdateScrollMetrics();
	}

	private void OnDisable()
	{
		if (_tween != null)
		{
			_tween.Kill();
			_tween = null;
		}
	}

	private void Update()
	{
		if (InputUtil.IsInputGuiClose())
		{
			Back();
		}
	}

	public void ShowMap(bool inTitle, string chapterName = "")
	{
		this.inTitle = inTitle;
		this.chapterName = chapterName;
		EnsureRuntimeReferences();
		RefreshUI();
		FocusTarget();
	}

	public void ShowMap(bool inTitle)
	{
		ShowMap(inTitle, "");
	}

	private int GetDefaultMoney(string tagName)
	{
		if (defaultMoneyData != null && defaultMoneyData.defaultMoneyDict != null && defaultMoneyData.defaultMoneyDict.ContainsKey(tagName))
		{
			return defaultMoneyData.defaultMoneyDict[tagName];
		}
		return 0;
	}

	public string[] ListNotificationInterests()
	{
		return new string[0];
	}

	public void HandleNotification(INotification notification)
	{
	}

	public void OnRegister()
	{
	}

	public void OnRemove()
	{
	}

	public override void OnTapBack()
	{
		Back();
	}

	private void EnsureRuntimeReferences()
	{
		if (runtimeReferencesInitialized) return;

		if (mainGame == null) mainGame = FindSceneObject<UtageUguiMainGame>();
		if (btnBack == null) btnBack = FindButton("BtnBack", "CloseButton", "Back", "ButtonBack");
		if (text_processTitle == null) text_processTitle = FindText("Text_Process", "ProcessTitle", "Text_ProcessTitle");
		if (text_processValue == null) text_processValue = FindText("Text_ProcessValue", "ProcessValue");
		BindButton(btnBack, OnTapBack);
		BindButton(btn_unlockAll, UnlockAllPlotMap);
		BindButton(btn_resetPlotProcess, ResetPlotProcess);
		BindButton(btn_resetSteamAchievement, ResetSteamAchievement);

		if (scrollViewRect == null)
		{
			Transform scrollView = FindChildRecursive(transform, "Scroll View");
			scrollViewRect = scrollView as RectTransform;
		}
		if (viewportRect == null)
		{
			Transform viewport = scrollViewRect != null
				? FindChildRecursive(scrollViewRect, "Viewport")
				: FindChildRecursive(transform, "Viewport");
			viewportRect = viewport as RectTransform;
		}
		if (contentRect == null)
		{
			Transform content = viewportRect != null
				? FindChildRecursive(viewportRect, "Content")
				: FindChildRecursive(transform, "Content");
			contentRect = content as RectTransform;
		}

		if (scrollBar == null || !IsUsablePlotScrollbar(scrollBar))
		{
			scrollBar = FindOrCreateHorizontalScrollbar();
		}
		else
		{
			scrollBar = EnsureScrollbarComponent(scrollBar.transform);
		}

		ConfigureScrollbar();
		ConfigureScrollRect();
		DisableStrayScrollbars();

		layoutGroups = GetComponentsInChildren<LayoutGroup>(true);
		sizeFitters = GetComponentsInChildren<ContentSizeFitter>(true);
		Transform plotRoot = contentRect != null ? contentRect : transform;
		chapterElements = new List<Image>(plotRoot.GetComponentsInChildren<Image>(true));
		plotChapterElements = plotRoot.GetComponentsInChildren<UI_PlotChapterElement>(true);
		plotElementLines = plotRoot.GetComponentsInChildren<UI.UI_PlotMapElementLine>(true);
		gamepadElements = GetComponentsInChildren<GamepadElement>(true);

		runtimeReferencesInitialized = true;
	}

	private void RefreshPlotElementLines()
	{
		if (plotElementLines == null || plotElementLines.Length == 0) return;

		foreach (UI.UI_PlotMapElementLine line in plotElementLines)
		{
			if (line == null) continue;
			line.SetPosition();
		}
	}

	private void RefreshStaticFlowchartLines()
	{
		if (chapterElements == null) return;

		foreach (Image image in chapterElements)
		{
			if (image == null || !IsStaticFlowchartLine(image)) continue;
			image.enabled = true;
			image.raycastTarget = false;

			Color color = image.color;
			if (color.a < 0.28f)
			{
				color.a = 0.72f;
				image.color = color;
			}
		}
	}

	private bool IsStaticFlowchartLine(Image image)
	{
		if (image == null) return false;
		string name = image.gameObject.name ?? "";
		return name.StartsWith("Line", System.StringComparison.OrdinalIgnoreCase)
			&& image.GetComponent<UI.UI_PlotMapElementLine>() == null;
	}

	private void ConfigureScrollbar()
	{
		if (scrollBar == null) return;

		scrollBar.gameObject.SetActive(true);
		scrollBar.enabled = true;
		scrollBar.direction = Scrollbar.Direction.LeftToRight;
		scrollBar.numberOfSteps = 0;
		scrollBar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
		scrollBar.onValueChanged.AddListener(OnScrollbarValueChanged);

		Graphic backgroundGraphic = scrollBar.GetComponent<Graphic>();
		if (backgroundGraphic != null)
		{
			backgroundGraphic.raycastTarget = true;
		}

		RectTransform handle = FindChildRecursive(scrollBar.transform, "Handle") as RectTransform;
		if (handle == null)
		{
			handle = CreateScrollbarHandle(scrollBar.transform);
		}
		if (handle != null)
		{
			scrollBar.handleRect = handle;
			Graphic handleGraphic = handle.GetComponent<Graphic>() ?? handle.GetComponentInChildren<Graphic>(true);
			if (handleGraphic == null)
			{
				Image handleImage = handle.gameObject.AddComponent<Image>();
				handleImage.color = Color.white;
				handleGraphic = handleImage;
			}
			if (handleGraphic != null)
			{
				handleGraphic.raycastTarget = true;
				scrollBar.targetGraphic = handleGraphic;
			}
		}
		else if (scrollBar.targetGraphic == null)
		{
			scrollBar.targetGraphic = backgroundGraphic;
		}

		StylePlotScrollbar(scrollBar);
	}

	private void StylePlotScrollbar(Scrollbar bar)
	{
		if (bar == null) return;

		const float trackWidth = 733f;
		const float trackHeight = 6f;
		const float handleHeight = 46f;

		RectTransform barRect = bar.transform as RectTransform;
		if (barRect != null)
		{
			barRect.anchorMin = new Vector2(0.5f, 0f);
			barRect.anchorMax = new Vector2(0.5f, 0f);
			barRect.pivot = new Vector2(0.5f, 0.5f);
			barRect.anchoredPosition = new Vector2(0f, 28f);
			barRect.sizeDelta = new Vector2(Mathf.Max(barRect.sizeDelta.x, trackWidth), handleHeight);
			barRect.localScale = Vector3.one;
			barRect.SetAsLastSibling();
		}

		Graphic backgroundGraphic = bar.GetComponent<Graphic>();
		Image trackImage = EnsureScrollbarTrack(bar, backgroundGraphic as Image, trackWidth, trackHeight);
		if (backgroundGraphic != null)
		{
			Color hitAreaColor = backgroundGraphic.color;
			hitAreaColor.a = backgroundGraphic.rectTransform == barRect ? 0f : 1f;
			backgroundGraphic.color = hitAreaColor;
			backgroundGraphic.raycastTarget = true;
			if (backgroundGraphic.rectTransform != barRect)
			{
				CompressHorizontalRect(backgroundGraphic.rectTransform, trackHeight);
			}
		}
		if (trackImage != null)
		{
			trackImage.enabled = true;
			trackImage.color = Color.white;
			trackImage.raycastTarget = false;
		}

		RectTransform handle = bar.handleRect;
		if (handle == null) return;

		RectTransform slidingArea = null;
		if (handle.parent != null && string.Equals(handle.parent.name, "Sliding Area", System.StringComparison.Ordinal))
		{
			slidingArea = handle.parent as RectTransform;
		}
		if (slidingArea == null)
		{
			slidingArea = FindChildRecursive(bar.transform, "Sliding Area") as RectTransform;
		}
		if (slidingArea != null)
		{
			slidingArea.anchorMin = Vector2.zero;
			slidingArea.anchorMax = Vector2.one;
			slidingArea.offsetMin = Vector2.zero;
			slidingArea.offsetMax = Vector2.zero;
			slidingArea.localScale = Vector3.one;
		}

		Graphic handleGraphic = handle.GetComponent<Graphic>() ?? handle.GetComponentInChildren<Graphic>(true);
		if (handleGraphic != null)
		{
			handleGraphic.color = Color.white;
			handleGraphic.raycastTarget = true;
		}

		handle.anchorMin = new Vector2(handle.anchorMin.x, 0f);
		handle.anchorMax = new Vector2(handle.anchorMax.x, 1f);
		handle.pivot = new Vector2(handle.pivot.x, 0.5f);
		float extraWidth = handle.sizeDelta.x;
		if (float.IsNaN(extraWidth) || extraWidth < 48f || extraWidth > 96f)
		{
			extraWidth = 48f;
		}
		handle.sizeDelta = new Vector2(extraWidth, 0f);
		Vector2 handleOffsetMin = handle.offsetMin;
		handleOffsetMin.y = 0f;
		handle.offsetMin = handleOffsetMin;
		Vector2 handleOffsetMax = handle.offsetMax;
		handleOffsetMax.y = 0f;
		handle.offsetMax = handleOffsetMax;
		handle.localScale = Vector3.one;
	}

	private Image EnsureScrollbarTrack(Scrollbar bar, Image sourceImage, float width, float height)
	{
		if (bar == null) return null;

		Transform existing = FindChildRecursive(bar.transform, "__RuntimeScrollbarTrack");
		if (existing == null)
		{
			GameObject trackObject = new GameObject("__RuntimeScrollbarTrack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			trackObject.transform.SetParent(bar.transform, false);
			existing = trackObject.transform;
		}

		existing.SetSiblingIndex(0);
		RectTransform trackRect = existing as RectTransform;
		if (trackRect != null)
		{
			trackRect.anchorMin = new Vector2(0.5f, 0.5f);
			trackRect.anchorMax = new Vector2(0.5f, 0.5f);
			trackRect.pivot = new Vector2(0.5f, 0.5f);
			trackRect.anchoredPosition = Vector2.zero;
			trackRect.sizeDelta = new Vector2(width, height);
			trackRect.localScale = Vector3.one;
		}

		Image trackImage = existing.GetComponent<Image>();
		if (trackImage == null) trackImage = existing.gameObject.AddComponent<Image>();
		if (sourceImage != null)
		{
			trackImage.sprite = sourceImage.sprite;
			trackImage.type = sourceImage.type;
			trackImage.preserveAspect = sourceImage.preserveAspect;
			trackImage.material = sourceImage.material;
		}
		return trackImage;
	}

	private void CompressHorizontalRect(RectTransform rectTransform, float height)
	{
		if (rectTransform == null) return;

		rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0.5f);
		rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0.5f);
		rectTransform.pivot = new Vector2(rectTransform.pivot.x, 0.5f);
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
		rectTransform.localScale = Vector3.one;
	}

	private void ConfigureScrollRect()
	{
		if (scrollViewRect == null || contentRect == null || viewportRect == null) return;

		scrollRect = scrollViewRect.GetComponent<ScrollRect>();
		if (scrollRect == null)
		{
			scrollRect = scrollViewRect.gameObject.AddComponent<ScrollRect>();
		}

		scrollRect.content = contentRect;
		scrollRect.viewport = viewportRect;
		scrollRect.horizontal = true;
		scrollRect.vertical = false;
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.inertia = true;
		scrollRect.scrollSensitivity = 40f;
		scrollRect.horizontalScrollbar = scrollBar;
		scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		scrollRect.verticalScrollbar = null;
	}

	private void DisableStrayScrollbars()
	{
		foreach (Scrollbar bar in GetComponentsInChildren<Scrollbar>(true))
		{
			if (bar == null || bar == scrollBar) continue;
			bar.interactable = false;
			bar.enabled = false;
		}
	}

	private void UpdateScrollMetrics()
	{
		if (contentRect == null || viewportRect == null) return;

		Canvas.ForceUpdateCanvases();
		contentLength = Mathf.Max(0f, contentRect.rect.width - viewportRect.rect.width);

		if (scrollBar != null)
		{
			float contentWidth = Mathf.Max(1f, contentRect.rect.width);
			float viewportWidth = Mathf.Max(1f, viewportRect.rect.width);
			scrollBar.size = Mathf.Clamp01(viewportWidth / contentWidth);
			scrollBar.interactable = contentLength > 0.1f;
			suppressScrollbarCallback = true;
			scrollBar.SetValueWithoutNotify(Mathf.Clamp01(scrollBar.value));
			suppressScrollbarCallback = false;
			OnScrollbarValueChanged(scrollBar.value);
		}
	}

	private void OnScrollbarValueChanged(float value)
	{
		if (suppressScrollbarCallback || contentRect == null || viewportRect == null) return;

		float overflow = Mathf.Max(0f, contentRect.rect.width - viewportRect.rect.width);
		Vector2 position = contentRect.anchoredPosition;
		position.x = -overflow * Mathf.Clamp01(value);
		contentRect.anchoredPosition = position;
	}

	private void UpdateProcessText()
	{
		if (text_processTitle != null)
		{
			text_processTitle.text = string.IsNullOrEmpty(chapterName) ? "剧情完成度" : chapterName;
		}

		if (text_processValue != null)
		{
			int total = 0;
			int unlocked = 0;
			HashSet<string> countedTags = new HashSet<string>();
			if (plotChapterElements != null)
			{
				foreach (UI_PlotChapterElement element in plotChapterElements)
				{
					if (element == null || string.IsNullOrEmpty(element.tagName)) continue;
					string normalizedTag = PlotMapProgressStore.Normalize(element.tagName);
					if (countedTags.Contains(normalizedTag)) continue;
					countedTags.Add(normalizedTag);
					++total;
					if (!element.IsLocking) ++unlocked;
				}
			}

			float process = total > 0 ? (float)unlocked / total : 1f;
			text_processValue.text = (process * 100f).ToString("0.00", CultureInfo.InvariantCulture) + "%";
		}
	}

	private Scrollbar FindOrCreateHorizontalScrollbar()
	{
		if (scrollViewRect != null)
		{
			Transform targetInScrollView = FindChildRecursive(scrollViewRect, "Scrollbar Horizontal");
			if (targetInScrollView != null)
			{
				return EnsureScrollbarComponent(targetInScrollView);
			}
		}

		Transform target = FindChildRecursive(transform, "Scrollbar Horizontal");
		if (target != null)
		{
			return EnsureScrollbarComponent(target);
		}

		if (scrollViewRect != null)
		{
			ScrollRect existingScrollRect = scrollViewRect.GetComponent<ScrollRect>();
			if (existingScrollRect != null && IsUsablePlotScrollbar(existingScrollRect.horizontalScrollbar))
			{
				return EnsureScrollbarComponent(existingScrollRect.horizontalScrollbar.transform);
			}
		}

		foreach (Scrollbar bar in GetComponentsInChildren<Scrollbar>(true))
		{
			if (!IsUsablePlotScrollbar(bar)) continue;
			return EnsureScrollbarComponent(bar.transform);
		}

		return null;
	}

	private Scrollbar EnsureScrollbarComponent(Transform target)
	{
		if (target == null) return null;

		Scrollbar targetBar = target.GetComponent<Scrollbar>();
		if (targetBar == null)
		{
			targetBar = target.gameObject.AddComponent<Scrollbar>();
		}
		targetBar.direction = Scrollbar.Direction.LeftToRight;

		if (targetBar.handleRect == null)
		{
			RectTransform handle = FindChildRecursive(target, "Handle") as RectTransform;
			if (handle == null)
			{
				handle = CreateScrollbarHandle(target);
			}
			targetBar.handleRect = handle;
		}

		return targetBar;
	}

	private bool IsUsablePlotScrollbar(Scrollbar bar)
	{
		if (bar == null) return false;
		if (bar.direction != Scrollbar.Direction.LeftToRight && bar.direction != Scrollbar.Direction.RightToLeft) return false;
		if (contentRect != null && bar.transform.IsChildOf(contentRect)) return false;
		if (scrollViewRect != null && !bar.transform.IsChildOf(scrollViewRect) && bar.transform.parent != scrollViewRect.parent) return false;
		return true;
	}

	private RectTransform CreateScrollbarHandle(Transform scrollbarTransform)
	{
		if (scrollbarTransform == null) return null;

		Transform slidingArea = FindChildRecursive(scrollbarTransform, "Sliding Area");
		if (slidingArea == null)
		{
			GameObject slidingAreaObject = new GameObject("Sliding Area", typeof(RectTransform));
			slidingAreaObject.transform.SetParent(scrollbarTransform, false);
			slidingArea = slidingAreaObject.transform;

			RectTransform slidingRect = slidingArea as RectTransform;
			if (slidingRect != null)
			{
				slidingRect.anchorMin = Vector2.zero;
				slidingRect.anchorMax = Vector2.one;
				slidingRect.offsetMin = Vector2.zero;
				slidingRect.offsetMax = Vector2.zero;
				slidingRect.localScale = Vector3.one;
			}
		}

		GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		handleObject.transform.SetParent(slidingArea, false);

		RectTransform handle = handleObject.transform as RectTransform;
		if (handle != null)
		{
			handle.anchorMin = Vector2.zero;
			handle.anchorMax = Vector2.one;
			handle.offsetMin = Vector2.zero;
			handle.offsetMax = Vector2.zero;
			handle.localScale = Vector3.one;
		}

		Image handleImage = handleObject.GetComponent<Image>();
		handleImage.color = Color.white;
		handleImage.raycastTarget = true;
		return handle;
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

	private Text FindText(params string[] names)
	{
		foreach (string name in names)
		{
			Transform target = FindChildRecursive(transform, name);
			if (target == null) continue;
			Text text = target.GetComponent<Text>() ?? target.GetComponentInChildren<Text>(true);
			if (text != null) return text;
		}
		return null;
	}

	private void BindButton(Button button, UnityEngine.Events.UnityAction action)
	{
		if (button == null || action == null) return;
		button.onClick.RemoveListener(action);
		button.onClick.AddListener(action);
		if (button.targetGraphic == null)
		{
			button.targetGraphic = button.GetComponent<Graphic>() ?? button.GetComponentInChildren<Graphic>(true);
		}
	}

	private bool MatchesChapter(UI_PlotChapterElement element, string targetName)
	{
		if (element == null || string.IsNullOrEmpty(targetName)) return false;
		string tag = PlotMapProgressStore.Normalize(element.tagName);
		string objectName = PlotMapProgressStore.Normalize(element.gameObject.name);
		return tag == targetName || objectName == targetName;
	}

	private void ScrollToElement(UI_PlotChapterElement element)
	{
		if (element == null || contentRect == null || viewportRect == null) return;

		Canvas.ForceUpdateCanvases();
		float contentWidth = Mathf.Max(1f, contentRect.rect.width);
		float viewportWidth = Mathf.Max(1f, viewportRect.rect.width);
		float overflow = Mathf.Max(0f, contentWidth - viewportWidth);
		if (overflow <= 0.1f)
		{
			SetScrollValue(0f, false);
			return;
		}

		RectTransform targetRect = element.transform as RectTransform;
		if (targetRect == null) return;

		Vector3 worldCenter = targetRect.TransformPoint(targetRect.rect.center);
		float localCenterX = contentRect.InverseTransformPoint(worldCenter).x;
		float positionFromLeft = localCenterX + contentRect.pivot.x * contentWidth;
		float value = Mathf.Clamp01((positionFromLeft - viewportWidth * 0.5f) / overflow);
		SetScrollValue(value, true);
	}

	private void SetScrollValue(float value, bool animate)
	{
		value = Mathf.Clamp01(value);
		if (_tween != null)
		{
			_tween.Kill();
			_tween = null;
		}

		if (!animate)
		{
			if (scrollBar != null) scrollBar.SetValueWithoutNotify(value);
			OnScrollbarValueChanged(value);
			return;
		}

		float current = scrollBar != null ? scrollBar.value : 0f;
		_tween = DOTween.To(() => current, next =>
		{
			current = next;
			if (scrollBar != null) scrollBar.SetValueWithoutNotify(next);
			OnScrollbarValueChanged(next);
		}, value, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
	}

	private T FindSceneObject<T>() where T : Component
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
