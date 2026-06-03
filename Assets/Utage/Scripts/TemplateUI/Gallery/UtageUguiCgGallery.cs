// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{
	/// <summary>
	/// CGギャラリー画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiCgGallery")]
	public class UtageUguiCgGallery : UguiView
	{
		public UtageUguiGallery Gallery
		{
			get { return this.GetComponentCacheFindIfMissing(ref gallery); }
		}

		[SerializeField] UtageUguiGallery gallery;

		/// <summary>
		/// CG表示画面
		/// </summary>
		public UtageUguiCgGalleryViewer CgView;

		/// カテゴリつきのグリッドビュー(ページ切り替え機能付き)
		/// 宴3までの古いやり方
		[UnityEngine.Serialization.FormerlySerializedAs("categoryGirdPage")]
		public UguiCategoryGridPage categoryGridPage;

		/// カテゴリつきのグリッドビュー(ページ切り替え機能なし)
		/// 宴4以降の新しいやり方
		public UguiCategoryPanel categoryPanel;

		protected UguiLayeredGridPage layeredGridPage;

		/// <summary>アイテムのリスト</summary>
		List<AdvCgGalleryData> itemDataList = new List<AdvCgGalleryData>();

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] AdvEngine engine;

		//ガイドメッセージの表示。設定してないときは表示しない
		public SystemUiGuideMessage guideMessage;


		protected bool isInit = false;

		/*
			void OnEnable()
			{
				OnClose();
				OnOpen();
			}
		*/
		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			PrepareRuntimeLayout();
			ResetPageContents();
			StartCoroutine(CoWaitOpen());
		}

		/// <summary>
		/// クローズしたときに呼ばれる
		/// </summary>
		protected virtual void OnClose()
		{
			isInit = false;
		}

		protected virtual void ResetPageContents()
		{
			if (layeredGridPage != null)
			{
				layeredGridPage.ClearItems();
			}
			if (categoryGridPage != null)
			{
				categoryGridPage.Clear();
			}
			else if (categoryPanel != null)
			{
				categoryPanel.Clear();
			}
		}

		//ロード待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			isInit = false;
			while (Engine.IsWaitBootLoading)
			{
				yield return null;
			}
			
			
			if (categoryGridPage != null)
			{
				string[] categories = Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryCategoryList().ToArray();
				SetCategoryButtonsVisible(categories.Length > 1);
				categoryGridPage.Init(categories, OpenCurrentCategory);
			}
			else if (categoryPanel != null)
			{
				// 宴4以降の新しいやり方
				string[] categories = Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryCategoryList().ToArray();
				SetCategoryButtonsVisible(categories.Length > 1);
				categoryPanel.Init(categories, OpenCurrentCategory);
			}
			isInit = true;
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (categoryGridPage == null)
			{
				categoryGridPage = GetComponent<UguiCategoryGridPage>() ?? GetComponentInChildren<UguiCategoryGridPage>(true);
			}
			if (categoryGridPage != null && categoryGridPage.gridPage == null)
			{
				Transform gridPageRoot = FindChildRecursive(transform, "GridPage");
				if (gridPageRoot != null)
				{
					categoryGridPage.gridPage = gridPageRoot.GetComponent<UguiGridPage>() ?? gridPageRoot.GetComponentInChildren<UguiGridPage>(true);
				}
			}
			if (layeredGridPage == null)
			{
				Transform layeredRoot = FindChildRecursive(transform, "GridPage_Layered");
				if (layeredRoot != null)
				{
					layeredGridPage = layeredRoot.GetComponent<UguiLayeredGridPage>() ?? layeredRoot.GetComponentInChildren<UguiLayeredGridPage>(true);
				}
			}
			if (categoryPanel == null)
			{
				categoryPanel = GetComponentInChildren<UguiCategoryPanel>(true);
			}
		}

		protected virtual void PrepareRuntimeLayout()
		{
			DeactivateNamedChildren("Text_Explain");

			if (PrepareLayeredRuntimeLayout())
			{
				return;
			}

			DeactivateNamedChildren("GridPage_Layered");

			if (categoryGridPage == null || categoryGridPage.gridPage == null) return;

			UguiGridPage gridPage = categoryGridPage.gridPage;
			gridPage.gameObject.SetActive(true);
			if (gridPage.itemPrefab == null)
			{
				gridPage.itemPrefab = FindRuntimePrefab("CgGalleryItem");
			}
			if (gridPage.pageCarouselPrefab == null)
			{
				gridPage.pageCarouselPrefab = FindRuntimePrefab("CarouselButton");
			}

			RectTransform pageRect = gridPage.transform as RectTransform;
			SetStretchRect(pageRect, new Vector2(74f, 132f), new Vector2(-150f, -164f));

			if (gridPage.grid == null)
			{
				gridPage.grid = gridPage.GetComponentInChildren<GridLayoutGroup>(true);
			}
			if (gridPage.grid == null) return;

			gridPage.grid.gameObject.SetActive(true);
			RectTransform gridRect = gridPage.grid.transform as RectTransform;
			SetTopLeftRect(gridRect, Vector2.zero, new Vector2(1320f, 508f));
			gridPage.grid.cellSize = new Vector2(300f, 169f);
			gridPage.grid.spacing = new Vector2(38f, 52f);
			gridPage.grid.childAlignment = TextAnchor.UpperLeft;
			gridPage.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			gridPage.grid.constraintCount = 4;
			gridPage.maxItemPerPageOverride = 8;
			gridPage.InvalidateLayoutCache();

			ConfigurePageCarousel(gridPage, new Vector2(0f, 0f), new Vector2(760f, 54f));
			LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
			LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
		}

		protected virtual bool PrepareLayeredRuntimeLayout()
		{
			if (layeredGridPage == null) return false;

			layeredGridPage.gameObject.SetActive(true);
			if (categoryGridPage != null && categoryGridPage.gridPage != null)
			{
				categoryGridPage.gridPage.ClearItems();
				categoryGridPage.gridPage.gameObject.SetActive(false);
			}
			if (layeredGridPage.itemPrefab == null)
			{
				layeredGridPage.itemPrefab = FindRuntimePrefab("CgGalleryItem");
			}
			if (layeredGridPage.pageCarouselPrefab == null)
			{
				layeredGridPage.pageCarouselPrefab = FindRuntimePrefab("CarouselButton");
			}
			layeredGridPage.PrepareRuntimeLayout();
			LayoutRebuilder.ForceRebuildLayoutImmediate(layeredGridPage.transform as RectTransform);
			return true;
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
				horizontal.space = 12f;
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

		protected virtual void DeactivateNamedChildren(string targetName)
		{
			if (string.IsNullOrEmpty(targetName)) return;
			foreach (Transform child in GetComponentsInChildren<Transform>(true))
			{
				if (child == null || child == transform || child.name != targetName) continue;
				child.gameObject.SetActive(false);
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

		protected virtual void Update()
		{
			//右クリックで戻る
			if (isInit && InputUtil.IsInputGuiClose())
			{
				Gallery.Back();
			}
		}


		/// <summary>
		/// 現在のカテゴリのページを開く（宴3までの古いやり方）
		/// </summary>
		protected virtual void OpenCurrentCategory(UguiCategoryGridPage gridPage)
		{
			itemDataList =
				Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryList(
					Engine.SystemSaveData.GalleryData, gridPage.CurrentCategory);
			if (layeredGridPage != null && layeredGridPage.gameObject.activeSelf)
			{
				layeredGridPage.Init(itemDataList.Count, CreateItem);
				layeredGridPage.CreateItems(0);
				return;
			}
			gridPage.OpenCurrentCategory(itemDataList.Count, CreateItem);
		}

		/// <summary>
		/// リストビューのアイテムが作成されるときに呼ばれるコールバック（宴3までの古いやり方）
		/// </summary>
		/// <param name="go">作成されたアイテムのGameObject</param>
		/// <param name="index">作成されたアイテムのインデックス</param>
		protected virtual void CreateItem(GameObject go, int index)
		{
			AdvCgGalleryData data = itemDataList[index];
			UtageUguiCgGalleryItem item = go.GetComponent<UtageUguiCgGalleryItem>();
			item.Init(data, OnTap);
		}

		/// <summary>
		/// 各アイテムが押された（宴3までの古いやり方）
		/// </summary>
		/// <param name="button">押されたアイテム</param>
		protected virtual void OnTap(UtageUguiCgGalleryItem item)
		{
			CgView.Open(item.Data);
		}
		
		// 現在のカテゴリのページを開く
		// 宴4以降の新しいやり方
		protected virtual void OpenCurrentCategory(UguiCategoryPanel panel)
		{
			itemDataList =
				Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryList(
					Engine.SystemSaveData.GalleryData, panel.CurrentCategory);
			panel.OpenCurrentCategory(itemDataList.Count, 
				(GameObject go, int index)=>
				{
					AdvCgGalleryData data = itemDataList[index];
					UtageUguiCgGalleryItem item = go.GetComponent<UtageUguiCgGalleryItem>();
					item.Init(data);
				});
		}

		//UtageUguiCgGalleryItemがクリックされたときに、プログラムから呼ばれる
		//宴4以降の新しいやり方
		public virtual void OnClickedButton(UtageUguiCgGalleryItem item)
		{
			if (item.Data.IsOpened)
			{
				//正常に開く
				CgView.Open(item.Data);
			}
			else
			{
				//開けない
				//ガイドメッセージの表示
				if (guideMessage != null)
				{
					guideMessage.Open(LanguageSystemText.LocalizeText(SystemText.UtageGuideMessageCgGalleryNotOpened));
				}
			}
		}
	}
}
