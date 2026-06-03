using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtageExtensions;

namespace Utage
{
	[AddComponentMenu("Utage/Lib/UI/UguiLayeredGridPage")]
	public class UguiLayeredGridPage : MonoBehaviour
	{
		const string RuntimePageButtonsName = "__RuntimePageButtons";

		public RectTransform gridUp;
		public RectTransform gridDown;
		public UguiToggleGroupIndexed pageCarouselToggles;
		public UguiAlignGroup pageCarouselAlignGroup;
		public GameObject itemPrefab;
		public GameObject pageCarouselPrefab;
		public int maxItemPerPage = 8;
		public int topRowCount = 4;
		public bool hidePageNavigationButtons = true;
		public bool pageCarouselTextOnly = true;

		public int CurrentPage { get { return currentPage; } }
		public int MaxPage
		{
			get
			{
				int perPage = Mathf.Max(1, maxItemPerPage);
				return maxItemNum <= 0 ? 0 : (maxItemNum - 1) / perPage;
			}
		}

		readonly List<GameObject> items = new List<GameObject>();
		int maxItemNum;
		int currentPage;
		System.Action<GameObject, int> callbackCreateItem;
		UguiToggleGroupIndexed boundPageCarouselToggles;
		bool isSettingCarouselIndex;

		void Awake()
		{
			EnsureRuntimeReferences();
		}

		public void PrepareRuntimeLayout()
		{
			EnsureRuntimeReferences();

			SetStretchRect(transform as RectTransform, new Vector2(25f, 216f), new Vector2(0f, -322f));
			SetTopLeftRect(gridUp, Vector2.zero, new Vector2(1739.6f, 263.34f));
			SetBottomRightRect(gridDown, Vector2.zero, new Vector2(1739.6f, 263.34f));

			RectTransform carouselRoot = pageCarouselToggles != null ? pageCarouselToggles.transform as RectTransform : null;
			SetBottomLeftRect(carouselRoot, new Vector2(0f, -134f), new Vector2(834.23f, 102.44f));
			DeactivateLegacyCarouselItems(carouselRoot);
			NormalizePageCarouselAlignGroup();

			if (itemPrefab != null && transform.IsChildOrSame(itemPrefab.transform))
			{
				itemPrefab.SetActive(false);
			}
		}

		public void Init(int maxItemNum, System.Action<GameObject, int> callbackCreateItem)
		{
			EnsureRuntimeReferences();
			this.maxItemNum = maxItemNum;
			this.callbackCreateItem = callbackCreateItem;

			if (pageCarouselToggles == null) return;

			pageCarouselToggles.ClearToggles();
			if (pageCarouselAlignGroup != null)
			{
				pageCarouselAlignGroup.DestroyAllChildren();
			}

			if (MaxPage > 0 && pageCarouselAlignGroup != null && pageCarouselPrefab != null)
			{
				List<GameObject> children = pageCarouselAlignGroup.AddChildrenFromPrefab(MaxPage + 1, pageCarouselPrefab, null);
				NormalizeCarouselButtons(children);
				pageCarouselToggles.AddToggles(GetToggles(children));
				SetCurrentCarouselIndex(0);
				RefreshPageCarouselLayout();
				SetActivePageNavigationButtons(true);
			}
			else
			{
				SetActivePageNavigationButtons(false);
			}
		}

		public void CreateItems(int page)
		{
			EnsureRuntimeReferences();
			if (itemPrefab == null || gridUp == null || gridDown == null)
			{
				Debug.LogError("[UguiLayeredGridPage] Item prefab or layered rows are missing.", this);
				return;
			}

			page = Mathf.Clamp(page, 0, MaxPage);
			currentPage = page;
			SetCurrentCarouselIndex(page);
			ClearItems();

			int pageTopIndex = Mathf.Max(1, maxItemPerPage) * CurrentPage;
			for (int i = 0; i < maxItemPerPage; ++i)
			{
				int index = pageTopIndex + i;
				if (index >= maxItemNum) break;

				RectTransform row = i < topRowCount ? gridUp : gridDown;
				int column = i < topRowCount ? i : i - topRowCount;
				int columnCount = i < topRowCount ? topRowCount : Mathf.Max(1, maxItemPerPage - topRowCount);

				GameObject go = row.AddChildPrefab(itemPrefab);
				go.SetActive(true);
				PositionItem(go.transform as RectTransform, row, column, columnCount);
				items.Add(go);
				if (callbackCreateItem != null) callbackCreateItem(go, index);
				PositionItem(go.transform as RectTransform, row, column, columnCount);
			}

			RefreshPageCarouselLayout();
		}

