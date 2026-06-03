// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Utage
{

	/// <summary>
	/// ギャラリー表示のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiGallery")]
	public class UtageUguiGallery : UguiView
	{
		public UguiView[] views;
		protected int tabIndex = -1;
		protected GameObject[] runtimeViewObjects;
		protected bool suppressDefaultTabOpen;
		protected bool runtimeTabButtonsBound;

		// タブインデックスを全てリセットしてオープン
		public virtual void OpenAndResetAllTabIndex(UguiView prev)
		{
			EnsureRuntimeReferences();
			SetNamedTabButtonsVisible(true);
			var lastTabIndex = tabIndex;
			tabIndex = -1;
			this.Open(prev);
			
			//全てのタブインデックスを0にリセット
			foreach (var toggleGroup in GetComponentsInChildren<UguiToggleGroupIndexed>(true))
			{
				//トグル変更のアニメーションをいったん無効化
				var toggles = toggleGroup.TogglesToArray;
				List<Toggle.ToggleTransition> toggleTransitions = new List<Toggle.ToggleTransition>();  
				foreach (var toggle in toggles)
				{
					if (toggle == null) continue;
					toggleTransitions.Add(toggle.toggleTransition);
					toggle.toggleTransition = Toggle.ToggleTransition.None;
				}
				//タブインデックスを0にリセット
				toggleGroup.CurrentIndex = 0;
				//トグル変更のアニメーションを戻しておく
				for (var i = 0; i < toggles.Length && i < toggleTransitions.Count; i++)
				{
					if (toggles[i] == null) continue;
					toggles[i].toggleTransition = toggleTransitions[i];
				}
			}
			//今の画面を開く
			tabIndex = 0;
			if (lastTabIndex == 0 && HasViewAt(lastTabIndex))
			{
				OpenViewAt(lastTabIndex);
			}
		}

		public virtual void OpenNamedView(UguiView prev, string viewName)
		{
			EnsureRuntimeReferences();
			NormalizeGalleryShell();
			suppressDefaultTabOpen = true;
			tabIndex = -1;
			this.Open(prev);
			suppressDefaultTabOpen = false;

			CloseAllRuntimeViews();
			GameObject target = FindViewObject(viewName);
			if (target == null)
			{
				Debug.LogWarning("Gallery view not found: " + viewName, this);
				return;
			}

			SetNamedTabButtonsVisible(IsViewBoundToNamedTab(target));
			UpdateTabIndexForView(target);
			CloseKnownGalleryViewsExcept(target);
			OpenViewObject(target);
			ForceActivateViewObject(target);
			CloseKnownGalleryViewsExcept(target);
		}

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			NormalizeGalleryShell();
			if (suppressDefaultTabOpen) return;
			SetNamedTabButtonsVisible(true);

			if (tabIndex < 0 && runtimeViewObjects != null && runtimeViewObjects.Length > 0)
			{
				tabIndex = 0;
			}
			if (tabIndex >= 0)
			{
				CloseAllRuntimeViews();
				OpenViewAt(tabIndex);
			}
		}
		

		//一時的に表示オフ
		public virtual void Sleep()
		{
			this.gameObject.SetActive(false);
		}

		//一時的な表示オフを解除
		public virtual void WakeUp()
		{
			this.gameObject.SetActive(true);
			NormalizeGalleryShell();
		}

		public virtual void OnTabIndexChanged(int index)
		{
			EnsureRuntimeReferences();
			NormalizeGalleryShell();

			if (!HasViewAt(index))
			{
				Debug.LogWarning("Gallery view index is not available: " + index, this);
				return;
			}
			
			// 直前にOpenNamedViewで開かれたSceneGalleryなども含めて閉じる
			GameObject targetView = runtimeViewObjects[index];
			SetNamedTabButtonsVisible(true);
			foreach (GameObject viewObject in EnumerateViewObjects())
			{
				if (viewObject == null || viewObject == targetView) continue;
				CloseViewObject(viewObject);
			}

			OpenViewObject(targetView);
			tabIndex = index;
		}

		protected virtual void EnsureRuntimeReferences()
		{
			UguiToggleGroupIndexed tabGroup = FindTabGroup();
			if (tabGroup != null)
			{
				tabGroup.OnValueChanged.RemoveListener(OnTabIndexChanged);
				tabGroup.OnValueChanged.AddListener(OnTabIndexChanged);
			}

			Transform tabRoot = tabGroup != null ? tabGroup.transform : FindChildRecursive(transform, "TabButtons");
			List<Transform> tabButtons = FindNamedTabButtons(tabRoot);
			BindNamedTabButtons(tabButtons);
			BindBackButton(tabRoot);

			Toggle[] toggles = FindTabToggles(tabGroup);
			int count = Mathf.Max(views != null ? views.Length : 0, Mathf.Max(toggles.Length, tabButtons.Count));
			count = Mathf.Max(count, 3);
			if (count <= 0) return;

			if (views == null || views.Length < count)
			{
				UguiView[] repairedViews = new UguiView[count];
				if (views != null)
				{
					for (int i = 0; i < views.Length; ++i)
					{
						repairedViews[i] = views[i];
					}
				}
				views = repairedViews;
			}

			runtimeViewObjects = new GameObject[count];
			for (int i = 0; i < count; ++i)
			{
				if (i < views.Length && views[i] != null)
				{
					runtimeViewObjects[i] = views[i].gameObject;
					continue;
				}

				string viewName = "";
				if (i < tabButtons.Count && tabButtons[i] != null)
				{
					viewName = tabButtons[i].name;
				}
				else if (i < toggles.Length && toggles[i] != null)
				{
					viewName = toggles[i].name;
				}
				GameObject fallback = FindViewObject(viewName);
				if (fallback == null) continue;

				runtimeViewObjects[i] = fallback;
				UguiView fallbackView = fallback.GetComponent<UguiView>();
				if (fallbackView != null)
				{
					views[i] = fallbackView;
				}
			}

			EnsureRuntimeViewObject(0, "CgGallery");
			EnsureRuntimeViewObject(1, "SoundRoom");
			EnsureRuntimeViewObject(2, "VoiceCollection");
			NormalizeGalleryShell();
		}

		protected virtual void NormalizeGalleryShell()
		{
			Transform tabRoot = FindChildRecursive(transform, "TabButtons");
			if (tabRoot != null)
			{
				foreach (Graphic graphic in tabRoot.GetComponents<Graphic>())
				{
					if (graphic == null) continue;
					graphic.enabled = false;
					graphic.raycastTarget = false;
				}
			}

			Transform staffRollButton = transform.Find("Button");
			if (staffRollButton != null)
			{
				staffRollButton.gameObject.SetActive(false);
			}
		}

		protected virtual void EnsureRuntimeViewObject(int index, string viewName)
		{
			if (index < 0 || string.IsNullOrEmpty(viewName)) return;
			if (runtimeViewObjects == null || index >= runtimeViewObjects.Length) return;
			if (runtimeViewObjects[index] != null) return;

			GameObject viewObject = FindViewObject(viewName);
			if (viewObject == null) return;

			runtimeViewObjects[index] = viewObject;
			if (views == null || index >= views.Length || views[index] != null) return;

			UguiView view = viewObject.GetComponent<UguiView>();
			if (view != null)
			{
				views[index] = view;
			}
		}

		protected virtual UguiToggleGroupIndexed FindTabGroup()
		{
			Transform tabRoot = FindChildRecursive(transform, "TabButtons");
			if (tabRoot != null)
			{
				UguiToggleGroupIndexed group = tabRoot.GetComponent<UguiToggleGroupIndexed>() ?? tabRoot.GetComponentInChildren<UguiToggleGroupIndexed>(true);
				if (group != null) return group;
			}

			return GetComponentsInChildren<UguiToggleGroupIndexed>(true).FirstOrDefault(group => group != null && group.transform != transform);
		}

		protected virtual Toggle[] FindTabToggles(UguiToggleGroupIndexed tabGroup)
		{
			Transform tabRoot = tabGroup != null ? tabGroup.transform : FindChildRecursive(transform, "TabButtons");
			if (tabRoot == null) return new Toggle[0];
			return tabRoot.GetComponentsInChildren<Toggle>(true).Where(toggle => toggle != null).ToArray();
		}

		protected virtual List<Transform> FindNamedTabButtons(Transform tabRoot)
		{
			List<Transform> results = new List<Transform>();
			if (tabRoot == null) return results;

			foreach (Transform child in tabRoot)
			{
				if (child == null) continue;
				if (child.name == "ButtonBack" || child.name == "Back" || child.name == "Close") continue;
				if (FindViewObject(child.name) != null)
				{
					results.Add(child);
				}
			}
			return results;
		}

		protected virtual void BindNamedTabButtons(List<Transform> tabButtons)
		{
			if (runtimeTabButtonsBound || tabButtons == null) return;
			if (tabButtons.Count == 0) return;

			for (int i = 0; i < tabButtons.Count; ++i)
			{
				Transform target = tabButtons[i];
				if (target == null) continue;

				Button button = target.GetComponent<Button>();
				if (button != null)
				{
					if (button.targetGraphic == null)
					{
						button.targetGraphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
					}
					int buttonIndex = i;
					button.onClick.AddListener(() => OnTabIndexChanged(buttonIndex));
					continue;
				}

				int index = i;
				BindTabEventTrigger(target.gameObject, index);
			}

			runtimeTabButtonsBound = true;
		}

		protected virtual void BindTabEventTrigger(GameObject target, int index)
		{
			if (target == null) return;

			EventTrigger trigger = target.GetComponent<EventTrigger>();
			if (trigger == null)
			{
				trigger = target.AddComponent<EventTrigger>();
			}
			if (trigger.triggers == null)
			{
				trigger.triggers = new List<EventTrigger.Entry>();
			}

			EventTrigger.Entry pointerClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
			pointerClick.callback.AddListener(_ => OnTabIndexChanged(index));
			trigger.triggers.Add(pointerClick);

			EventTrigger.Entry submit = new EventTrigger.Entry { eventID = EventTriggerType.Submit };
			submit.callback.AddListener(_ => OnTabIndexChanged(index));
			trigger.triggers.Add(submit);
		}

		protected virtual void BindBackButton(Transform tabRoot)
		{
			if (tabRoot == null) return;

			Transform target = FindChildRecursive(tabRoot, "ButtonBack") ?? FindChildRecursive(tabRoot, "Back") ?? FindChildRecursive(tabRoot, "Close");
			if (target == null) return;

			Button button = target.GetComponent<Button>() ?? target.GetComponentInChildren<Button>(true);
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}
			if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
			}
			button.onClick.RemoveListener(OnTapBack);
			button.onClick.AddListener(OnTapBack);
		}

		protected virtual bool IsViewBoundToNamedTab(GameObject target)
		{
			if (target == null) return false;

			Transform tabRoot = FindChildRecursive(transform, "TabButtons");
			if (tabRoot == null) return false;

			foreach (Transform button in FindNamedTabButtons(tabRoot))
			{
				if (button == null) continue;
				GameObject viewObject = FindViewObject(button.name);
				if (viewObject == target)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void SetNamedTabButtonsVisible(bool visible)
		{
			Transform tabRoot = FindChildRecursive(transform, "TabButtons");
			if (tabRoot == null) return;

			foreach (Transform child in tabRoot)
			{
				if (child == null) continue;
				if (IsBackButtonName(child.name))
				{
					child.gameObject.SetActive(true);
					continue;
				}
				if (FindViewObject(child.name) == null) continue;
				child.gameObject.SetActive(visible);
			}
		}

		protected virtual bool IsBackButtonName(string buttonName)
		{
			return buttonName == "ButtonBack" || buttonName == "Back" || buttonName == "Close";
		}

		protected virtual bool HasViewAt(int index)
		{
			return runtimeViewObjects != null
				&& index >= 0
				&& index < runtimeViewObjects.Length
				&& runtimeViewObjects[index] != null;
		}

		protected virtual void OpenViewAt(int index)
		{
			if (!HasViewAt(index)) return;
			OpenViewObject(runtimeViewObjects[index]);
		}

		protected virtual void UpdateTabIndexForView(GameObject target)
		{
			if (target == null || runtimeViewObjects == null) return;

			for (int i = 0; i < runtimeViewObjects.Length; ++i)
			{
				if (runtimeViewObjects[i] != target) continue;
				tabIndex = i;
				return;
			}
		}

		protected virtual void CloseAllRuntimeViews()
		{
			foreach (GameObject viewObject in EnumerateViewObjects())
			{
				CloseViewObject(viewObject);
			}
		}

		protected virtual IEnumerable<GameObject> EnumerateViewObjects()
		{
			HashSet<GameObject> results = new HashSet<GameObject>();

			if (runtimeViewObjects != null)
			{
				foreach (GameObject viewObject in runtimeViewObjects)
				{
					if (viewObject != null && results.Add(viewObject)) yield return viewObject;
				}
			}

			foreach (Transform child in transform)
			{
				if (child == null || !IsGalleryViewObject(child)) continue;
				if (results.Add(child.gameObject)) yield return child.gameObject;
			}
		}

		protected virtual bool IsGalleryViewObject(Transform child)
		{
			if (child == null) return false;
			if (child.name == "TabButtons" || child.name == "Button" || child.name == "ButtonBack" || child.name == "Shelter") return false;

			return child.GetComponent<UguiView>() != null
				|| child.GetComponent("UtageUguiVoiceCollection") != null;
		}

		protected virtual void OpenViewObject(GameObject viewObject)
		{
			if (viewObject == null) return;

			UguiView view = viewObject.GetComponent<UguiView>();
			if (view != null && view != this)
			{
				view.Open(this);
			}
			else
			{
				viewObject.SetActive(true);
			}
		}

		protected virtual void ForceActivateViewObject(GameObject viewObject)
		{
			if (viewObject == null) return;
			if (viewObject.activeSelf) return;

			viewObject.SetActive(true);
		}

		protected virtual void CloseViewObject(GameObject viewObject)
		{
			if (viewObject == null) return;

			UguiView view = viewObject.GetComponent<UguiView>();
			if (view != null && view != this)
			{
				view.ToggleOpen(false);
			}
			else
			{
				viewObject.SetActive(false);
			}
		}

		protected virtual GameObject FindViewObject(string viewName)
		{
			if (string.IsNullOrEmpty(viewName)) return null;

			GameObject localMatch = FindViewObjectInHierarchy(transform, viewName);
			if (localMatch != null) return localMatch;

			foreach (string alias in EnumerateViewAliases(viewName))
			{
				if (alias == viewName) continue;

				localMatch = FindViewObjectInHierarchy(transform, alias);
				if (localMatch != null) return localMatch;
			}

			return FindViewObjectInScene(viewName);
		}

		protected virtual GameObject FindViewObjectInHierarchy(Transform root, string viewName)
		{
			if (root == null || string.IsNullOrEmpty(viewName)) return null;

			foreach (Transform child in root)
			{
				if (child != null && child.name == viewName)
				{
					return child.gameObject;
				}
			}

			Transform tabRoot = FindChildRecursive(root, "TabButtons");
			foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
			{
				if (child == null || child == root || child.name != viewName) continue;
				if (tabRoot != null && child.IsChildOf(tabRoot)) continue;
				return child.gameObject;
			}

			return null;
		}

		protected virtual GameObject FindViewObjectInScene(string viewName)
		{
			Scene currentScene = gameObject.scene;
			GameObject bestMatch = null;

			foreach (string alias in EnumerateViewAliases(viewName))
			{
				bestMatch = FindSceneObjectByName(alias, currentScene);
				if (bestMatch != null) return bestMatch;
			}

			return null;
		}

		protected virtual IEnumerable<string> EnumerateViewAliases(string viewName)
		{
			yield return viewName;

			switch (viewName)
			{
				case "Gallery":
					yield return "CgGallery";
					yield break;
				case "Archive":
					yield return "SceneGallery";
					yield break;
			}
		}

		protected virtual GameObject FindSceneObjectByName(string objectName, Scene scene)
		{
			if (string.IsNullOrEmpty(objectName)) return null;

			GameObject firstExactName = null;
			foreach (Transform item in Resources.FindObjectsOfTypeAll<Transform>())
			{
				if (item == null || item.name != objectName) continue;
				if (!item.gameObject.scene.IsValid()) continue;
				if (scene.IsValid() && item.gameObject.scene != scene) continue;
				if (item == transform) continue;

				if (item.IsChildOf(transform))
				{
					Transform tabRoot = FindChildRecursive(transform, "TabButtons");
					if (tabRoot != null && item.IsChildOf(tabRoot)) continue;
					return item.gameObject;
				}

				if (firstExactName == null && IsProbableViewObject(item))
				{
					firstExactName = item.gameObject;
				}
			}

			return firstExactName;
		}

		protected virtual void CloseKnownGalleryViewsExcept(GameObject keep)
		{
			string[] names =
			{
				"CgGallery",
				"SoundRoom",
				"VoiceCollection",
				"SceneGallery",
			};

			foreach (string name in names)
			{
				GameObject candidate = FindViewObject(name);
				if (candidate == null || candidate == keep) continue;
				CloseViewObject(candidate);
			}
		}

		protected virtual bool IsProbableViewObject(Transform candidate)
		{
			if (candidate == null) return false;
			if (candidate.GetComponent<UguiView>() != null) return true;
			if (candidate.GetComponent("UtageUguiVoiceCollection") != null) return true;
			if (candidate.GetComponent<CanvasRenderer>() != null && candidate.GetComponent<Graphic>() != null) return true;
			return false;
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
}
