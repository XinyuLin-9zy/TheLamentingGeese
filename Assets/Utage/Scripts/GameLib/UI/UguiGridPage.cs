// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtageExtensions;
using TMPro;

namespace Utage
{

	/// <summary>
	/// CGギャラリー画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/UguiGridPage")]
	public class UguiGridPage : MonoBehaviour
	{
		/// <summary>
		/// グリッドグループ
		/// </summary>
		public GridLayoutGroup grid;

		/// <summary>
		/// アイテムプレハブ
		/// </summary>
		public GameObject itemPrefab;

		/// <summary>
		/// ページ切り替えボタンのグループ
		/// </summary>
		public UguiToggleGroupIndexed pageCarouselToggles;
		public UguiAlignGroup pageCarouselAlignGroup;

		/// <summary>
		/// 
		/// </summary>
		public GameObject pageCarouselPrefab;

		public int maxItemPerPageOverride = 0;
		public bool hidePageNavigationButtons = false;
		public bool pageCarouselTextOnly = false;

		/// <summary>
		/// 1ページあたりの表示アイテム数
		/// </summary>
		public int MaxItemPerPage
		{
			get
			{
				if (maxItemPerPageOverride > 0)
				{
					return maxItemPerPageOverride;
				}
				if (maxItemPerPage < 0)
				{
					Rect rect = (grid.transform as RectTransform).rect;
					int countX = GetCellCount(grid.cellSize.x, rect.size.x, grid.spacing.x);
					int countY = GetCellCount(grid.cellSize.y, rect.size.y, grid.spacing.y);

					switch (grid.constraint)
					{
						case GridLayoutGroup.Constraint.FixedColumnCount:
						countX = Mathf.Min(countX, grid.constraintCount);
							break;
						case GridLayoutGroup.Constraint.FixedRowCount:
						countY = Mathf.Min(countY, grid.constraintCount);
							break;
						case GridLayoutGroup.Constraint.Flexible:
					default:
							break;
					}
					maxItemPerPage = Mathf.Max(1, countX * countY);
				}
				return maxItemPerPage;
			}
		}
		int maxItemPerPage = -1;

		public void InvalidateLayoutCache()
		{
			maxItemPerPage = -1;
		}

		int GetCellCount(float cellSize, float rectSize, float space)
		{
			int count = 0;
			float size = 0;
			while (true)
			{
				size += cellSize;
				if (size > rectSize)
				{
					break;
				}
				++count;
				size += space;
			}
			return count;
		}

		/// <summary>
		/// 表示アイテムの最大数
		/// </summary>
		int maxItemNum = 0;


		//現在のページ
		public int CurrentPage { get { return currentPage; } }
		int currentPage = 0;

		//最大ページ
		public int MaxPage { get { return maxItemNum <= 0 ? 0 : (maxItemNum - 1) / MaxItemPerPage; } }

		//次のページ
		public int NextPage { get { return Mathf.Min(CurrentPage + 1, MaxPage); } }
		//前のページ
		public int PrevPage { get { return Mathf.Max(CurrentPage - 1, 0); } }

		//アイテムリスト
		public List<GameObject> Items { get { return items; } }
		List<GameObject> items = new List<GameObject>();

		System.Action<GameObject, int> CallbackCreateItem;  //アイテムが作成されたときのコールバック
		bool runtimeReferencesInitialized;
		UguiToggleGroupIndexed boundPageCarouselToggles;

		void Awake()
		{
			EnsureRuntimeReferences();
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (grid == null)
			{
				grid = GetComponentInChildren<GridLayoutGroup>(true);
			}
			if (grid == null)
			{
				Transform gridTransform = FindChildRecursive(transform, "Grid") ?? FindChildRecursive(transform, "Content");
				if (gridTransform == null || HasBlockingLayoutGroup(gridTransform))
				{
					gridTransform = CreateRuntimeGridRoot();
				}
				grid = gridTransform.gameObject.AddComponent<GridLayoutGroup>();
				if (grid != null)
				{
					InitializeAddedGrid(grid);
				}
			}
			else if (grid.cellSize.x <= 0 || grid.cellSize.y <= 0)
			{
				InitializeAddedGrid(grid);
			}

			if (pageCarouselToggles == null)
			{
				pageCarouselToggles = GetComponentInChildren<UguiToggleGroupIndexed>(true);
			}
			if (pageCarouselAlignGroup == null)
			{
				pageCarouselAlignGroup = GetComponentInChildren<UguiAlignGroup>(true);
			}

			EnsurePageCarouselListener();
			if (runtimeReferencesInitialized) return;
			BindPageButton("ShiftLeft", OnClickPrevPage);
			BindPageButton("Prev", OnClickPrevPage);
			BindPageButton("Previous", OnClickPrevPage);
			BindPageButton("ShiftRight", OnClickNextPage);
			BindPageButton("Next", OnClickNextPage);
			runtimeReferencesInitialized = true;
		}

