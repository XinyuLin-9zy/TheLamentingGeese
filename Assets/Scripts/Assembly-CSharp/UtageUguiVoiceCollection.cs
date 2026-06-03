using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using Utage.TemplateUI.Gallery;
using UtageExtensions;

[AddComponentMenu("Utage/TemplateUI/UtageUguiVoiceCollection")]
public class UtageUguiVoiceCollection : UguiView
{
	public UtageUguiGallery Gallery
	{
		get { return this.GetComponentCacheFindIfMissing(ref gallery); }
	}

	[SerializeField] protected UtageUguiGallery gallery;
	[SerializeField] protected UguiCategoryGridPage categoryGridPage;
	public UguiListView listView;
	public RectTransform rootItems;
	public GameObject itemPrefab;

	public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
	[SerializeField] protected AdvEngine engine;

	protected List<AdvBacklog> itemDataList = new List<AdvBacklog>();
	protected bool isInit;
	protected int voiceRequestId;

	protected virtual void OnOpen()
	{
		EnsureRuntimeReferences();
		isInit = false;
		ClearItems();
		PrepareRuntimeLayout();
		StartCoroutine(CoWaitOpen());
	}

	protected virtual void OnClose()
	{
		isInit = false;
		++voiceRequestId;
		ClearItems();
		SoundManager manager = SoundManager.GetInstance();
		if (manager != null)
		{
			manager.StopVoice(0);
		}
	}

	protected virtual IEnumerator CoWaitOpen()
	{
		while (Engine != null && Engine.IsWaitBootLoading)
		{
			yield return null;
		}
		CreateItems();
		isInit = true;
	}

	protected virtual void Update()
	{
		if (isInit && InputUtil.IsInputGuiClose())
		{
			Gallery.Back();
		}
	}

	protected virtual void ClearItems()
	{
		if (categoryGridPage != null)
		{
			categoryGridPage.Clear();
			return;
		}

		if (listView != null)
		{
			listView.ClearItems();
		}
		else if (rootItems != null)
		{
			rootItems.DestroyChildren();
		}
	}

	protected virtual void CreateItems()
	{
		itemDataList = AdvVoiceCollectionData.Get(Engine).CollectionLogs
			.Where(log => log != null && !string.IsNullOrEmpty(log.MainVoiceFileName))
			.ToList();

		if (categoryGridPage != null)
		{
			categoryGridPage.Init(new[] { "语音收藏" }, OpenCurrentCategory);
			return;
		}

		if (listView != null)
		{
			listView.CreateItems(itemDataList.Count, CallBackCreateItem);
			return;
		}

		if (rootItems != null && itemPrefab != null)
		{
			rootItems.DestroyChildren();
			for (int i = 0; i < itemDataList.Count; ++i)
			{
				GameObject go = rootItems.AddChildPrefab(itemPrefab);
				CallBackCreateItem(go, i);
			}
		}
	}

	protected virtual void OpenCurrentCategory(UguiCategoryGridPage page)
	{
		if (page == null) return;
		page.OpenCurrentCategory(itemDataList.Count, CallBackCreateItem);
	}

	protected virtual void CallBackCreateItem(GameObject go, int index)
	{
		if (go == null || index < 0 || index >= itemDataList.Count) return;

		UtageUguiVoiceCollectionItem item = go.GetComponent<UtageUguiVoiceCollectionItem>();
		if (item == null)
		{
			item = go.AddComponent<UtageUguiVoiceCollectionItem>();
		}
		item.Init(itemDataList[index], OnTapPlay, OnTapRemove, index);
	}

	protected virtual void OnTapPlay(UtageUguiVoiceCollectionItem item)
	{
		if (item == null || item.Data == null) return;

		string voiceFileName = item.Data.MainVoiceFileName;
		if (string.IsNullOrEmpty(voiceFileName)) return;

		int requestId = ++voiceRequestId;
		StartCoroutine(CoPlayVoice(voiceFileName, item.Data.FindCharacerLabel(voiceFileName), requestId));
	}

	protected virtual void OnTapRemove(UtageUguiVoiceCollectionItem item)
	{
		if (item == null || item.Data == null) return;

		AdvVoiceCollectionData.Get(Engine).RemoveCollectionLogs(item.Data);
		ShowCollectHud(false);
		ClearItems();
		CreateItems();
	}

