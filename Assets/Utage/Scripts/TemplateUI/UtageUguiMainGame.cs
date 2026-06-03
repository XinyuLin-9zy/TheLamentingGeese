// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// メインゲーム画面のサンプル
	/// 入力処理に起点になるため、スクリプトの実行順を通常よりも少しはやくすること
	/// http://docs-jp.unity3d.com/Documentation/Components/class-ScriptExecution.html
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiMainGame")]
	public class UtageUguiMainGame : UguiView
	{
		/// <summary>ADVエンジン</summary>
		public virtual AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;

		/// <summary>キャプチャ用のカメラ</summary>
		public virtual LetterBoxCamera LetterBoxCamera
		{
			get { return this.GetComponentCacheFindIfMissing(ref letterBoxCamera); }
		}

		[SerializeField] protected LetterBoxCamera letterBoxCamera;


		/// <summary>タイトル画面</summary>
		public UtageUguiTitle title;

		/// <summary>コンフィグ画面</summary>
		public UtageUguiConfig config;

		/// <summary>セーブロード画面</summary>
		public UtageUguiSaveLoad saveLoad;

		/// <summary>ギャラリー画面</summary>
		public UtageUguiGallery gallery;

		/// <summary>ボタン</summary>
		public GameObject buttons;

		/// <summary>スキップボタン</summary>
		public Toggle checkSkip;

		/// <summary>自動で読み進むボタン</summary>
		public Toggle checkAuto;

		//ガイドメッセージの表示。設定してないときは表示しない
		public SystemUiGuideMessage guideMessage;

		//起動タイプ
		protected enum BootType
		{
			Default,
			Start,
			Load,
			SceneGallery,
			StartLabel,
		};

		protected BootType bootType;

		//ロードするセーブデータ
		protected AdvSaveData loadData;

		protected bool isInit = false;

		/// <summary>起動するシナリオラベル</summary>
		protected string scenarioLabel;

		protected bool buttonBindingsInitialized;
		protected bool buttonVisualsInitialized;
		protected bool buttonHintBindingsInitialized;
		protected Toggle expandToggle;
		protected GameObject buttonExplainRoot;
		protected Text buttonExplainText;
		protected string currentButtonHintName;
		protected UnityAction<bool> saveToggleListener;
		protected UnityAction<bool> loadToggleListener;
		protected UnityAction<bool> configToggleListener;
		protected UnityAction<bool> plotMapToggleListener;
		protected UnityAction<bool> autoToggleListener;
		protected UnityAction<bool> skipToggleListener;
		protected UnityAction<bool> historyToggleListener;
		protected UnityAction<bool> hideUiToggleListener;
		protected UnityAction<bool> expandToggleListener;

		const float ButtonHintVerticalGap = 6f;

		protected struct ButtonLayoutSpec
		{
			public ButtonLayoutSpec(float anchorX, float anchorY, float posX, float posY, float width, float height, float pivotX = 0.5f, float pivotY = 0f)
			{
				AnchorMin = new Vector2(anchorX, anchorY);
				AnchorMax = new Vector2(anchorX, anchorY);
				AnchoredPosition = new Vector2(posX, posY);
				SizeDelta = new Vector2(width, height);
				Pivot = new Vector2(pivotX, pivotY);
			}

			public Vector2 AnchorMin;
			public Vector2 AnchorMax;
			public Vector2 AnchoredPosition;
			public Vector2 SizeDelta;
			public Vector2 Pivot;
		}

		protected virtual void Awake()
		{
			AdvEngine advEngine = Engine;
			if (advEngine != null && advEngine.Page != null)
			{
				advEngine.Page.OnEndText.AddListener((page) => CaptureScreenOnSavePoint(page));
			}
			EnsureRuntimeReferences();
			EnsureButtonBindings();
		}

		protected virtual void OnEnable()
		{
			EnsureRuntimeReferences();
			EnsureButtonBindings();
		}

		/// <summary>
		/// 画面を閉じる
		/// </summary>
		public override void Close()
		{
			base.Close();
			AdvEngine advEngine = Engine;
			if (advEngine == null) return;
			if (advEngine.UiManager != null) advEngine.UiManager.Close();
			if (advEngine.Config != null) advEngine.Config.IsSkip = false;
		}

		//起動データをクリア
		protected virtual void ClearBootData()
		{
			bootType = BootType.Default;
			isInit = false;
			loadData = null;
		}

		/// <summary>
		/// ゲームをはじめから開始
		/// </summary>
		public virtual void OpenStartGame()
		{
			ClearBootData();
			bootType = BootType.Start;
			Open();
		}

		/// <summary>
		/// 指定ラベルからゲーム開始
		/// </summary>
		public virtual void OpenStartLabel(string label)
		{
			ClearBootData();
			bootType = BootType.StartLabel;
			this.scenarioLabel = label;
			Open();
		}

		/// <summary>
		/// セーブデータをロードしてゲーム再開
		/// </summary>
		/// <param name="loadData">ロードするセーブデータ</param>
		public virtual void OpenLoadGame(AdvSaveData loadData)
		{
			ClearBootData();
			bootType = BootType.Load;
			this.loadData = loadData;
			Open();
		}

		/// <summary>
		/// シーン回想としてシーンを開始
		/// </summary>
		/// <param name="scenarioLabel">シーンラベル</param>
		public virtual void OpenSceneGallery(string scenarioLabel)
		{
			ClearBootData();
			bootType = BootType.SceneGallery;
			this.scenarioLabel = scenarioLabel;
			Open();
		}

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			EnsureExclusiveOpenState();
			EnsureButtonBindings();
			AdvEngine advEngine = Engine;
			if (advEngine == null)
			{
				Debug.LogError("[UtageUguiMainGame] AdvEngine is missing. Main game cannot be opened.", this);
				return;
			}

			//スクショをクリア
			if (advEngine.SaveManager != null && advEngine.SaveManager.Type != AdvSaveManager.SaveType.SavePoint)
			{
				advEngine.SaveManager.ClearCaptureTexture();
			}

			StartCoroutine(CoWaitOpen());
		}

		protected virtual void EnsureButtonBindings()
		{
			EnsureRuntimeReferences();
			if (buttons == null) return;

			Transform root = buttons.transform;
			EnsureButtonVisuals(root);
			EnsureButtonHintBindings(root);
			if (buttonBindingsInitialized && !AreButtonToggleDelegatesLost(root)) return;
			if (checkSkip == null) checkSkip = FindToggle(root, "Skip");
			if (checkAuto == null) checkAuto = FindToggle(root, "Auto");
			if (expandToggle == null) expandToggle = FindToggle(root, "Expand");

			BindMomentaryControl(root, "Save", OnTapSave, ref saveToggleListener);
			BindMomentaryControl(root, "Load", OnTapLoad, ref loadToggleListener);
			BindMomentaryControl(root, "Config", OnTapConfig, ref configToggleListener);
			BindMomentaryControl(root, "PlotMap", OnTapPlotMap, ref plotMapToggleListener);
			BindStateToggle(root, "Auto", OnTapAuto, ref autoToggleListener);
			BindStateToggle(root, "Skip", OnTapSkip, ref skipToggleListener);
			BindMomentaryControl(root, "History", OpenBacklogFromMainGame, ref historyToggleListener);
			BindMomentaryControl(root, "HideUI", () =>
			{
				if (Engine != null && Engine.UiManager != null)
				{
					Engine.UiManager.Status = AdvUiManager.UiStatus.HideMessageWindow;
				}
			}, ref hideUiToggleListener);
			BindMomentaryControl(root, "Expand", OnTapExpand, ref expandToggleListener);
			SetMenuButtonVisible(root, "QSave", false);
			SetMenuButtonVisible(root, "QLoad", false);

			buttonBindingsInitialized = true;
		}

		protected virtual bool AreButtonToggleDelegatesLost(Transform root)
		{
			return IsTogglePresentWithoutDelegate(root, "Save", saveToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "Load", loadToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "Config", configToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "PlotMap", plotMapToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "Auto", autoToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "Skip", skipToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "History", historyToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "HideUI", hideUiToggleListener)
			       || IsTogglePresentWithoutDelegate(root, "Expand", expandToggleListener);
		}

		protected virtual bool IsTogglePresentWithoutDelegate(Transform root, string name, UnityAction<bool> listener)
		{
			return FindToggle(root, name) != null && listener == null;
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (title == null) title = FindSceneObject<UtageUguiTitle>();
			if (config == null) config = FindSceneObject<UtageUguiConfig>();
			if (saveLoad == null) saveLoad = FindSceneObject<UtageUguiSaveLoad>();
			if (gallery == null) gallery = FindSceneObject<UtageUguiGallery>();
			if (buttons == null)
			{
				Transform buttonsTransform = FindChildRecursive(transform, "Buttons");
				if (buttonsTransform != null) buttons = buttonsTransform.gameObject;
			}
		}

		protected virtual T FindSceneObject<T>() where T : Component
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

		protected virtual void BindMomentaryControl(Transform root, string name, Action action, ref UnityAction<bool> listener)
		{
			Toggle toggle = FindToggle(root, name);
			if (toggle != null)
			{
				if (listener != null)
				{
					toggle.onValueChanged.RemoveListener(listener);
				}
				listener = isOn =>
				{
					if (!isOn) return;
					toggle.SetIsOnWithoutNotify(false);
					action();
				};
				toggle.SetIsOnWithoutNotify(false);
				toggle.onValueChanged.AddListener(listener);
				return;
			}

			BindButton(root, name, action);
		}

		protected virtual void BindStateToggle(Transform root, string name, Action<bool> action, ref UnityAction<bool> listener)
		{
			Toggle toggle = FindToggle(root, name);
			if (toggle == null || action == null) return;

			if (listener != null)
			{
				toggle.onValueChanged.RemoveListener(listener);
			}
			listener = isOn => action(isOn);
			toggle.onValueChanged.AddListener(listener);
		}

		protected virtual void BindButton(Transform root, string name, Action action)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null || action == null) return;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}
			if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
			}

			button.onClick.AddListener(() => action());
		}

		protected virtual Toggle FindToggle(Transform root, string name)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) return null;
			return target.GetComponent<Toggle>() ?? target.GetComponentInChildren<Toggle>(true);
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


		//起動待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			AdvEngine advEngine = Engine;
			if (advEngine == null) yield break;
			while (advEngine.IsWaitBootLoading) yield return null;

			switch (bootType)
			{
				case BootType.Default:
					if (advEngine.UiManager != null) advEngine.UiManager.Open();
					break;
				case BootType.Start:
					advEngine.StartGame();
					break;
				case BootType.Load:
					advEngine.OpenLoadGame(loadData);
					break;
				case BootType.SceneGallery:
					advEngine.StartSceneGallery(scenarioLabel);
					break;
				case BootType.StartLabel:
					advEngine.StartGame(scenarioLabel);
					break;
			}

			ClearBootData();
			loadData = null;
			if (advEngine.Config != null) advEngine.Config.IsSkip = false;
			isInit = true;
		}

		//更新中
		protected virtual void Update()
		{
			if (!isInit) return;
			AdvEngine advEngine = Engine;
			if (advEngine == null) return;

			//ローディングアイコンを表示
			if (SystemUi.GetInstance())
			{
				if (advEngine.IsLoading)
				{
					SystemUi.GetInstance().StartIndicator(this);
				}
				else
				{
					SystemUi.GetInstance().StopIndicator(this);
				}
			}


			if (advEngine.IsEndScenario)
			{
				Close();
				EnsureRuntimeReferences();
				if (advEngine.IsSceneGallery)
				{
					//回想シーン終了したのでギャラリーに
					if (gallery != null) gallery.Open();
				}
				else
				{
					//シナリオ終了したのでタイトルへ
					if (title != null) title.Open(this);
				}
			}
		}
		
		//表示の更新
		//UtageUguiMenuButtonsと機能が重複しているので、
		//UtageUguiMenuButtonsを使う場合は、buttonsやcheckSkipをnullにすること
		protected virtual void LateUpdate()
		{
			EnsureRuntimeReferences();
			EnsureButtonBindings();
			AdvEngine advEngine = Engine;
			if (advEngine == null) return;

			//メニューボタンの表示・表示を切り替え
			if (buttons != null)
			{
				bool isHideUi = advEngine.UiManager != null && advEngine.UiManager.Status == AdvUiManager.UiStatus.HideMessageWindow;
				buttons.SetActive(advEngine.UiManager != null && advEngine.UiManager.IsShowingMenuButton &&
				                  (advEngine.UiManager.Status == AdvUiManager.UiStatus.Default || isHideUi));
				UpdateMenuButtonStates(buttons.transform, isHideUi);
			}

			//スキップフラグを反映
			if (checkSkip && advEngine.Config != null)
			{
				if (checkSkip.isOn != advEngine.Config.IsSkip)
				{
					checkSkip.isOn = advEngine.Config.IsSkip;
				}
				UpdateToggleIndicator(checkSkip);
			}

			//オートフラグを反映
			if (checkAuto && advEngine.Config != null)
			{
				if (checkAuto.isOn != advEngine.Config.IsAutoBrPage)
				{
					checkAuto.isOn = advEngine.Config.IsAutoBrPage;
				}
				UpdateToggleIndicator(checkAuto);
			}

			if (string.IsNullOrEmpty(currentButtonHintName))
			{
				UpdateDefaultButtonExplainText();
			}
		}

		protected virtual void CaptureScreenOnSavePoint(AdvPage page)
		{
			AdvEngine advEngine = Engine;
			if (advEngine == null || advEngine.SaveManager == null || page == null) return;
			if (advEngine.SaveManager.Type == AdvSaveManager.SaveType.SavePoint)
			{
				if (page.IsSavePoint)
				{
//					Debug.Log("Capture");
					StartCoroutine(CoCaptureScreen());
				}
			}
		}

		protected virtual IEnumerator CoCaptureScreen()
		{
			yield return new WaitForEndOfFrame();
			//セーブ用のスクショを撮る
			AdvEngine advEngine = Engine;
			if (advEngine != null && advEngine.SaveManager != null)
			{
				advEngine.SaveManager.CaptureTexture = CaptureScreen();
			}
		}

		//スキップボタンが押された
		//UtageUguiMenuButtonsと機能が重複しているので注意
		public virtual void OnTapSkip(bool isOn)
		{
			if (Engine != null && Engine.Config != null) Engine.Config.IsSkip = isOn;
		}

		//自動読み進みボタンが押された
		//UtageUguiMenuButtonsと機能が重複しているので注意
		public virtual void OnTapAuto(bool isOn)
		{
			if (Engine != null && Engine.Config != null) Engine.Config.IsAutoBrPage = isOn;
		}

		//コンフィグボタンが押された
		public virtual void OnTapConfig()
		{
			EnsureRuntimeReferences();
			if (config == null) return;
			Close();
			config.Open(this);
		}

		//セーブボタンが押された
		public virtual void OnTapSave()
		{
			EnsureRuntimeReferences();
			if (saveLoad == null) return;
			if (Engine == null || Engine.IsSceneGallery) return;
			if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;

			StartCoroutine(CoSave());
		}

		protected virtual void OnTapPlotMap()
		{
			UguiView plotMap = FindViewByName("UI_PlotMap");
			if (plotMap == null) return;

			Close();
			plotMap.Open(this);
			System.Reflection.MethodInfo showMap = plotMap.GetType().GetMethod("ShowMap", new Type[] { typeof(bool) });
			if (showMap != null)
			{
				showMap.Invoke(plotMap, new object[] { false });
			}
		}

		protected virtual UguiView FindViewByName(string viewName)
		{
			foreach (UguiView view in Resources.FindObjectsOfTypeAll<UguiView>())
			{
				if (view != null && view.name == viewName && view.gameObject.scene.IsValid())
				{
					return view;
				}
			}
			return null;
		}

		protected virtual IEnumerator CoSave()
		{
			AdvEngine advEngine = Engine;
			if (advEngine == null || advEngine.SaveManager == null) yield break;
			if (advEngine.SaveManager.Type != AdvSaveManager.SaveType.SavePoint)
			{
				yield return new WaitForEndOfFrame();
				//セーブ用のスクショを撮る
				advEngine.SaveManager.CaptureTexture = CaptureScreen();
			}

			//セーブ画面開く
			Close();
			EnsureRuntimeReferences();
			if (saveLoad == null) yield break;
			saveLoad.OpenSave(this);
		}

		//ロードボタンが押された
		public virtual void OnTapLoad()
		{
			EnsureRuntimeReferences();
			if (saveLoad == null) return;
			if (Engine == null || Engine.IsSceneGallery) return;

			Close();
			saveLoad.OpenLoad(this);
		}

		protected virtual void OpenBacklogFromMainGame()
		{
			if (Engine == null || Engine.UiManager == null) return;

			GameObject uiRoot = Engine.UiManager.gameObject;
			if (uiRoot != null && !uiRoot.activeSelf)
			{
				uiRoot.SetActive(true);
			}
			Engine.UiManager.Status = AdvUiManager.UiStatus.Backlog;
		}

		protected virtual void OnTapExpand()
		{
			if (Engine == null || Engine.UiManager == null) return;

			Engine.UiManager.Status = AdvUiManager.UiStatus.Default;
			if (expandToggle != null)
			{
				expandToggle.SetIsOnWithoutNotify(false);
			}
		}

		//クイックセーブボタンが押された
		public virtual void OnTapQSave()
		{
			if (Engine == null || Engine.IsSceneGallery) return;

			if (Engine.Config != null) Engine.Config.IsSkip = false;
			StartCoroutine(CoQSave());
		}

		protected virtual IEnumerator CoQSave()
		{
			AdvEngine advEngine = Engine;
			if (advEngine == null || advEngine.SaveManager == null) yield break;
			if (advEngine.SaveManager.Type != AdvSaveManager.SaveType.SavePoint)
			{
				yield return new WaitForEndOfFrame();
				//セーブ用のスクショを撮る
				advEngine.SaveManager.CaptureTexture = CaptureScreen();
			}

			//クイックセーブ
			advEngine.QuickSave();
			//スクショをクリア
			if (advEngine.SaveManager.Type != AdvSaveManager.SaveType.SavePoint)
			{
				advEngine.SaveManager.ClearCaptureTexture();
			}

			//ガイドメッセージの表示
			if (guideMessage != null)
			{
				guideMessage.Open(LanguageSystemText.LocalizeText(SystemText.UtageGuideMessageQuickSave));
			}
		}

		//クイックロードボタンが押された
		public virtual void OnTapQLoad()
		{
			if (Engine == null || Engine.IsSceneGallery) return;

			if (Engine.Config != null) Engine.Config.IsSkip = false;
			Engine.QuickLoad();
			
			//ガイドメッセージの表示
			if (guideMessage!=null)
			{
				guideMessage.Open(LanguageSystemText.LocalizeText(SystemText.UtageGuideMessageQuickLoad));
			}
		}


		//セーブ用のスクショを撮る
		protected virtual Texture2D CaptureScreen()
		{
			AdvEngine advEngine = Engine;
			if (advEngine == null || advEngine.SaveManager == null || !advEngine.SaveManager.EnableCapture(advEngine.Param))
			{
				return null;
			}
			Camera camera = LetterBoxCamera != null ? LetterBoxCamera.CachedCamera : null;
			Rect rect = camera != null ? camera.rect : new Rect(0, 0, 1, 1);
			int x = Mathf.CeilToInt(rect.x * Screen.width);
			int y = Mathf.CeilToInt(rect.y * Screen.height);
			int width = Mathf.FloorToInt(rect.width * Screen.width);
			int height = Mathf.FloorToInt(rect.height * Screen.height);
			return UtageToolKit.CaptureScreen(new Rect(x, y, width, height));
		}

		protected virtual void EnsureButtonVisuals(Transform root)
		{
			if (buttonVisualsInitialized || root == null) return;

			EnsureButtonLayout(root);
			NormalizeSwappedButtonSprites(root);
			EnsureTargetGraphic(root, "PlotMap");
			EnsureTargetGraphic(root, "Save");
			EnsureTargetGraphic(root, "Load");
			EnsureTargetGraphic(root, "Auto");
			EnsureTargetGraphic(root, "Skip");
			EnsureTargetGraphic(root, "History");
			EnsureTargetGraphic(root, "Config");
			EnsureTargetGraphic(root, "HideUI");
			EnsureTargetGraphic(root, "Expand");
			EnsureToggleIndicator(root, "Auto", "Icon");
			EnsureToggleIndicator(root, "Skip", "Icon");

			buttonVisualsInitialized = true;
		}

		protected virtual void EnsureButtonLayout(Transform root)
		{
			RestoreButtonLayout(root, "PlotMap");
			RestoreButtonLayout(root, "Save");
			RestoreButtonLayout(root, "Load");
			RestoreButtonLayout(root, "Auto");
			RestoreButtonLayout(root, "Skip");
			RestoreButtonLayout(root, "History");
			RestoreButtonLayout(root, "Config");
			RestoreButtonLayout(root, "HideUI");
		}

		protected virtual void RestoreButtonLayout(Transform root, string name)
		{
			RectTransform rect = FindChildRecursive(root, name) as RectTransform;
			if (rect == null) return;

			ButtonLayoutSpec spec;
			if (!TryGetButtonLayoutSpec(name, out spec)) return;
			if (!ShouldRestoreButtonLayout(rect, spec)) return;

			rect.anchorMin = spec.AnchorMin;
			rect.anchorMax = spec.AnchorMax;
			rect.pivot = spec.Pivot;
			rect.anchoredPosition = spec.AnchoredPosition;
			rect.sizeDelta = spec.SizeDelta;
		}

		protected virtual bool ShouldRestoreButtonLayout(RectTransform rect, ButtonLayoutSpec spec)
		{
			const float epsilon = 0.01f;

			bool anchorLooksCollapsed = rect.anchorMin.y < 0.5f && spec.AnchorMin.y > 0.5f;
			bool positionLooksZeroed = rect.anchoredPosition.sqrMagnitude <= epsilon && spec.AnchoredPosition.sqrMagnitude > epsilon;
			bool anchorMismatch =
				Mathf.Abs(rect.anchorMin.x - spec.AnchorMin.x) > epsilon ||
				Mathf.Abs(rect.anchorMin.y - spec.AnchorMin.y) > epsilon ||
				Mathf.Abs(rect.anchorMax.x - spec.AnchorMax.x) > epsilon ||
				Mathf.Abs(rect.anchorMax.y - spec.AnchorMax.y) > epsilon;

			return anchorLooksCollapsed || positionLooksZeroed || anchorMismatch;
		}

		protected virtual bool TryGetButtonLayoutSpec(string name, out ButtonLayoutSpec spec)
		{
			switch (name)
			{
				case "PlotMap":
					spec = new ButtonLayoutSpec(0f, 1f, 39f, -34.5f, 78f, 69f, 0.5f, 0.5f);
					return true;
				case "Save":
					spec = new ButtonLayoutSpec(0f, 1f, 135f, -69f, 78f, 69f);
					return true;
				case "Load":
					spec = new ButtonLayoutSpec(0f, 1f, 231f, -69f, 78f, 69f);
					return true;
				case "Auto":
					spec = new ButtonLayoutSpec(0f, 1f, 327f, -69f, 78f, 69f);
					return true;
				case "Skip":
					spec = new ButtonLayoutSpec(0f, 1f, 423f, -69f, 78f, 69f);
					return true;
				case "History":
					spec = new ButtonLayoutSpec(0f, 1f, 519f, -34.5f, 78f, 69f, 0.5f, 0.5f);
					return true;
				case "Config":
					spec = new ButtonLayoutSpec(0f, 1f, 615f, -69f, 78f, 69f);
					return true;
				case "HideUI":
					spec = new ButtonLayoutSpec(0f, 1f, 711f, -34.5f, 78f, 69f, 0.5f, 0.5f);
					return true;
				default:
					spec = default;
					return false;
			}
		}

		protected virtual void NormalizeSwappedButtonSprites(Transform root)
		{
			Image saveImage = FindChildRecursive(root, "Save")?.GetComponent<Image>();
			Image loadImage = FindChildRecursive(root, "Load")?.GetComponent<Image>();
			if (saveImage == null || loadImage == null || saveImage.sprite == null || loadImage.sprite == null) return;

			bool saveLooksLikeLoad = saveImage.sprite.name.IndexOf("load", StringComparison.OrdinalIgnoreCase) >= 0;
			bool loadLooksLikeSave = loadImage.sprite.name.IndexOf("save", StringComparison.OrdinalIgnoreCase) >= 0;
			if (!saveLooksLikeLoad || !loadLooksLikeSave) return;

			Sprite sprite = saveImage.sprite;
			saveImage.sprite = loadImage.sprite;
			loadImage.sprite = sprite;
		}

		protected virtual void EnsureTargetGraphic(Transform root, string name)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) return;

			Graphic graphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
			if (graphic == null) return;

			Button button = target.GetComponent<Button>();
			if (button != null) button.targetGraphic = graphic;

			Toggle toggle = target.GetComponent<Toggle>();
			if (toggle != null) toggle.targetGraphic = graphic;
		}

		protected virtual void EnsureToggleIndicator(Transform root, string name, string childName)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) return;

			Toggle toggle = target.GetComponent<Toggle>();
			if (toggle == null) return;

			Graphic graphic = FindChildRecursive(target, childName)?.GetComponent<Graphic>();
			if (graphic == null) return;

			graphic.enabled = true;
			graphic.raycastTarget = false;
			if (!graphic.gameObject.activeSelf) graphic.gameObject.SetActive(true);
			toggle.graphic = graphic;
			UpdateToggleIndicator(toggle);
		}

		protected virtual void UpdateToggleIndicator(Toggle toggle)
		{
			if (toggle == null || toggle.graphic == null) return;

			if (!toggle.graphic.enabled) toggle.graphic.enabled = true;
			if (!toggle.graphic.gameObject.activeSelf) toggle.graphic.gameObject.SetActive(true);
			toggle.graphic.canvasRenderer.SetAlpha(toggle.isOn ? 1f : 0f);
		}

		protected virtual void EnsureButtonHintBindings(Transform root)
		{
			if (buttonHintBindingsInitialized || root == null) return;

			buttonExplainRoot = FindChildRecursive(root, "ExplainImage")?.gameObject;
			buttonExplainText = buttonExplainRoot != null ? buttonExplainRoot.GetComponentInChildren<Text>(true) : null;
			if (buttonExplainText == null)
			{
				buttonHintBindingsInitialized = true;
				return;
			}
			foreach (Graphic graphic in buttonExplainRoot.GetComponentsInChildren<Graphic>(true))
			{
				graphic.raycastTarget = false;
			}

			BindButtonHint(root, "PlotMap");
			BindButtonHint(root, "Save");
			BindButtonHint(root, "Load");
			BindButtonHint(root, "Auto");
			BindButtonHint(root, "Skip");
			BindButtonHint(root, "History");
			BindButtonHint(root, "Config");
			BindButtonHint(root, "HideUI");
			BindButtonHint(root, "Expand");
			UpdateDefaultButtonExplainText();

			buttonHintBindingsInitialized = true;
		}

		protected virtual void BindButtonHint(Transform root, string name)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) return;

			EventTrigger trigger = target.GetComponent<EventTrigger>();
			if (trigger == null)
			{
				trigger = target.gameObject.AddComponent<EventTrigger>();
			}

			RectTransform targetRect = target as RectTransform;
			AddEventTrigger(trigger, EventTriggerType.PointerEnter, () => ShowButtonHint(name, targetRect));
			AddEventTrigger(trigger, EventTriggerType.Select, () => ShowButtonHint(name, targetRect));
			AddEventTrigger(trigger, EventTriggerType.PointerExit, () => HideButtonHint(name));
			AddEventTrigger(trigger, EventTriggerType.Deselect, () => HideButtonHint(name));
		}

		protected virtual void AddEventTrigger(EventTrigger trigger, EventTriggerType type, Action action)
		{
			if (trigger == null || action == null) return;

			EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
			entry.callback.AddListener(_ => action());
			trigger.triggers.Add(entry);
		}

		protected virtual void UpdateButtonExplainText(string buttonName)
		{
			UpdateButtonExplainText(buttonName, null);
		}

		protected virtual void UpdateButtonExplainText(string buttonName, RectTransform targetRect)
		{
			if (buttonExplainText == null) return;

			string text = GetButtonExplainText(buttonName);
			buttonExplainText.text = text ?? string.Empty;
			if (!string.IsNullOrEmpty(buttonExplainText.text))
			{
				PositionButtonExplain(buttonName, targetRect);
			}
			if (buttonExplainRoot != null)
			{
				buttonExplainRoot.SetActive(!string.IsNullOrEmpty(buttonExplainText.text));
			}
		}

		protected virtual void ShowButtonHint(string buttonName, RectTransform targetRect)
		{
			currentButtonHintName = buttonName;
			UpdateButtonExplainText(buttonName, targetRect);
		}

		protected virtual void HideButtonHint(string buttonName)
		{
			if (!string.Equals(currentButtonHintName, buttonName, StringComparison.Ordinal)) return;

			currentButtonHintName = null;
			UpdateDefaultButtonExplainText();
		}

		protected virtual void PositionButtonExplain(string buttonName, RectTransform targetRect)
		{
			if (buttonExplainRoot == null) return;

			RectTransform explainRect = buttonExplainRoot.transform as RectTransform;
			if (explainRect == null) return;
			if (targetRect == null && buttons != null)
			{
				targetRect = FindChildRecursive(buttons.transform, buttonName) as RectTransform;
			}
			if (targetRect == null || explainRect.parent != targetRect.parent) return;

			RectTransform parentRect = explainRect.parent as RectTransform;
			float explainWidth = explainRect.rect.width;
			float explainHeight = explainRect.rect.height;
			float centerX = targetRect.anchoredPosition.x + (0.5f - targetRect.pivot.x) * targetRect.rect.width;
			float bottomY = targetRect.anchoredPosition.y - targetRect.pivot.y * targetRect.rect.height;
			float topY = bottomY + targetRect.rect.height;
			if (parentRect != null && explainWidth > 0f)
			{
				float halfWidth = explainWidth * 0.5f;
				centerX = Mathf.Clamp(centerX, halfWidth, Mathf.Max(halfWidth, parentRect.rect.width - halfWidth));
			}

			explainRect.anchorMin = targetRect.anchorMin;
			explainRect.anchorMax = targetRect.anchorMax;
			if (parentRect != null && bottomY - ButtonHintVerticalGap - explainHeight < -parentRect.rect.height)
			{
				explainRect.pivot = new Vector2(0.5f, 0f);
				explainRect.anchoredPosition = new Vector2(centerX, topY + ButtonHintVerticalGap);
			}
			else
			{
				explainRect.pivot = new Vector2(0.5f, 1f);
				explainRect.anchoredPosition = new Vector2(centerX, bottomY - ButtonHintVerticalGap);
			}
			explainRect.SetAsLastSibling();
		}

		protected virtual void UpdateDefaultButtonExplainText()
		{
			if (buttonExplainText == null) return;

			if (Engine != null && Engine.UiManager != null && Engine.UiManager.Status == AdvUiManager.UiStatus.HideMessageWindow)
			{
				UpdateButtonExplainText("Expand");
				return;
			}

			if (checkAuto != null && checkAuto.isOn)
			{
				UpdateButtonExplainText("Auto");
				return;
			}

			if (checkSkip != null && checkSkip.isOn)
			{
				UpdateButtonExplainText("Skip");
				return;
			}

			UpdateButtonExplainText(null);
		}

		protected virtual string GetButtonExplainText(string buttonName)
		{
			if (string.IsNullOrEmpty(buttonName)) return string.Empty;

			string language = NormalizeButtonHintLanguage(LanguageManagerBase.Instance != null ? LanguageManagerBase.Instance.CurrentLanguage : string.Empty);
			switch (language)
			{
				case "tc":
					return GetTraditionalChineseButtonHint(buttonName);
				case "english":
					return GetEnglishButtonHint(buttonName);
				case "japanese":
					return GetJapaneseButtonHint(buttonName);
				case "russian":
					return GetEnglishButtonHint(buttonName);
				default:
					return GetSimplifiedChineseButtonHint(buttonName);
			}
		}

		protected virtual string GetSimplifiedChineseButtonHint(string buttonName)
		{
			switch (buttonName)
			{
				case "PlotMap": return "记忆梳理";
				case "Save": return "存储进度(S)";
				case "Load": return "加载进度(D)";
				case "Auto": return "自动";
				case "Skip": return "跳过";
				case "History": return "历史记录";
				case "Config": return "系统设定";
				case "HideUI": return "隐藏界面";
				case "Expand": return "恢复界面";
				case "QSave": return "快速存档";
				case "QLoad": return "快速读档";
				default: return string.Empty;
			}
		}

		protected virtual string GetTraditionalChineseButtonHint(string buttonName)
		{
			switch (buttonName)
			{
				case "PlotMap": return "記憶梳理";
				case "Save": return "儲存進度(S)";
				case "Load": return "載入進度(D)";
				case "Auto": return "自動";
				case "Skip": return "跳過";
				case "History": return "歷史記錄";
				case "Config": return "系統設定";
				case "HideUI": return "隱藏介面";
				case "Expand": return "恢復介面";
				case "QSave": return "快速存檔";
				case "QLoad": return "快速讀檔";
				default: return string.Empty;
			}
		}

		protected virtual string GetEnglishButtonHint(string buttonName)
		{
			switch (buttonName)
			{
				case "PlotMap": return "Flowchart";
				case "Save": return "Save";
				case "Load": return "Load";
				case "Auto": return "Auto";
				case "Skip": return "Skip";
				case "History": return "History";
				case "Config": return "System Settings";
				case "HideUI": return "Hide UI";
				case "Expand": return "Show UI";
				case "QSave": return "Quick Save";
				case "QLoad": return "Quick Load";
				default: return string.Empty;
			}
		}

		protected virtual string GetJapaneseButtonHint(string buttonName)
		{
			switch (buttonName)
			{
				case "PlotMap": return "フローチャート";
				case "Save": return "セーブ";
				case "Load": return "ロード";
				case "Auto": return "オート";
				case "Skip": return "スキップ";
				case "History": return "履歴";
				case "Config": return "システム設定";
				case "HideUI": return "UI非表示";
				case "Expand": return "UI表示";
				case "QSave": return "クイックセーブ";
				case "QLoad": return "クイックロード";
				default: return string.Empty;
			}
		}

		protected virtual void EnsureExclusiveOpenState()
		{
			ForceHideView(title);
			ForceHideView(config);
			ForceHideView(saveLoad);
			ForceHideView(gallery);
		}

		protected virtual void ForceHideView(UguiView view)
		{
			if (view == null || view == this) return;
			if (!view.gameObject.scene.IsValid()) return;
			if (!view.gameObject.activeSelf) return;

			view.gameObject.SetActive(false);
		}

		protected virtual void UpdateMenuButtonStates(Transform root, bool isHideUi)
		{
			if (root == null) return;

			SetMenuButtonVisible(root, "PlotMap", !isHideUi);
			SetMenuButtonVisible(root, "Save", !isHideUi);
			SetMenuButtonVisible(root, "Load", !isHideUi);
			SetMenuButtonVisible(root, "Auto", !isHideUi);
			SetMenuButtonVisible(root, "Skip", !isHideUi);
			SetMenuButtonVisible(root, "History", !isHideUi);
			SetMenuButtonVisible(root, "Config", !isHideUi);
			SetMenuButtonVisible(root, "HideUI", !isHideUi);
			SetMenuButtonVisible(root, "QSave", false);
			SetMenuButtonVisible(root, "QLoad", false);
			SetMenuButtonVisible(root, "ExplainImage", !isHideUi);
			SetMenuButtonVisible(root, "Expand", isHideUi);
		}

		protected virtual void SetMenuButtonVisible(Transform root, string name, bool visible)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) return;
			if (target.gameObject.activeSelf == visible) return;

			target.gameObject.SetActive(visible);
		}

		protected virtual string NormalizeButtonHintLanguage(string language)
		{
			if (string.IsNullOrEmpty(language)) return "sc";

			string lower = language.Trim().ToLowerInvariant();
			if (lower.Contains("tc") || lower.Contains("traditional")) return "tc";
			if (lower.Contains("english") || lower == "en") return "english";
			if (lower.Contains("japanese") || lower == "ja") return "japanese";
			if (lower.Contains("russian") || lower == "ru") return "russian";
			return "sc";
		}
	}
}