		public void ClearItems()
		{
			EnsureRuntimeReferences();
			items.Clear();
			ClearRow(gridUp);
			ClearRow(gridDown);
		}

		public void OnClickNextPage()
		{
			if (CurrentPage < MaxPage) CreateItems(CurrentPage + 1);
		}

		public void OnClickPrevPage()
		{
			if (CurrentPage > 0) CreateItems(CurrentPage - 1);
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (gridUp == null) gridUp = FindChildRecursive(transform, "GridUp") as RectTransform;
			if (gridDown == null) gridDown = FindChildRecursive(transform, "GridDown") as RectTransform;

			if (pageCarouselToggles == null)
			{
				Transform carouselRoot = FindChildRecursive(transform, "PageCarousels");
				if (carouselRoot != null)
				{
					pageCarouselToggles = carouselRoot.GetComponent<UguiToggleGroupIndexed>() ?? carouselRoot.GetComponentInChildren<UguiToggleGroupIndexed>(true);
				}
			}

			if (pageCarouselAlignGroup == null)
			{
				pageCarouselAlignGroup = EnsureRuntimePageAlignGroup();
			}

			if (itemPrefab == null)
			{
				itemPrefab = FindRuntimePrefab("CgGalleryItem") ?? FindRowTemplate();
			}

			EnsurePageCarouselListener();
		}

		protected virtual UguiAlignGroup EnsureRuntimePageAlignGroup()
		{
			Transform carouselRoot = pageCarouselToggles != null ? pageCarouselToggles.transform : FindChildRecursive(transform, "PageCarousels");
			if (carouselRoot == null) return null;

			Transform runtimeRoot = FindChildRecursive(carouselRoot, RuntimePageButtonsName);
			if (runtimeRoot == null)
			{
				GameObject runtimeObject = new GameObject(RuntimePageButtonsName, typeof(RectTransform));
				runtimeObject.transform.SetParent(carouselRoot, false);
				runtimeRoot = runtimeObject.transform;
			}

			UguiAlignGroup alignGroup = runtimeRoot.GetComponent<UguiAlignGroup>();
			if (alignGroup == null)
			{
				alignGroup = runtimeRoot.gameObject.AddComponent<UguiHorizontalAlignGroup>();
			}
			return alignGroup;
		}

		protected virtual void EnsurePageCarouselListener()
		{
			if (boundPageCarouselToggles == pageCarouselToggles) return;

			if (boundPageCarouselToggles != null)
			{
				boundPageCarouselToggles.OnValueChanged.RemoveListener(OnPageCarouselChanged);
			}
			boundPageCarouselToggles = pageCarouselToggles;
			if (boundPageCarouselToggles != null)
			{
				boundPageCarouselToggles.OnValueChanged.RemoveListener(OnPageCarouselChanged);
				boundPageCarouselToggles.OnValueChanged.AddListener(OnPageCarouselChanged);
			}
		}

		protected virtual void OnPageCarouselChanged(int page)
		{
			if (isSettingCarouselIndex) return;
			CreateItems(page);
		}

		protected virtual void SetCurrentCarouselIndex(int page)
		{
			if (pageCarouselToggles == null) return;

			isSettingCarouselIndex = true;
			pageCarouselToggles.CurrentIndex = page;
			isSettingCarouselIndex = false;
		}

		protected virtual IEnumerable<Toggle> GetToggles(List<GameObject> children)
		{
			if (children == null) yield break;

			foreach (GameObject child in children)
			{
				Toggle toggle = EnsureCarouselToggle(child);
				if (toggle != null) yield return toggle;
			}
		}

		protected virtual void NormalizePageCarouselAlignGroup()
		{
			if (pageCarouselAlignGroup == null) return;

			RectTransform rect = pageCarouselAlignGroup.transform as RectTransform;
			if (rect != null)
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.offsetMin = Vector2.zero;
				rect.offsetMax = Vector2.zero;
				rect.localScale = Vector3.one;
			}