	protected virtual IEnumerator CoPlayVoice(string voiceFileName, string characterLabel, int requestId)
	{
		SoundManager manager = SoundManager.GetInstance();
		if (manager != null)
		{
			manager.StopVoice(0);
		}

		AssetFile file = AssetFileManager.Load(voiceFileName, this);
		if (file == null)
		{
			yield break;
		}

		while (!file.IsLoadEnd)
		{
			yield return null;
		}

		if (requestId != voiceRequestId)
		{
			file.Unuse(this);
			yield break;
		}

		manager = SoundManager.GetInstance();
		if (manager != null)
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

	protected virtual void ShowCollectHud(bool isCollect)
	{
		foreach (UI_DialogMsg dialogMsg in Resources.FindObjectsOfTypeAll<UI_DialogMsg>())
		{
			if (dialogMsg == null || !dialogMsg.gameObject.scene.IsValid()) continue;
			dialogMsg.SetCollectVoiceState(isCollect);
			return;
		}
	}

	protected virtual void EnsureRuntimeReferences()
	{
		if (gallery == null)
		{
			gallery = GetComponentInParent<UtageUguiGallery>(true);
		}
		if (categoryGridPage == null)
		{
			categoryGridPage = GetComponent<UguiCategoryGridPage>() ?? GetComponentInChildren<UguiCategoryGridPage>(true);
		}
		if (listView == null)
		{
			listView = GetComponentInChildren<UguiListView>(true);
		}
		if (rootItems == null)
		{
			Transform root = FindChildRecursive(transform, "Content") ?? FindChildRecursive(transform, "Grid");
			rootItems = root as RectTransform;
		}
		if (engine == null)
		{
			engine = GetComponentInParent<AdvEngine>(true);
		}

		if (itemPrefab == null && categoryGridPage != null && categoryGridPage.gridPage != null)
		{
			itemPrefab = categoryGridPage.gridPage.itemPrefab;
		}
		if (itemPrefab == null && listView != null && listView.ItemPrefab != null)
		{
			itemPrefab = listView.ItemPrefab;
		}
		if (itemPrefab == null)
		{
			itemPrefab = FindRuntimePrefab("VoiceCollectionItem");
		}
	}

	protected virtual void PrepareRuntimeLayout()
	{
		if (categoryGridPage != null && categoryGridPage.gridPage != null)
		{
			UguiGridPage gridPage = categoryGridPage.gridPage;
			gridPage.gameObject.SetActive(true);
			if (gridPage.itemPrefab == null)
			{
				gridPage.itemPrefab = itemPrefab;
			}
			else if (itemPrefab == null)
			{
				itemPrefab = gridPage.itemPrefab;
			}
			EnsureGridPageLayout(gridPage);
			SetCategoryButtonsVisible(false);
			if (listView != null)
			{
				listView.ClearItems();
				listView.gameObject.SetActive(false);
			}
			return;
		}

		if (listView != null)
		{
			if (listView.ItemPrefab == null)
			{
				listView.ItemPrefab = itemPrefab;
			}
			PrepareListViewLayout(listView);
		}
	}

	protected virtual void EnsureGridPageLayout(UguiGridPage gridPage)
	{
		if (gridPage == null) return;

		RectTransform pageRect = gridPage.transform as RectTransform;
		SetStretchRect(pageRect, new Vector2(90f, 132f), new Vector2(-830f, -164f));

		Vector2 cellSize = new Vector2(632f, 101f);
		gridPage.grid = EnsureRuntimeGrid(gridPage, "__RuntimeGrid");
		if (gridPage.grid == null) return;

		gridPage.grid.gameObject.SetActive(true);
		DeactivateLegacyItemRoots(gridPage, gridPage.grid.transform);
		RectTransform gridRect = gridPage.grid.transform as RectTransform;
		SetTopLeftRect(gridRect, Vector2.zero, new Vector2(680f, 560f));
		gridPage.grid.cellSize = cellSize;
		gridPage.grid.spacing = new Vector2(0f, 12f);
		gridPage.grid.childAlignment = TextAnchor.UpperLeft;
		gridPage.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		gridPage.grid.constraintCount = 1;
		gridPage.maxItemPerPageOverride = 5;
		gridPage.InvalidateLayoutCache();

		if (gridPage.pageCarouselPrefab == null)
		{
			gridPage.pageCarouselPrefab = FindRuntimePrefab("CarouselButton");
		}
		ConfigurePageCarousel(gridPage, new Vector2(0f, 0f), new Vector2(300f, 54f));
		LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
	}

	protected virtual GridLayoutGroup EnsureRuntimeGrid(UguiGridPage gridPage, string runtimeGridName)
	{
		if (gridPage == null) return null;

		Transform gridRoot = FindChildRecursive(gridPage.transform, runtimeGridName);
		if (gridRoot == null)
		{
			GameObject gridObject = new GameObject(runtimeGridName, typeof(RectTransform));
			gridObject.transform.SetParent(gridPage.transform, false);
			gridRoot = gridObject.transform;
		}

		RectTransform rect = gridRoot as RectTransform;
		if (rect != null)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;
		}

		GridLayoutGroup runtimeGrid = gridRoot.GetComponent<GridLayoutGroup>() ?? gridRoot.gameObject.AddComponent<GridLayoutGroup>();
		foreach (GridLayoutGroup otherGrid in gridPage.GetComponentsInChildren<GridLayoutGroup>(true))
		{
			if (otherGrid == null || otherGrid == runtimeGrid) continue;
			otherGrid.transform.DestroyChildren();
			otherGrid.gameObject.SetActive(false);
		}

		runtimeGrid.transform.DestroyChildren();
		runtimeGrid.gameObject.SetActive(true);
		return runtimeGrid;
	}

