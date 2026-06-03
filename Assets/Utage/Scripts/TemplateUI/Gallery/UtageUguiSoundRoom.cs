// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// サウンドルーム画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSoundRoom")]
	public class UtageUguiSoundRoom : UguiView
	{
		public UtageUguiGallery Gallery
		{
			get { return this.GetComponentCacheFindIfMissing(ref gallery); }
		}

		[SerializeField] protected UtageUguiGallery gallery;

		/// リストビュー(旧仕様)
		public UguiListView listView;
		public UguiGridPage gridPage;

		/// 各サウンド再生ボタンのルートオブジェクト
		public RectTransform rootItems;
		public GameObject itemPrefab;

		/// <summary>
		/// リストビューアイテムのリスト
		/// </summary>
		protected List<AdvSoundSettingData> itemDataList = new List<AdvSoundSettingData>();

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;

		protected bool isInit = false;
		protected bool isChangedBgm = false;

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			isInit = false;
			isChangedBgm = false;
			ClearItems();
			PrepareRuntimeLayout();
			StartCoroutine(CoWaitOpen());
		}

		/// <summary>
		/// クローズしたときに呼ばれる
		/// </summary>
		protected virtual void OnClose()
		{
			isInit = false;
			ClearItems();
			if (isChangedBgm) Engine.SoundManager.StopAll(0.2f);
			isChangedBgm = false;
		}

		//起動待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			while (Engine.IsWaitBootLoading)
			{
				yield return null;
			}
			CreateItems();
			isInit = true;
		}
		
		//アイテム（ボタン）消去
		protected virtual void ClearItems()
		{
			if (gridPage != null)
			{
				gridPage.ClearItems();
			}
			else if (this.listView != null)
			{
				this.listView.ClearItems();
			}
			else if (rootItems != null)
			{
				rootItems.DestroyChildren();
			}
		}

		//アイテム（ボタン）作成
		protected virtual void CreateItems()
		{
			itemDataList = Engine.DataManager.SettingDataManager.SoundSetting.GetSoundRoomList();
			if (gridPage != null)
			{
				gridPage.Init(itemDataList.Count, CallBackCreateItem);
				gridPage.CreateItems(0);
			}
			else if (this.listView != null)
			{
				listView.CreateItems(itemDataList.Count, CallBackCreateItem);
			}
			else if (rootItems != null)
			{
				rootItems.DestroyChildren();
				for (var i = 0; i < itemDataList.Count; i++)
				{
					var go = rootItems.AddChildPrefab(itemPrefab);
					CallBackCreateItem(go, i);
				}
			}
		}


		/// <summary>
		/// リストビューのアイテムが作成されるときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作成されたアイテムのGameObject</param>
		/// <param name="index">作成されたアイテムのインデックス</param>
		protected virtual void CallBackCreateItem(GameObject go, int index)
		{
			UtageUguiSoundRoomItem item = go.GetComponent<UtageUguiSoundRoomItem>();
			if (item == null)
			{
				item = go.AddComponent<UtageUguiSoundRoomItem>();
			}
			AdvSoundSettingData data = itemDataList[index];
			item.Init(data, OnTap, index);
		}

		protected virtual void Update()
		{
			//右クリックで戻る
			if (isInit && InputUtil.IsInputGuiClose())
			{
				Gallery.Back();
			}
		}

		/// <summary>
		/// 各アイテムが押された
		/// </summary>
		/// <param name="button">押されたアイテム</param>
		protected virtual void OnTap(UtageUguiSoundRoomItem item)
		{
			AdvSoundSettingData data = item.Data;
			string path = Engine.DataManager.SettingDataManager.SoundSetting.LabelToFilePath(data.Key, SoundType.Bgm);

			StartCoroutine(CoPlaySound(path));
		}

		//サウンドをロードして鳴らす
		protected virtual IEnumerator CoPlaySound(string path)
		{
			isChangedBgm = true;
			AssetFile file = AssetFileManager.Load(path, this);
			while (!file.IsLoadEnd) yield return null;
			Engine.SoundManager.PlayBgm(file);
			file.Unuse(this);
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (gallery == null)
			{
				gallery = GetComponentInParent<UtageUguiGallery>(true);
			}
			if (listView == null)
			{
				listView = GetComponentInChildren<UguiListView>(true);
			}
			if (gridPage == null)
			{
				Transform gridPageRoot = FindChildRecursive(transform, "GridPage");
				if (gridPageRoot != null)
				{
					gridPage = gridPageRoot.GetComponent<UguiGridPage>() ?? gridPageRoot.GetComponentInChildren<UguiGridPage>(true);
				}
			}
			if (itemPrefab == null && gridPage != null && gridPage.itemPrefab != null)
			{
				itemPrefab = gridPage.itemPrefab;
			}
			if (itemPrefab == null && listView != null && listView.ItemPrefab != null)
			{
				itemPrefab = listView.ItemPrefab;
			}
			if (engine == null)
			{
				engine = GetComponentInParent<AdvEngine>(true);
			}
		}

		protected virtual void PrepareRuntimeLayout()
		{
			SetCategoryButtonsVisible(false);

			if (gridPage != null)
			{
				if (itemPrefab == null)
				{
					itemPrefab = FindRuntimePrefab("SoundRoomItem");
				}
				if (gridPage.itemPrefab == null)
				{
					gridPage.itemPrefab = itemPrefab;
				}
				if (gridPage.pageCarouselPrefab == null)
				{
					gridPage.pageCarouselPrefab = FindRuntimePrefab("CarouselButton");
				}
				gridPage.gameObject.SetActive(true);
				PrepareGridPageLayout(gridPage);
				if (listView != null)
				{
					listView.ClearItems();
					listView.gameObject.SetActive(false);
				}
				return;
			}

			if (listView == null) return;

			if (listView.ItemPrefab == null)
			{
				listView.ItemPrefab = itemPrefab != null ? itemPrefab : FindRuntimePrefab("SoundRoomItem");
			}
			else if (itemPrefab == null)
			{
				itemPrefab = listView.ItemPrefab;
			}

			Graphic graphic = listView.GetComponent<Graphic>();
			if (graphic == null)
			{
				graphic = listView.gameObject.AddComponent<Image>();
			}
			graphic.enabled = true;
			graphic.color = new Color(0f, 0f, 0f, 0f);
			graphic.raycastTarget = true;

			RectMask2D rectMask = listView.GetComponent<RectMask2D>();
			if (rectMask == null)
			{
				rectMask = listView.gameObject.AddComponent<RectMask2D>();
			}
			rectMask.enabled = true;

			Mask mask = listView.GetComponent<Mask>();
			if (mask != null)
			{
				mask.showMaskGraphic = false;
				mask.enabled = false;
			}

			ScrollRect scrollRect = listView.ScrollRect;
			if (scrollRect == null) return;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;

			RectTransform viewport = FindChildRecursive(listView.transform, "Viewport") as RectTransform;
			if (viewport != null)
			{
				scrollRect.viewport = viewport;
			}
			else
			{
				scrollRect.viewport = listView.GetComponent<RectTransform>();
			}
		}

		protected virtual void PrepareGridPageLayout(UguiGridPage targetGridPage)
		{
			if (targetGridPage == null) return;

			RectTransform pageRect = targetGridPage.transform as RectTransform;
			SetStretchRect(pageRect, new Vector2(84f, 132f), new Vector2(-160f, -164f));

			targetGridPage.grid = EnsureRuntimeGrid(targetGridPage, "__RuntimeGrid");
			if (targetGridPage.grid == null) return;

			targetGridPage.grid.gameObject.SetActive(true);
			DeactivateLegacyItemRoots(targetGridPage, targetGridPage.grid.transform);
			RectTransform gridRect = targetGridPage.grid.transform as RectTransform;
			SetTopLeftRect(gridRect, new Vector2(260f, -6f), new Vector2(1364f, 711f));
			targetGridPage.grid.cellSize = new Vector2(622f, 96f);
			targetGridPage.grid.spacing = new Vector2(120f, 27f);
			targetGridPage.grid.childAlignment = TextAnchor.UpperLeft;
			targetGridPage.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			targetGridPage.grid.constraintCount = 2;
			targetGridPage.maxItemPerPageOverride = 12;
			targetGridPage.InvalidateLayoutCache();

			ConfigurePageCarousel(targetGridPage, new Vector2(352f, 0f), new Vector2(240f, 54f));
			LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
			LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
		}

		protected virtual GridLayoutGroup EnsureRuntimeGrid(UguiGridPage targetGridPage, string runtimeGridName)
		{
			if (targetGridPage == null) return null;

			Transform gridRoot = FindChildRecursive(targetGridPage.transform, runtimeGridName);
			if (gridRoot == null)
			{
				GameObject gridObject = new GameObject(runtimeGridName, typeof(RectTransform));
				gridObject.transform.SetParent(targetGridPage.transform, false);
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
			foreach (GridLayoutGroup otherGrid in targetGridPage.GetComponentsInChildren<GridLayoutGroup>(true))
			{
				if (otherGrid == null || otherGrid == runtimeGrid) continue;
				otherGrid.transform.DestroyChildren();
				otherGrid.gameObject.SetActive(false);
			}

			runtimeGrid.transform.DestroyChildren();
			runtimeGrid.gameObject.SetActive(true);
			return runtimeGrid;
		}

		protected virtual void DeactivateLegacyItemRoots(UguiGridPage targetGridPage, Transform runtimeGridRoot)
		{
			if (targetGridPage == null || runtimeGridRoot == null) return;

			foreach (Transform child in targetGridPage.transform)
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
			return child.GetComponentInChildren<UtageUguiSoundRoomItem>(true) != null;
		}

		protected virtual void ConfigurePageCarousel(UguiGridPage targetGridPage, Vector2 anchoredPosition, Vector2 size)
		{
			if (targetGridPage == null) return;

			Transform carouselRoot = null;
			if (targetGridPage.pageCarouselToggles != null)
			{
				carouselRoot = targetGridPage.pageCarouselToggles.transform;
			}
			else
			{
				carouselRoot = FindChildRecursive(targetGridPage.transform, "PageCarousels");
				if (carouselRoot != null)
				{
					targetGridPage.pageCarouselToggles = carouselRoot.GetComponent<UguiToggleGroupIndexed>() ?? carouselRoot.GetComponentInChildren<UguiToggleGroupIndexed>(true);
				}
			}

			if (targetGridPage.pageCarouselAlignGroup == null)
			{
				targetGridPage.pageCarouselAlignGroup = targetGridPage.GetComponentInChildren<UguiAlignGroup>(true);
			}
			if (carouselRoot == null && targetGridPage.pageCarouselAlignGroup != null)
			{
				carouselRoot = targetGridPage.pageCarouselAlignGroup.transform;
			}
			if (carouselRoot == null) return;

			targetGridPage.hidePageNavigationButtons = true;
			targetGridPage.pageCarouselTextOnly = true;
			carouselRoot.gameObject.SetActive(true);
			SetBottomLeftRect(carouselRoot as RectTransform, anchoredPosition, size);

			UguiHorizontalAlignGroup horizontal = targetGridPage.pageCarouselAlignGroup as UguiHorizontalAlignGroup;
			if (horizontal != null)
			{
				horizontal.direction = UguiHorizontalAlignGroup.AlignDirection.LeftToRight;
				horizontal.paddingLeft = 0f;
				horizontal.paddingRight = 0f;
				horizontal.space = 6f;
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
}