		protected virtual void EnsurePageCarouselListener()
		{
			if (boundPageCarouselToggles == pageCarouselToggles) return;

			if (boundPageCarouselToggles != null)
			{
				boundPageCarouselToggles.OnValueChanged.RemoveListener(CreateItems);
			}
			boundPageCarouselToggles = pageCarouselToggles;
			if (boundPageCarouselToggles != null)
			{
				boundPageCarouselToggles.OnValueChanged.RemoveListener(CreateItems);
				boundPageCarouselToggles.OnValueChanged.AddListener(CreateItems);
			}
		}

		protected virtual void InitializeAddedGrid(GridLayoutGroup targetGrid)
		{
			if (targetGrid == null) return;

			bool isSaveLoadGrid = IsSaveLoadItemPrefab();
			Vector2 cellSize = isSaveLoadGrid ? new Vector2(311, 713) : new Vector2(544, 172);
			if (itemPrefab != null && itemPrefab.TryGetComponent(out RectTransform itemRect))
			{
				Vector2 prefabSize = itemRect.sizeDelta;
				if (prefabSize.x > 0 && prefabSize.y > 0)
				{
					cellSize = prefabSize;
				}
			}

			targetGrid.cellSize = cellSize;
			if (targetGrid.spacing == Vector2.zero)
			{
				targetGrid.spacing = isSaveLoadGrid ? new Vector2(22, 0) : new Vector2(12, 12);
			}
			targetGrid.childAlignment = isSaveLoadGrid ? TextAnchor.MiddleCenter : targetGrid.childAlignment;
			targetGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
			targetGrid.constraintCount = 1;
		}

		protected virtual bool IsSaveLoadItemPrefab()
		{
			return itemPrefab != null
				&& (itemPrefab.GetComponent<UtageUguiSaveLoadItem>() != null
					|| itemPrefab.name.IndexOf("SaveLoad", System.StringComparison.OrdinalIgnoreCase) >= 0);
		}

		protected virtual bool HasBlockingLayoutGroup(Transform target)
		{
			if (target == null) return false;
			LayoutGroup layoutGroup = target.GetComponent<LayoutGroup>();
			return layoutGroup != null && !(layoutGroup is GridLayoutGroup);
		}

		protected virtual Transform CreateRuntimeGridRoot()
		{
			Transform existing = FindChildRecursive(transform, "__RuntimeGrid");
			if (existing != null) return existing;

			GameObject gridObject = new GameObject("__RuntimeGrid", typeof(RectTransform));
			gridObject.transform.SetParent(transform, false);

			RectTransform rect = gridObject.transform as RectTransform;
			if (rect != null)
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.offsetMin = Vector2.zero;
				rect.offsetMax = Vector2.zero;
				rect.localScale = Vector3.one;
			}

			return gridObject.transform;
		}