	protected virtual void DeactivateLegacyItemRoots(UguiGridPage gridPage, Transform runtimeGridRoot)
	{
		if (gridPage == null || runtimeGridRoot == null) return;

		foreach (Transform child in gridPage.transform)
		{
			if (child == null || child == runtimeGridRoot) continue;
			if (!IsLegacyItemRoot(child)) continue;
			child.gameObject.SetActive(false);
		}
	}

	protected virtual bool IsLegacyItemRoot(Transform child)
	{
		if (child == null) return false;
		if (child.name == "Grid" || child.name == "Content" || child.name == "Items") return true;
		return child.GetComponentInChildren<UtageUguiVoiceCollectionItem>(true) != null;
	}

	protected virtual void ConfigurePageCarousel(UguiGridPage gridPage, Vector2 anchoredPosition, Vector2 size)
	{
		if (gridPage == null) return;

		Transform carouselRoot = null;
		if (gridPage.pageCarouselToggles != null)
		{
			carouselRoot = gridPage.pageCarouselToggles.transform;
		}
		else
		{
			carouselRoot = FindChildRecursive(gridPage.transform, "PageCarousels");
			if (carouselRoot != null)
			{
				gridPage.pageCarouselToggles = carouselRoot.GetComponent<UguiToggleGroupIndexed>() ?? carouselRoot.GetComponentInChildren<UguiToggleGroupIndexed>(true);
			}
		}

		if (gridPage.pageCarouselAlignGroup == null)
		{
			gridPage.pageCarouselAlignGroup = gridPage.GetComponentInChildren<UguiAlignGroup>(true);
		}
		if (carouselRoot == null && gridPage.pageCarouselAlignGroup != null)
		{
			carouselRoot = gridPage.pageCarouselAlignGroup.transform;
		}
		if (carouselRoot == null) return;

		gridPage.hidePageNavigationButtons = true;
		gridPage.pageCarouselTextOnly = true;
		carouselRoot.gameObject.SetActive(true);
		SetBottomLeftRect(carouselRoot as RectTransform, anchoredPosition, size);

		UguiHorizontalAlignGroup horizontal = gridPage.pageCarouselAlignGroup as UguiHorizontalAlignGroup;
		if (horizontal != null)
		{
			horizontal.direction = UguiHorizontalAlignGroup.AlignDirection.LeftToRight;
			horizontal.paddingLeft = 0f;
			horizontal.paddingRight = 0f;
			horizontal.space = 14f;
			horizontal.isAutoResize = false;
		}
	}

	protected virtual void SetCategoryButtonsVisible(bool visible)
	{
		Transform categoryButtons = FindChildRecursive(transform, "CategoryButtons");
		if (categoryButtons != null)
		{
			categoryButtons.gameObject.SetActive(visible);
		}
	}

	protected static void SetStretchRect(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
	{
		if (rect == null) return;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = offsetMin;
		rect.offsetMax = offsetMax;
		rect.localScale = Vector3.one;
	}

	protected static void SetTopLeftRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
	{
		if (rect == null) return;
		rect.anchorMin = new Vector2(0f, 1f);
		rect.anchorMax = new Vector2(0f, 1f);
		rect.pivot = new Vector2(0f, 1f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;
		rect.localScale = Vector3.one;
	}

	protected static void SetBottomLeftRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
	{
		if (rect == null) return;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.zero;
		rect.pivot = Vector2.zero;
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;
		rect.localScale = Vector3.one;
	}

	protected virtual void PrepareListViewLayout(UguiListView targetListView)
	{
		if (targetListView == null) return;

		ScrollRect scrollRect = targetListView.ScrollRect;
		if (scrollRect != null)
		{
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
		}

		Graphic graphic = targetListView.GetComponent<Graphic>();
		if (graphic == null)
		{
			graphic = targetListView.gameObject.AddComponent<Image>();
		}
		graphic.color = new Color(0f, 0f, 0f, 0f);
		graphic.raycastTarget = true;

		RectMask2D mask = targetListView.GetComponent<RectMask2D>();
		if (mask == null)
		{
			targetListView.gameObject.AddComponent<RectMask2D>();
		}
	}

	protected virtual GameObject FindRuntimePrefab(string prefabName)
	{
		IEnumerable<GameObject> candidates = Resources.FindObjectsOfTypeAll<GameObject>()
			.Where(x => x != null && x.name == prefabName);
		return candidates.FirstOrDefault(x => !x.scene.IsValid()) ?? candidates.FirstOrDefault();
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