			UguiHorizontalAlignGroup horizontal = pageCarouselAlignGroup as UguiHorizontalAlignGroup;
			if (horizontal != null)
			{
				horizontal.direction = UguiHorizontalAlignGroup.AlignDirection.LeftToRight;
				horizontal.paddingLeft = 0f;
				horizontal.paddingRight = 0f;
				horizontal.space = 12f;
				horizontal.isAutoResize = false;
			}
		}

		protected virtual void DeactivateLegacyCarouselItems(Transform carouselRoot)
		{
			if (carouselRoot == null) return;

			Transform runtimeRoot = pageCarouselAlignGroup != null ? pageCarouselAlignGroup.transform : null;
			foreach (Transform child in carouselRoot)
			{
				if (child == null || child == runtimeRoot) continue;
				if (IsPageNavigationButton(child.name)) continue;
				child.gameObject.SetActive(false);
			}
		}

		protected virtual bool IsPageNavigationButton(string objectName)
		{
			switch (objectName)
			{
				case "ShiftLeft":
				case "Prev":
				case "Previous":
				case "ShiftRight":
				case "Next":
				case "JumpLeftEdgeButton":
				case "JumptLeftEdgeButton":
				case "JumpRightEdgeButton":
				case "JumptRightEdgeButton":
					return true;
				default:
					return false;
			}
		}

		protected virtual void NormalizeCarouselButtons(List<GameObject> children)
		{
			if (children == null) return;

			for (int i = 0; i < children.Count; ++i)
			{
				NormalizeCarouselButton(children[i], i);
			}
		}