		protected virtual void BindPageButton(string buttonName, UnityAction action)
		{
			if (action == null) return;
			Transform root = pageCarouselToggles != null ? pageCarouselToggles.transform : transform;
			Transform target = FindChildRecursive(root, buttonName);
			if (target == null) return;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
				button.targetGraphic = target.GetComponent<Graphic>();
			}
			else if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>();
			}

			button.onClick.RemoveListener(action);
			button.onClick.AddListener(action);

			if (pageCarouselToggles != null)
			{
				if (buttonName == "ShiftLeft" && pageCarouselToggles.shiftLeftButton == null)
				{
					pageCarouselToggles.shiftLeftButton = button;
				}
				if (buttonName == "ShiftRight" && pageCarouselToggles.shiftRightButton == null)
				{
					pageCarouselToggles.shiftRightButton = button;
				}
			}
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

		//
		public void Init(int maxItemNum, System.Action<GameObject, int> callbackCreateItem)
		{
			EnsureRuntimeReferences();
			InvalidateLayoutCache();
			this.maxItemNum = maxItemNum;
			this.CallbackCreateItem = callbackCreateItem;
			if (pageCarouselToggles)
			{
				NormalizeTextOnlyCarouselRoot();
				pageCarouselToggles.ClearToggles();
				if (pageCarouselAlignGroup != null) pageCarouselAlignGroup.DestroyAllChildren();
				if (MaxPage > 0)
				{
					if (pageCarouselAlignGroup != null && pageCarouselPrefab != null)
					{
						List<GameObject> children =
							pageCarouselAlignGroup.AddChildrenFromPrefab( MaxPage + 1, pageCarouselPrefab, null );
						NormalizeCarouselButtons(children);
						pageCarouselToggles.AddToggles(children.Select(EnsureCarouselToggle));
					}
					pageCarouselToggles.CurrentIndex = 0;
					RefreshPageCarouselLayout();
					SetActivePageNavigationButtons(true);
				}
				else
				{
					SetActivePageNavigationButtons(false);
				}
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

		protected virtual void NormalizeTextOnlyCarouselRoot()
		{
			if (!pageCarouselTextOnly || pageCarouselAlignGroup == null) return;

			RectTransform rectTransform = pageCarouselAlignGroup.transform as RectTransform;
			if (rectTransform == null) return;

			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
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
			NormalizePageNumberText(go);
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
				text.fontSize = Mathf.Max(text.fontSize, 24);
				text.alignment = TextAlignmentOptions.Center;
				text.raycastTarget = false;
				StretchTextRect(text.transform as RectTransform);
				text.transform.SetAsLastSibling();
			}
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

		protected virtual void SetCarouselPageText(GameObject go, string textValue)
		{
			if (go == null) return;

			bool foundPageNumber = false;
			Transform pageNumber = FindChildRecursive(go.transform, "PageNumber");
			if (pageNumber != null)
			{
				foundPageNumber = SetTextComponents(pageNumber.gameObject, textValue);
			}

			if (foundPageNumber) return;

			foreach (Text text in go.GetComponentsInChildren<Text>(true))
			{
				text.text = textValue;
			}
			foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
			{
				text.text = textValue;
			}
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

			return toggle;
		}

		//指定のページのアイテムを作成
		public void CreateItems(int page)
		{
			EnsureRuntimeReferences();
			if (grid == null || itemPrefab == null)
			{
				Debug.LogError("[UguiGridPage] Grid or ItemPrefab is missing.", this);
				return;
			}

			page = Mathf.Clamp(page, 0, MaxPage);
			this.currentPage = page;
			if (this.pageCarouselToggles != null) this.pageCarouselToggles.CurrentIndex = page;
			///いったん削除
			ClearItems();

			int pageTopIndex = MaxItemPerPage * CurrentPage;
			for (int i = 0; i < MaxItemPerPage; ++i)
			{
				int index = pageTopIndex + i;
				if (index >= maxItemNum) break;

				GameObject go = grid.transform.AddChildPrefab(itemPrefab);
				items.Add(go);
				if (CallbackCreateItem != null) CallbackCreateItem(go, index);
			}
			RefreshPageCarouselLayout();
		}

		/// <summary>
		/// アイテムをクリア
		/// </summary>
		public void ClearItems()
		{
			EnsureRuntimeReferences();
			if (grid == null) return;
			items.Clear();
			///閉じる
			grid.transform.DestroyChildren();
		}

		/// <summary>
		/// 次ページボタンが押された
		/// </summary>
		public void OnClickNextPage()
		{
			int nextPage = NextPage;
			if (nextPage != CurrentPage)
			{
				CreateItems(nextPage);
			}
		}

		/// <summary>
		/// 前ページボタンが押された
		/// </summary>
		public void OnClickPrevPage()
		{
			int prevPage = PrevPage;
			if (prevPage != CurrentPage)
			{
				CreateItems(prevPage);
			}
		}
	}
}
