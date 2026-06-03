// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtageExtensions;

namespace Utage
{
	[AddComponentMenu("Utage/ADV/AdvUguiSelectionManager")]
	public class AdvUguiSelectionManager : MonoBehaviour, IAdvEngineGetter
	{
		public AdvEngine Engine
		{
			get
			{
				if (engine != null) return engine;
				engine = GetComponentInParent<AdvEngine>(true);
				if (engine != null) return engine;
				engine = gameObject.scene.GetComponentInScene<AdvEngine>(true);
				if (engine != null) return engine;
				engine = WrapperFindObject.FindObjectOfTypeIncludeInactive<AdvEngine>();
				return engine;
			}
			protected set => engine = value;
		}
		[SerializeField] protected AdvEngine engine;
		public AdvEngine AdvEngineGetter => Engine;

		protected enum SelectedColorMode
		{
			None,
			Change,
		}
		[SerializeField] protected SelectedColorMode selectedColorMode = SelectedColorMode.None;
		[SerializeField] protected Color selectedColor = new Color(0.8f, 0.8f, 0.8f);

		public List<GameObject> PrefabList { get { return prefabList; } }
		[SerializeField] protected List<GameObject> prefabList;

		protected AdvSelectionManager SelectionManager { get { return Engine != null ? Engine.SelectionManager : null; } }

		public UguiListView ListView => this.GetComponentCache(ref listView);
		UguiListView listView;

		CanvasGroup CanvasGroup => this.gameObject.GetComponentCacheCreateIfMissing<CanvasGroup>(ref canvasGroup);
		CanvasGroup canvasGroup;

		public List<GameObject> Items { get { return items; } }
		List<GameObject> items = new List<GameObject>();

		private bool IsInitialized { get; set; }

		public virtual void Open()
		{
			gameObject.SetActive(true);
			PrepareRuntimeLayout();
			InitSub();
		}

		public virtual void Close()
		{
			gameObject.SetActive(false);
		}

		protected virtual void Awake()
		{
			if (prefabList == null) prefabList = new List<GameObject>();
			InitSub();
		}

		protected virtual void OnEnable()
		{
			InitSub();
		}

		public void InitEngine(AdvEngine advEngine)
		{
			Engine = advEngine;
			InitSub();
		}

		public void ReleaseEngine()
		{
			ReleaseEvents();
			Engine = null;
		}

		protected virtual void InitSub()
		{
			if (IsInitialized) return;
			if (Engine == null || SelectionManager == null) return;
			if (prefabList == null) prefabList = new List<GameObject>();

			SelectionManager.OnClear.AddListener(OnClear);
			SelectionManager.OnBeginShow.AddListener(OnBeginShow);
			SelectionManager.OnBeginWaitInput.AddListener(OnBeginWaitInput);
			ClearAll();
			IsInitialized = true;
		}

		protected virtual void ReleaseEvents()
		{
			if (!IsInitialized) return;
			if (Engine != null && SelectionManager != null)
			{
				SelectionManager.OnClear.RemoveListener(OnClear);
				SelectionManager.OnBeginShow.RemoveListener(OnBeginShow);
				SelectionManager.OnBeginWaitInput.RemoveListener(OnBeginWaitInput);
			}
			IsInitialized = false;
		}

		protected virtual void ClearAll()
		{
			if (ListView != null)
			{
				ListView.ClearItems();
			}
			else
			{
				foreach (var item in Items)
				{
					if (item != null)
					{
						GameObject.Destroy(item);
					}
				}
			}
			Items.Clear();
		}

		protected virtual void CreateItems()
		{
			if (SelectionManager == null) return;

			PrepareRuntimeLayout();
			ClearAll();
			List<GameObject> listViewItems = new List<GameObject>();
			foreach (var data in SelectionManager.Selections)
			{
				GameObject prefab = GetPrefab(data);
				if (prefab == null)
				{
					Debug.LogError("Selection prefab is missing.", this);
					return;
				}

				GameObject go = Instantiate(prefab) as GameObject;
				AdvUguiSelection selection = go.GetComponentInChildren<AdvUguiSelection>();
				if (selection == null) selection = go.AddComponent<AdvUguiSelection>();
				if (selection) selection.Init(data, OnTap);

				switch (selectedColorMode)
				{
					case SelectedColorMode.Change:
						if (selection != null && Engine.SystemSaveData.SelectionData.Check(data))
						{
							selection.OnInitSelected(selectedColor);
						}
						break;
					case SelectedColorMode.None:
					default:
						break;
				}

				Items.Add(go);
				if (data.X == null || data.Y == null)
				{
					listViewItems.Add(go);
				}
				else
				{
					transform.AddChild(go, new Vector3(data.X.Value, data.Y.Value, 0));
				}
			}

			if (ListView != null)
			{
				ListView.AddItems(listViewItems);
				ListView.Reposition();
				Canvas.ForceUpdateCanvases();
			}
			else
			{
				int index = 0;
				foreach (GameObject item in listViewItems)
				{
					item.transform.SetParent(transform, false);
					RectTransform rectTransform = item.transform as RectTransform;
					if (rectTransform != null)
					{
						rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
						rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
						rectTransform.anchoredPosition = new Vector2(0, -120 * index);
					}
					++index;
				}
			}
		}

		protected virtual GameObject GetPrefab(AdvSelection selectionData)
		{
			GameObject prefab = null;
			if (!string.IsNullOrEmpty(selectionData.PrefabName))
			{
				prefab = PrefabList != null ? PrefabList.Find(x => x != null && x.name == selectionData.PrefabName) : null;
				if (prefab != null)
				{
					return prefab;
				}
				Debug.LogError("Not found Selection Prefab : " + selectionData.PrefabName);
			}

			prefab = PrefabList != null && PrefabList.Count > 0 ? PrefabList[0] : null;
			if (prefab == null && ListView != null)
			{
				prefab = ListView.ItemPrefab;
			}
			return prefab;
		}

		protected virtual void OnTap(AdvUguiSelection item)
		{
			SelectionManager.Select(item.Data);
			ClearAll();
		}

		public virtual void OnClear(AdvSelectionManager manager)
		{
			ClearAll();
			ApplyCanvasGroupState(false);
		}

		public virtual void OnBeginShow(AdvSelectionManager manager)
		{
			PrepareRuntimeLayout();
			CreateItems();
			ApplyCanvasGroupState(false);
		}

		public virtual void OnBeginWaitInput(AdvSelectionManager manager)
		{
			PrepareRuntimeLayout();
			ApplyCanvasGroupState(true);
		}

		void ApplyCanvasGroupState(bool interactable)
		{
			CanvasGroup.alpha = 1;
			CanvasGroup.interactable = interactable;
			CanvasGroup.blocksRaycasts = true;
		}

		protected virtual void PrepareRuntimeLayout()
		{
			transform.SetAsLastSibling();

			Mask mask = GetComponent<Mask>();
			if (mask != null)
			{
				mask.enabled = false;
			}

			RectMask2D rectMask = GetComponent<RectMask2D>();
			if (rectMask == null)
			{
				rectMask = gameObject.AddComponent<RectMask2D>();
			}
			rectMask.enabled = true;

			Image image = GetComponent<Image>();
			if (image != null)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
				image.raycastTarget = false;
			}
		}
	}
}