		protected virtual void NormalizeCarouselButton(GameObject go, int index)
		{
			if (go == null) return;

			SetCarouselPageText(go, (index + 1).ToString());
			if (!pageCarouselTextOnly) return;

			RectTransform rectTransform = go.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 84f);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 67f);
				rectTransform.localScale = Vector3.one;
			}

			foreach (Image image in go.GetComponentsInChildren<Image>(true))
			{
				if (image == null) continue;
				if (image.transform.name == "ToggleProxy")
				{
					image.color = new Color(1f, 1f, 1f, 0f);
					image.raycastTarget = true;
					continue;
				}

				image.enabled = image.sprite != null;
				image.color = Color.white;
				image.raycastTarget = false;
			}
			EnsureCarouselRaycastTarget(go, EnsureCarouselToggle(go));
			NormalizePageNumberText(go);
		}

		protected virtual Toggle EnsureCarouselToggle(GameObject go)
		{
			if (go == null) return null;

			Toggle toggle = go.GetComponent<Toggle>();
			if (toggle == null)
			{
				Selectable selectable = go.GetComponent<Selectable>();
				if (selectable != null && !(selectable is Toggle))
				{
					Transform proxy = FindChildRecursive(go.transform, "ToggleProxy");
					if (proxy == null)
					{
						GameObject proxyObject = new GameObject("ToggleProxy", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
						proxyObject.transform.SetParent(go.transform, false);
						proxyObject.transform.SetAsLastSibling();

						RectTransform proxyRect = proxyObject.GetComponent<RectTransform>();
						proxyRect.anchorMin = Vector2.zero;
						proxyRect.anchorMax = Vector2.one;
						proxyRect.offsetMin = Vector2.zero;
						proxyRect.offsetMax = Vector2.zero;

						Image proxyImage = proxyObject.GetComponent<Image>();
						proxyImage.color = new Color(1f, 1f, 1f, 0f);
						proxyImage.raycastTarget = true;
					}
					proxy = FindChildRecursive(go.transform, "ToggleProxy");
					toggle = proxy != null ? proxy.GetComponent<Toggle>() : null;
				}
				else
				{
					toggle = go.AddComponent<Toggle>();
				}
			}

			if (toggle.targetGraphic == null)
			{
				toggle.targetGraphic = toggle.GetComponent<Graphic>() ?? go.GetComponent<Graphic>() ?? go.GetComponentInChildren<Graphic>(true);
			}
			if (toggle.graphic == null)
			{
				Transform checkmark = FindChildRecursive(go.transform, "Checkmark");
				if (checkmark != null)
				{
					toggle.graphic = checkmark.GetComponent<Graphic>();
				}
			}
			EnsureCarouselRaycastTarget(go, toggle);
			return toggle;
		}

		protected virtual Graphic EnsureCarouselRaycastTarget(GameObject go, Toggle toggle)
		{
			if (go == null) return null;

			Graphic graphic = toggle != null ? toggle.targetGraphic : null;
			if (graphic != null && graphic.raycastTarget)
			{
				return graphic;
			}

			Transform proxy = FindChildRecursive(go.transform, "ToggleProxy");
			if (proxy == null)
			{
				GameObject proxyObject = new GameObject("ToggleProxy", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				proxyObject.transform.SetParent(go.transform, false);
				proxyObject.transform.SetAsFirstSibling();

				RectTransform proxyRect = proxyObject.GetComponent<RectTransform>();
				proxyRect.anchorMin = Vector2.zero;
				proxyRect.anchorMax = Vector2.one;
				proxyRect.offsetMin = Vector2.zero;
				proxyRect.offsetMax = Vector2.zero;
				proxyRect.localScale = Vector3.one;

				proxy = proxyObject.transform;
			}

			Image proxyImage = proxy.GetComponent<Image>();
			if (proxyImage == null)
			{
				proxyImage = proxy.gameObject.AddComponent<Image>();
			}

			proxyImage.enabled = true;
			proxyImage.color = new Color(1f, 1f, 1f, 0f);
			proxyImage.raycastTarget = true;
			if (toggle != null && toggle.targetGraphic == null)
			{
				toggle.targetGraphic = proxyImage;
			}
			return proxyImage;
		}

		protected virtual void NormalizePageNumberText(GameObject go)
		{
			if (go == null) return;

			foreach (Text text in go.GetComponentsInChildren<Text>(true))
			{
				if (text == null) continue;
				text.gameObject.SetActive(true);
				if (text.font == null) text.font = ResolveDefaultFont(24);
				text.color = Color.white;
				text.fontSize = Mathf.Max(text.fontSize, 24);
				text.alignment = TextAnchor.MiddleCenter;
				text.raycastTarget = false;
				StretchTextRect(text.transform as RectTransform);
				text.transform.SetAsLastSibling();
			}
			foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
			{
				if (text == null) continue;
				text.gameObject.SetActive(true);
				text.color = Color.white;
				text.fontSize = Mathf.Max(text.fontSize, 24f);
				text.alignment = TextAlignmentOptions.Center;
				text.raycastTarget = false;
				StretchTextRect(text.transform as RectTransform);
				text.transform.SetAsLastSibling();
			}
		}

		protected virtual void SetCarouselPageText(GameObject go, string textValue)
		{
			bool foundPageNumber = false;
			Transform pageNumber = FindChildRecursive(go.transform, "PageNumber");
			if (pageNumber != null)
			{
				foundPageNumber = SetTextComponents(pageNumber.gameObject, textValue);
			}
			if (foundPageNumber) return;

			SetTextComponents(go, textValue);
		}

		protected virtual bool SetTextComponents(GameObject go, string textValue)
		{
			if (go == null) return false;

			bool changed = false;
			foreach (Text text in go.GetComponentsInChildren<Text>(true))
			{
				text.text = textValue;
				changed = true;
			}
			foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
			{
				text.text = textValue;
				changed = true;
			}
			return changed;
		}

		protected virtual void SetActivePageNavigationButtons(bool isActive)
		{
			bool shouldShow = isActive && !hidePageNavigationButtons;
			if (pageCarouselToggles != null)
			{
				pageCarouselToggles.SetActiveLRButtons(shouldShow);
			}

			Transform root = pageCarouselToggles != null ? pageCarouselToggles.transform : transform;
			SetNamedPageNavigationButtonActive(root, shouldShow, "ShiftLeft");
			SetNamedPageNavigationButtonActive(root, shouldShow, "Prev");
			SetNamedPageNavigationButtonActive(root, shouldShow, "Previous");
			SetNamedPageNavigationButtonActive(root, shouldShow, "ShiftRight");
			SetNamedPageNavigationButtonActive(root, shouldShow, "Next");
			SetNamedPageNavigationButtonActive(root, shouldShow, "JumpLeftEdgeButton");
			SetNamedPageNavigationButtonActive(root, shouldShow, "JumptLeftEdgeButton");
			SetNamedPageNavigationButtonActive(root, shouldShow, "JumpRightEdgeButton");
			SetNamedPageNavigationButtonActive(root, shouldShow, "JumptRightEdgeButton");
		}

		protected virtual void SetNamedPageNavigationButtonActive(Transform root, bool isActive, string buttonName)
		{
			Transform button = FindChildRecursive(root, buttonName);
			if (button != null)
			{
				button.gameObject.SetActive(isActive);
			}
		}

		protected virtual void RefreshPageCarouselLayout()
		{
			if (pageCarouselAlignGroup == null) return;

			pageCarouselAlignGroup.Reposition();
			RectTransform rectTransform = pageCarouselAlignGroup.transform as RectTransform;
			if (rectTransform != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			}
		}

		protected virtual void ClearRow(RectTransform row)
		{
			if (row == null) return;

			List<Transform> children = new List<Transform>();
			foreach (Transform child in row)
			{
				children.Add(child);
			}

			foreach (Transform child in children)
			{
				if (child == null) continue;
				if (itemPrefab != null && child.gameObject == itemPrefab)
				{
					child.gameObject.SetActive(false);
				}
				else
				{
					Object.Destroy(child.gameObject);
				}
			}
		}

		protected virtual GameObject FindRuntimePrefab(string prefabName)
		{
			GameObject fallback = null;
			foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (candidate == null || candidate.name != prefabName) continue;
				if (!candidate.scene.IsValid()) return candidate;
				if (fallback == null) fallback = candidate;
			}
			return fallback;
		}

		protected virtual GameObject FindRowTemplate()
		{
			GameObject template = FindRowTemplate(gridUp);
			return template != null ? template : FindRowTemplate(gridDown);
		}

		protected virtual GameObject FindRowTemplate(RectTransform row)
		{
			if (row == null) return null;
			UtageUguiCgGalleryItem item = row.GetComponentInChildren<UtageUguiCgGalleryItem>(true);
			return item != null ? item.gameObject : null;
		}

		protected virtual void PositionItem(RectTransform item, RectTransform row, int column, int columnCount)
		{
			if (item == null || row == null) return;

			Vector2 itemSize = item.sizeDelta;
			if (itemSize.x <= 0f || itemSize.y <= 0f)
			{
				Rect rect = item.rect;
				itemSize = new Vector2(rect.width > 0f ? rect.width : 401f, rect.height > 0f ? rect.height : 226f);
			}

			float rowWidth = row.rect.width > 0f ? row.rect.width : row.sizeDelta.x;
			if (rowWidth <= 0f) rowWidth = itemSize.x * columnCount;
			float spacing = columnCount > 1 ? (rowWidth - itemSize.x * columnCount) / (columnCount - 1) : 0f;
			spacing = Mathf.Max(0f, spacing);

			item.anchorMin = new Vector2(0f, 1f);
			item.anchorMax = new Vector2(0f, 1f);
			item.pivot = new Vector2(0f, 1f);
			item.anchoredPosition = new Vector2(column * (itemSize.x + spacing), 0f);
			item.sizeDelta = itemSize;
			item.localScale = Vector3.one;
		}

		protected static void StretchTextRect(RectTransform rect)
		{
			if (rect == null) return;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;
		}

		protected virtual Font ResolveDefaultFont(int size)
		{
			return Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, size);
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

		protected static void SetBottomRightRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
		{
			if (rect == null) return;
			rect.anchorMin = new Vector2(1f, 0f);
			rect.anchorMax = new Vector2(1f, 0f);
			rect.pivot = new Vector2(1f, 0f);
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

		protected static Transform FindChildRecursive(Transform root, string name)
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

	static class TransformLayeredGridPageExtensions
	{
		public static bool IsChildOrSame(this Transform root, Transform child)
		{
			if (root == null || child == null) return false;
			Transform current = child;
			while (current != null)
			{
				if (current == root) return true;
				current = current.parent;
			}
			return false;
		}
	}
}
