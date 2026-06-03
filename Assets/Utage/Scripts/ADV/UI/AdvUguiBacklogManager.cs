// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// バックログ表示
	/// </summary>
	[AddComponentMenu("Utage/ADV/AdvUguiBacklogManager")]
	public class AdvUguiBacklogManager : MonoBehaviour
		, IAdvEngineGetter
	{
		const float LegacyBacklogContentWidth = 1543.96f;
		const float LegacyBacklogContentMinHeight = 300f;
		const float LegacyBacklogItemWidth = 1543.96f;
		const float LegacyBacklogItemMinHeight = 84f;
		const float LegacyBacklogItemBottomPadding = 18f;
		const float LegacyTextTop = -15f;
		const float LegacyTextWidth = 1197.96f;
		const float LegacyTextMinHeight = 49f;
		const float LegacyTextRight = 1543.96f;
		const float LegacyNameLeft = 17.839966f;
		const float LegacyNameWidth = 165f;
		const float LegacyNameHeight = 30f;
		const float LegacySoundLeft = 229.05997f;
		const float LegacyCollectionLeft = 299.77997f;
		const float LegacyIconCenterY = -39.5f;
		const float LegacyIconSize = 49f;
		const float LegacyHeadIconCenterY = -100f;
		const float LegacyHeadIconWidth = 276f;
		const float LegacyHeadIconHeight = 131f;
		const float RuntimeBackButtonWidth = 168f;
		const float RuntimeBackButtonHeight = 72f;
		const float RuntimeBackButtonRight = 40f;
		const float RuntimeBackButtonTop = 32f;
		const string RuntimeBackButtonName = "CodexBacklogCloseButton";
		static readonly string[] RuntimeBackButtonFontNames =
		{
			"Source Han Serif CN",
			"Source Han Serif SC",
			"Microsoft YaHei UI",
			"Microsoft YaHei",
			"SimHei",
			"Arial Unicode MS",
			"Arial",
		};

		public enum BacklogType
		{
			MessageWindow,		//メッセージウィンドウ
			FullScreenText,		//全画面テキスト
		};

		BacklogType Type { get { return type; } }
		[SerializeField]
		BacklogType type = BacklogType.MessageWindow;

		public AdvEngine Engine
		{
			get => engine;
			protected set => engine = value;
		}
		[SerializeField]
		protected AdvEngine engine;
		public AdvEngine AdvEngineGetter => Engine;


		/// <summary>選択肢のリストビュー</summary>
		public UguiListView ListView
		{
			get { return listView; }
		}
		[SerializeField]
		UguiListView listView = null;

		/// <summary>全画面テキスト</summary>
		public UguiNovelText FullScreenLogText
		{
			get { return fullScreenLogText; }
		}
		[SerializeField]
		UguiNovelText fullScreenLogText = null;

		//バックログデータへのインターフェース
		protected AdvBacklogManager BacklogManager { get { return engine.BacklogManager; } }
		
		//スクロール最下段でマウスホイール入力で閉じる入力するか
		public bool isCloseScrollWheelDown = false;

		protected bool deactivateUiRootOnBack = false;


		/// <summary>開いているか</summary>
		public virtual bool IsOpen { get { return this.gameObject.activeSelf; } }
		
	
		//外部からAdvEngineを設定する
		public void InitEngine(AdvEngine advEngine)
		{
			Engine = advEngine; 
		}

		//外部から設定したAdvEngineを開放する
		public void ReleaseEngine()
		{
			Engine = null;
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public virtual void Close()
		{
			if (ListView!=null) ListView.ClearItems();
			if (FullScreenLogText != null) FullScreenLogText.text = "";
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// 開く
		/// </summary>
		public virtual void Open()
		{
			EnsureRuntimeReferences();
			EnsureUiRootVisible();
			this.gameObject.SetActive(true);
			PrepareRuntimeLayout();
			switch( Type )
			{
				case BacklogType.FullScreenText:
					InitialzeAsFullScreenText();
					break;
				case BacklogType.MessageWindow:
				default:
					InitialzeAsMessageWindow();
					break;
			}
		}

		protected virtual void InitialzeAsMessageWindow()			
		{
			AdvBacklogManager manager = GetBacklogManager();
			if (ListView == null || manager == null) return;
			ListView.CreateItems(manager.Backlogs.Count, CallbackCreateItem);
			RefreshListViewLayout();
			if (ListView.ScrollRect != null)
			{
				ListView.ScrollRect.StopMovement();
				ListView.ScrollRect.verticalNormalizedPosition = 0f;
			}
		}

		protected virtual void InitialzeAsFullScreenText()
		{
			AdvBacklogManager manager = GetBacklogManager();
			if (ListView == null || manager == null) return;
			ListView.CreateItems(manager.Backlogs.Count, CallbackCreateItem);
			RefreshListViewLayout();
			if (ListView.ScrollRect != null)
			{
				ListView.ScrollRect.StopMovement();
				ListView.ScrollRect.verticalNormalizedPosition = 0f;
			}
		}

		/// <summary>
		/// リストビューのアイテムが作られたときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作られたアイテムのGameObject</param>
		/// <param name="index">アイテムのインデックス</param>
		protected virtual void CallbackCreateItem(GameObject go, int index)
		{
			AdvBacklogManager manager = GetBacklogManager();
			if (go == null || manager == null) return;
			AdvBacklog data = manager.Backlogs[index];
			AdvUguiBacklog backlog = go.GetComponent<AdvUguiBacklog>();
			if (backlog == null)
			{
				backlog = go.AddComponent<AdvUguiBacklog>();
			}
			backlog.Engine = this.Engine;
			backlog.Init(data);
			ApplyRuntimeItemLayout(go);
		}

		// 戻るボタンが押された
		public void OnTapBack()
		{
			Back();
		}

		// 更新
		protected virtual void Update()
		{
			//閉じる入力された
			if (InputUtil.IsInputGuiClose() || IsInputBottomEndScrollWheelDown() )
			{
				Back();
			}
		}

		// バックログ閉じて、メッセージウィンドウ開く
		protected virtual void Back()
		{
			this.Close();
			if (engine != null && engine.UiManager != null)
			{
				engine.UiManager.Status = AdvUiManager.UiStatus.Default;
			}
			if (deactivateUiRootOnBack)
			{
				AdvUguiManager uiManager = engine != null ? engine.UiManager as AdvUguiManager : null;
				if (uiManager != null)
				{
					uiManager.gameObject.SetActive(false);
				}
				deactivateUiRootOnBack = false;
			}
		}

		//スクロール最下段でマウスホイール入力で閉じる入力するチェック
		protected virtual bool IsInputBottomEndScrollWheelDown()
		{
			if(isCloseScrollWheelDown && InputUtil.IsInputCloseBackLog() && ListView != null)
			{
				if (ListView.ScrollRect == null) return false;
				Scrollbar scrollBar = ListView.ScrollRect.verticalScrollbar;
				if(scrollBar)
				{
					return scrollBar.value <= 0;
				}
			}
			return false;
		}

		protected virtual AdvBacklogManager GetBacklogManager()
		{
			return engine != null ? engine.BacklogManager : null;
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (engine == null)
			{
				engine = GetComponentInParent<AdvEngine>(true);
				if (engine == null)
				{
					foreach (AdvEngine item in Resources.FindObjectsOfTypeAll<AdvEngine>())
					{
						if (item != null && item.gameObject.scene.IsValid())
						{
							engine = item;
							break;
						}
					}
				}
			}
			if (listView == null)
			{
				listView = GetComponentInChildren<UguiListView>(true);
			}
			if (fullScreenLogText == null)
			{
				fullScreenLogText = GetComponentInChildren<UguiNovelText>(true);
			}
		}

		protected virtual void EnsureUiRootVisible()
		{
			deactivateUiRootOnBack = false;
			AdvUguiManager uiManager = engine != null ? engine.UiManager as AdvUguiManager : null;
			if (uiManager == null) return;
			if (uiManager.gameObject.activeSelf) return;

			uiManager.gameObject.SetActive(true);
			deactivateUiRootOnBack = true;
		}

		protected virtual void PrepareRuntimeLayout()
		{
			ConfigureBackdrop();
			CleanupUnexpectedContentArtifacts();
			RectTransform root = transform as RectTransform;
			if (root != null)
			{
				root.anchorMin = Vector2.zero;
				root.anchorMax = Vector2.one;
				root.offsetMin = Vector2.zero;
				root.offsetMax = Vector2.zero;
				root.localScale = Vector3.one;
			}
			BindBackButton();

			if (ListView == null) return;
			ListView.enabled = true;
			ScrollRect scrollRect = ListView.ScrollRect;
			if (scrollRect == null) return;
			ConfigureListViewVisuals(ListView, scrollRect);
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.inertia = true;
			scrollRect.scrollSensitivity = Mathf.Max(30f, scrollRect.scrollSensitivity);

			RectTransform listRect = ListView.transform as RectTransform;
			if (listRect != null && (listRect.rect.width <= 0 || listRect.rect.height <= 0))
			{
				listRect.anchorMin = new Vector2(0f, 0f);
				listRect.anchorMax = new Vector2(1f, 1f);
				listRect.offsetMin = new Vector2(220f, 96f);
				listRect.offsetMax = new Vector2(-96f, -72f);
			}
		}

		protected virtual void ApplyRuntimeItemLayout(GameObject go)
		{
			RectTransform rectTransform = go != null ? go.transform as RectTransform : null;
			if (rectTransform == null) return;
			rectTransform.anchorMin = new Vector2(1f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(1f, 0.5f);
			rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
			rectTransform.localScale = Vector3.one;
			float itemHeight = NormalizeLegacyItemLayout(rectTransform);
			rectTransform.sizeDelta = new Vector2(LegacyBacklogItemWidth, itemHeight);

			LayoutElement itemLayout = go.GetComponent<LayoutElement>();
			if (itemLayout == null)
			{
				itemLayout = go.AddComponent<LayoutElement>();
			}
			itemLayout.minWidth = LegacyBacklogItemWidth;
			itemLayout.preferredWidth = LegacyBacklogItemWidth;
			itemLayout.minHeight = itemHeight;
			itemLayout.preferredHeight = itemHeight;
			itemLayout.flexibleWidth = 0f;
			itemLayout.flexibleHeight = 0f;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		protected virtual void RefreshListViewLayout()
		{
			if (ListView == null) return;

			RectTransform content = ListView.Content;
			if (content == null) return;

			foreach (RectTransform child in content)
			{
				if (child == null) continue;
				LayoutRebuilder.ForceRebuildLayoutImmediate(child);
			}

			Canvas.ForceUpdateCanvases();
			ListView.Reposition();
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(content);

			RectTransform viewport = ResolveViewport(ListView.transform as RectTransform);
			if (viewport != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
			}
		}

		protected virtual void ConfigureBackdrop()
		{
			Image background = GetComponent<Image>();
			if (background != null)
			{
				background.color = new Color(0f, 0f, 0f, Mathf.Max(background.color.a, 0.62f));
				background.raycastTarget = true;
			}

			Transform filterTransform = FindChildRecursive(transform, "Filter");
			Image filter = filterTransform != null ? filterTransform.GetComponent<Image>() : null;
			if (filter != null)
			{
				filter.color = new Color(0f, 0f, 0f, Mathf.Max(filter.color.a, 0.6f));
				filter.raycastTarget = true;
			}
		}

		protected virtual void ConfigureListViewVisuals(UguiListView listView, ScrollRect scrollRect)
		{
			if (listView == null || scrollRect == null) return;

			RectTransform viewport = ResolveViewport(listView.transform as RectTransform);
			if (viewport != null)
			{
				scrollRect.viewport = viewport;
			}

			Graphic maskGraphic = listView.GetComponent<Graphic>();
			if (maskGraphic == null)
			{
				maskGraphic = listView.gameObject.AddComponent<Image>();
			}
			maskGraphic.enabled = true;
			maskGraphic.raycastTarget = true;
			maskGraphic.color = new Color(0f, 0f, 0f, 0f);
			if (maskGraphic.canvasRenderer != null)
			{
				maskGraphic.canvasRenderer.cullTransparentMesh = false;
			}

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

			RectTransform content = listView.Content;
			if (content != null)
			{
				content.anchorMin = new Vector2(0.5f, 1f);
				content.anchorMax = new Vector2(0.5f, 1f);
				content.pivot = new Vector2(0.5f, 1f);
				content.sizeDelta = new Vector2(LegacyBacklogContentWidth, Mathf.Max(LegacyBacklogContentMinHeight, content.sizeDelta.y));
				content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

				UguiVerticalAlignGroup alignGroup = content.GetComponent<UguiVerticalAlignGroup>();
				if (alignGroup == null)
				{
					alignGroup = content.gameObject.AddComponent<UguiVerticalAlignGroup>();
				}
				alignGroup.direction = UguiVerticalAlignGroup.AlignDirection.TopToBottom;
				alignGroup.isAutoResize = true;
				alignGroup.repositionZeroContent = true;
				alignGroup.paddingTop = 12f;
				alignGroup.paddingBottom = 12f;
				alignGroup.space = 8f;
			}

			ConfigureScrollbar(scrollRect, listView.transform);
		}

		protected virtual void CleanupUnexpectedContentArtifacts()
		{
			Transform contentRoot = FindChildRecursive(transform, "Content");
			if (contentRoot == null) return;

			foreach (Transform child in contentRoot)
			{
				if (child == null || child.name != "GameObject") continue;

				Dropdown dropdown = child.GetComponent<Dropdown>();
				RawImage rawImage = child.GetComponent<RawImage>();
				if (dropdown == null && rawImage == null) continue;

				child.gameObject.SetActive(false);
			}
		}

		protected virtual float NormalizeLegacyItemLayout(RectTransform itemRect)
		{
			if (itemRect == null) return LegacyBacklogItemMinHeight;

			SanitizeLegacyItemGraphics(itemRect);

			NormalizeLegacyChildRect(
				FindChildRecursive(itemRect, "HeadIcon") as RectTransform,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 0.5f),
				new Vector2(0f, LegacyHeadIconCenterY),
				new Vector2(LegacyHeadIconWidth, LegacyHeadIconHeight));

			RectTransform nameRect = FindChildRecursive(itemRect, "name") as RectTransform;
			NormalizeLegacyChildRect(
				nameRect,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(LegacyNameLeft, LegacyTextTop),
				new Vector2(LegacyNameWidth, LegacyNameHeight));
			RefreshGraphic(nameRect);

			NormalizeLegacyChildRect(
				FindChildRecursive(itemRect, "sound") as RectTransform,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0.5f, 0.5f),
				new Vector2(LegacySoundLeft, LegacyIconCenterY),
				new Vector2(LegacyIconSize, LegacyIconSize));

			NormalizeLegacyChildRect(
				FindChildRecursive(itemRect, "collection") as RectTransform,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0.5f, 0.5f),
				new Vector2(LegacyCollectionLeft, LegacyIconCenterY),
				new Vector2(LegacyIconSize, LegacyIconSize));

			RectTransform textRect = FindChildRecursive(itemRect, "text") as RectTransform;
			float textHeight = ResolveTextHeight(textRect);
			NormalizeLegacyChildRect(
				textRect,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(LegacyTextRight, LegacyTextTop),
				new Vector2(LegacyTextWidth, textHeight));
			if (textRect == null) return LegacyBacklogItemMinHeight;

			UguiNovelText novelText = textRect.GetComponent<UguiNovelText>();
			if (novelText != null)
			{
				novelText.alignment = TextAnchor.UpperLeft;
				novelText.SetAllDirty();
				_ = novelText.preferredHeight;
			}
			RefreshGraphic(textRect);
			return CalculateItemHeight(itemRect);
		}

		protected virtual float ResolveTextHeight(RectTransform textRect)
		{
			if (textRect == null) return LegacyTextMinHeight;

			Text legacyText = textRect.GetComponent<Text>();
			TextMeshProNovelText textMeshPro = textRect.GetComponent<TextMeshProNovelText>();
			float preferredHeight = NovelTextComponentWrapper.GetPreferredHeight(legacyText, textMeshPro);
			float currentHeight = textRect.sizeDelta.y;
			return Mathf.Max(LegacyTextMinHeight, currentHeight, preferredHeight);
		}

		protected virtual float CalculateItemHeight(RectTransform itemRect)
		{
			float maxBottom = 0f;
			maxBottom = Mathf.Max(maxBottom, GetBottomEdgeFromTop(FindChildRecursive(itemRect, "name") as RectTransform));
			maxBottom = Mathf.Max(maxBottom, GetBottomEdgeFromTop(FindChildRecursive(itemRect, "sound") as RectTransform));
			maxBottom = Mathf.Max(maxBottom, GetBottomEdgeFromTop(FindChildRecursive(itemRect, "collection") as RectTransform));
			maxBottom = Mathf.Max(maxBottom, GetBottomEdgeFromTop(FindChildRecursive(itemRect, "text") as RectTransform));
			maxBottom = Mathf.Max(maxBottom, GetBottomEdgeFromTop(FindChildRecursive(itemRect, "HeadIcon") as RectTransform));
			return Mathf.Max(LegacyBacklogItemMinHeight, maxBottom + LegacyBacklogItemBottomPadding);
		}

		protected virtual float GetBottomEdgeFromTop(RectTransform rectTransform)
		{
			if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy) return 0f;
			return -rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y * rectTransform.pivot.y;
		}

		protected virtual void NormalizeLegacyChildRect(
			RectTransform rectTransform,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 pivot,
			Vector2 anchoredPosition,
			Vector2 sizeDelta)
		{
			if (rectTransform == null) return;

			rectTransform.anchorMin = anchorMin;
			rectTransform.anchorMax = anchorMax;
			rectTransform.pivot = pivot;
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.localScale = Vector3.one;
		}

		protected virtual void RefreshGraphic(RectTransform rectTransform)
		{
			if (rectTransform == null) return;

			Graphic graphic = rectTransform.GetComponent<Graphic>();
			if (graphic != null)
			{
				DisableNullSpriteImage(graphic as Image);
				graphic.SetAllDirty();
				if (graphic is UguiNovelText novelText)
				{
					_ = novelText.preferredHeight;
				}
			}
		}

		protected virtual void SanitizeLegacyItemGraphics(RectTransform itemRect)
		{
			if (itemRect == null) return;

			foreach (Image image in itemRect.GetComponentsInChildren<Image>(true))
			{
				DisableNullSpriteImage(image);
			}
		}

		protected virtual void DisableNullSpriteImage(Image image)
		{
			if (image == null) return;
			if (image.sprite != null) return;

			image.enabled = false;
			image.raycastTarget = false;
		}

		protected virtual RectTransform ResolveViewport(RectTransform listRect)
		{
			if (listRect == null) return null;

			Transform viewport = FindChildRecursive(listRect, "Viewport");
			if (viewport != null)
			{
				return viewport as RectTransform;
			}
			return listRect;
		}

		protected virtual void ConfigureScrollbar(ScrollRect scrollRect, Transform root)
		{
			if (scrollRect == null || root == null) return;

			Scrollbar scrollbar = scrollRect.verticalScrollbar;
			if (scrollbar == null)
			{
				Transform target = FindChildRecursive(root, "Scrollbar")
					?? FindChildRecursive(root, "Scrollbar Vertical");
				if (target != null)
				{
					scrollbar = target.GetComponent<Scrollbar>();
					if (scrollbar == null)
					{
						scrollbar = target.gameObject.AddComponent<Scrollbar>();
					}
				}
			}
			if (scrollbar == null) return;

			scrollbar.direction = Scrollbar.Direction.BottomToTop;
			if (scrollbar.targetGraphic == null)
			{
				scrollbar.targetGraphic = scrollbar.GetComponent<Graphic>() ?? scrollbar.GetComponentInChildren<Graphic>(true);
			}
			if (scrollbar.handleRect == null)
			{
				Transform handle = FindChildRecursive(scrollbar.transform, "Handle");
				if (handle != null)
				{
					scrollbar.handleRect = handle as RectTransform;
				}
			}
			scrollRect.verticalScrollbar = scrollbar;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
			scrollRect.horizontalScrollbar = null;
		}

		protected virtual void BindBackButton()
		{
			Transform target = FindChildRecursive(transform, RuntimeBackButtonName);
			if (target == null)
			{
				target = CreateRuntimeBackButton();
			}
			if (target == null) return;

			ConfigureRuntimeBackButton(target);
		}

		protected virtual void ConfigureRuntimeBackButton(Transform target)
		{
			if (target == null) return;

			target.gameObject.SetActive(true);
			target.SetAsLastSibling();

			ConfigureRuntimeBackButtonRect(target as RectTransform);

			Image image = target.GetComponent<Image>();
			if (image == null)
			{
				image = target.gameObject.AddComponent<Image>();
			}
			image.color = new Color(0f, 0f, 0f, 0.72f);
			image.raycastTarget = true;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}
			button.targetGraphic = image;
			button.interactable = true;
			button.transition = Selectable.Transition.ColorTint;
			Navigation navigation = button.navigation;
			navigation.mode = Navigation.Mode.None;
			button.navigation = navigation;
			ConfigureRuntimeBackButtonColors(button);

			Text label = target.GetComponentInChildren<Text>(true);
			if (label == null)
			{
				label = CreateRuntimeBackButtonLabel(target);
			}
			ConfigureRuntimeBackButtonLabel(label);

			button.onClick.RemoveListener(OnTapBack);
			button.onClick.AddListener(OnTapBack);
		}

		protected virtual Transform CreateRuntimeBackButton()
		{
			GameObject buttonObject = new GameObject(RuntimeBackButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonObject.transform.SetParent(transform, false);
			ConfigureRuntimeBackButton(buttonObject.transform);
			return buttonObject.transform;
		}

		protected virtual void ConfigureRuntimeBackButtonRect(RectTransform rectTransform)
		{
			if (rectTransform == null) return;

			rectTransform.anchorMin = new Vector2(1f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(1f, 1f);
			rectTransform.anchoredPosition = new Vector2(-RuntimeBackButtonRight, -RuntimeBackButtonTop);
			rectTransform.sizeDelta = new Vector2(RuntimeBackButtonWidth, RuntimeBackButtonHeight);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
		}

		protected virtual void ConfigureRuntimeBackButtonColors(Button button)
		{
			if (button == null) return;

			ColorBlock colors = button.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
			colors.selectedColor = colors.highlightedColor;
			colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
			colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f);
			colors.colorMultiplier = 1f;
			colors.fadeDuration = 0.06f;
			button.colors = colors;
		}

		protected virtual Text CreateRuntimeBackButtonLabel(Transform root)
		{
			if (root == null) return null;

			GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			labelObject.transform.SetParent(root, false);

			RectTransform labelRect = labelObject.transform as RectTransform;
			if (labelRect != null)
			{
				labelRect.anchorMin = Vector2.zero;
				labelRect.anchorMax = Vector2.one;
				labelRect.pivot = new Vector2(0.5f, 0.5f);
				labelRect.offsetMin = Vector2.zero;
				labelRect.offsetMax = Vector2.zero;
				labelRect.localScale = Vector3.one;
				labelRect.localRotation = Quaternion.identity;
			}

			Text text = labelObject.GetComponent<Text>();
			ConfigureRuntimeBackButtonLabel(text);
			return text;
		}

		protected virtual void ConfigureRuntimeBackButtonLabel(Text text)
		{
			if (text == null) return;

			text.gameObject.SetActive(true);
			text.text = "返回";
			text.font = ResolveRuntimeBackButtonFont(30);
			text.fontSize = 30;
			text.fontStyle = FontStyle.Bold;
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.white;
			text.raycastTarget = false;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 18;
			text.resizeTextMaxSize = 30;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;

			Outline outline = text.GetComponent<Outline>();
			if (outline == null)
			{
				outline = text.gameObject.AddComponent<Outline>();
			}
			outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
			outline.effectDistance = new Vector2(1.25f, -1.25f);
			outline.useGraphicAlpha = true;
		}

		protected virtual Font ResolveRuntimeBackButtonFont(int size)
		{
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				if (text == null || text.font == null) continue;
				return text.font;
			}

			Font font = Font.CreateDynamicFontFromOSFont(RuntimeBackButtonFontNames, size);
			if (font != null) return font;
			return Resources.GetBuiltinResource<Font>("Arial.ttf");
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
