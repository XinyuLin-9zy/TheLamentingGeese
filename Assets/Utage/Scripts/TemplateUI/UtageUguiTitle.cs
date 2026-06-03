// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;
using System.Collections;
using System;
using System.Reflection;

namespace Utage
{

	/// <summary>
	/// タイトル表示のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiTitle")]
	public class UtageUguiTitle : UguiView
	{
		/// <summary>スターター</summary>
		public AdvEngineStarter Starter
		{
			get { return this.GetComponentCacheFindIfMissing(ref starter); }
		}

		[SerializeField] protected AdvEngineStarter starter;

		/// <summary>メインゲーム画面</summary>
		public UtageUguiMainGame mainGame;

		/// <summary>コンフィグ画面</summary>
		public UtageUguiConfig config;

		/// <summary>セーブデターのロード画面</summary>
		public UtageUguiSaveLoad load;

		///<summary>ギャラリー画面</summary>
		public UtageUguiGallery gallery;
		public bool resetTabIndexOnOpenGallery = false;

		///<summary>ダウンロード画面</summary>
		public UtageUguiLoadWait download;

		///<summary>ダウンロードボタン</summary>
		public GameObject downloadButton;

		protected bool runtimeBindingsInitialized;
		protected Coroutine deferredSpineTitleBackgroundCoroutine;
		protected Coroutine titleSpineWaveCoroutine;
		protected RectTransform titleSpineRectTransform;
		protected Vector2 titleSpineWaveBaseAnchoredPosition;
		protected Vector3 titleSpineWaveBaseLocalScale = Vector3.one;
		protected Quaternion titleSpineWaveBaseLocalRotation = Quaternion.identity;
		protected Text appVersionText;
		const float TitleSpineOverscan = 1.18f;
		const float TitleSpineMeshCropScale = 1.2f;
		const float TitleSpineWaveX = 3.5f;
		const float TitleSpineWaveY = 2.0f;
		const float TitleSpineWaveScale = 0.0035f;
		const float TitleSpineWaveRotation = 0.08f;
		const string LegacyTitleVersionText = "Version: 1.3";
		static readonly string[] LegacyTitleButtonRoots = { "Start", "Archive", "PlotMap", "Gallery", "ExtraStory" };
		static readonly string[] LegacyTitleGraphicRoots = { "Logo", "Config", "Exit" };

		protected virtual void Awake()
		{
			EnsureRuntimeReferences();
			EnsureLegacyTitleVisuals();
			EnsureButtonBindings();
		}

		protected virtual void OnEnable()
		{
			EnsureRuntimeReferences();
			EnsureLegacyTitleVisuals();
			EnsureButtonBindings();
		}

		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			EnsureExclusiveOpenState();
			EnsureLegacyTitleVisuals();
			EnsureButtonBindings();
			EnsureSpineTitleBackground();
			StartDeferredSpineTitleBackground();
			StartTitleSpineWave();
			//		if (Starter != null && Starter.enabled != AdvEngineStarter.ScenarioLoadType.Server)
			{
				if (downloadButton != null)
				{
					downloadButton.SetActive(false);
				}
			}
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (mainGame == null) mainGame = FindSceneObject<UtageUguiMainGame>();
			if (config == null) config = FindSceneObject<UtageUguiConfig>();
			if (load == null) load = FindSceneObject<UtageUguiSaveLoad>();
			if (gallery == null) gallery = FindSceneObject<UtageUguiGallery>();
			if (download == null) download = FindSceneObject<UtageUguiLoadWait>();
			if (downloadButton == null)
			{
				Transform target = FindChildRecursive(transform, "Button-Download") ?? FindChildRecursive(transform, "Download");
				if (target != null) downloadButton = target.gameObject;
			}
		}

		protected virtual void EnsureLegacyTitleVisuals()
		{
			EnsureTitleVisualHierarchy();
			RefreshTitleLanguageSpriteAdapters();
			EnsureActionButtonsBackdrop();
			EnsureTitleGraphicVisibility();
			HideLegacyActionButtonArtifacts();
			EnsureAppVersionLabel();
		}

		protected virtual void EnsureTitleVisualHierarchy()
		{
			Transform bg = FindDirectChild(transform, "BG") ?? FindChildRecursive(transform, "BG");
			if (bg != null)
			{
				bg.gameObject.SetActive(true);
				bg.SetSiblingIndex(0);
				StretchTitleRect(bg as RectTransform, 0f);

				Transform spine = FindDirectChild(bg, "Spine") ?? FindChildRecursive(bg, "Spine");
				if (spine != null)
				{
					spine.gameObject.SetActive(true);
					spine.SetAsFirstSibling();
				}

				Transform logo = FindDirectChild(bg, "Logo") ?? FindChildRecursive(bg, "Logo");
				if (logo != null)
				{
					logo.gameObject.SetActive(true);
					logo.SetAsLastSibling();
				}
			}

			Transform actionButtons = FindDirectChild(transform, "ActionBtns") ?? FindChildRecursive(transform, "ActionBtns");
			if (actionButtons != null)
			{
				actionButtons.gameObject.SetActive(true);
				if (bg != null && transform.childCount > 1)
				{
					actionButtons.SetSiblingIndex(1);
				}
			}

			foreach (string rootName in LegacyTitleButtonRoots)
			{
				Transform root = FindChildRecursive(transform, rootName);
				if (root != null) root.gameObject.SetActive(true);
			}

			foreach (string rootName in LegacyTitleGraphicRoots)
			{
				Transform root = FindChildRecursive(transform, rootName);
				if (root != null)
				{
					root.gameObject.SetActive(true);
					root.SetAsLastSibling();
				}
			}

			Transform download = FindDirectChild(transform, "Download") ?? FindChildRecursive(transform, "Download");
			if (download != null) download.SetAsLastSibling();

			Transform appVersion = FindDirectChild(transform, "AppVersion") ?? FindChildRecursive(transform, "AppVersion");
			if (appVersion != null)
			{
				appVersion.gameObject.SetActive(true);
				appVersion.SetAsLastSibling();
			}
		}

		protected virtual void RefreshTitleLanguageSpriteAdapters()
		{
			foreach (MonoBehaviour component in GetComponentsInChildren<MonoBehaviour>(true))
			{
				if (component == null) continue;

				string typeName = component.GetType().Name;
				if (typeName == "UI_LanguageImageAdapter")
				{
					InvokeSpineMethod(component, "RefreshImage");
				}
				else if (typeName == "UI_LanguageButtonAdapter")
				{
					InvokeSpineMethod(component, "RefreshButtonSprite");
				}
			}
		}

		protected virtual void EnsureTitleGraphicVisibility()
		{
			foreach (string rootName in LegacyTitleGraphicRoots)
			{
				Transform root = FindChildRecursive(transform, rootName);
				EnsureGraphicVisible(root, true);
			}

			foreach (string rootName in LegacyTitleButtonRoots)
			{
				Transform root = FindChildRecursive(transform, rootName);
				EnsureGraphicVisible(root, true);
			}

			Transform appVersion = FindDirectChild(transform, "AppVersion") ?? FindChildRecursive(transform, "AppVersion");
			EnsureGraphicVisible(appVersion, true);
		}

		protected virtual void EnsureGraphicVisible(Transform root, bool includeChildren)
		{
			if (root == null) return;

			Graphic[] graphics = includeChildren
				? root.GetComponentsInChildren<Graphic>(true)
				: root.GetComponents<Graphic>();
			foreach (Graphic graphic in graphics)
			{
				if (graphic == null) continue;

				graphic.gameObject.SetActive(true);
				graphic.enabled = true;
				Color color = graphic.color;
				if (color.a < 1f)
				{
					graphic.color = new Color(color.r, color.g, color.b, 1f);
				}
				graphic.canvasRenderer.SetAlpha(1f);
				graphic.SetAllDirty();
			}
		}

		protected virtual void HideLegacyActionButtonArtifacts()
		{
			Transform actionButtons = FindChildRecursive(transform, "ActionBtns");
			if (actionButtons == null) return;

			foreach (Transform actionRoot in actionButtons)
			{
				foreach (Image image in actionRoot.GetComponentsInChildren<Image>(true))
				{
					if (image == null) continue;
					if (!ShouldHideLegacyActionButtonImage(actionRoot, image)) continue;

					image.enabled = false;
					image.gameObject.SetActive(false);
				}
			}
		}

		protected virtual bool ShouldHideLegacyActionButtonImage(Transform actionRoot, Image image)
		{
			if (actionRoot == null || image == null) return false;

			string imageName = image.name ?? "";
			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";

			if (TitleNameEquals(imageName, "Btn")) return false;
			if (TitleNameEquals(imageName, "Lock")) return false;
			if (IsLegacyTitleButtonBridge(actionRoot, image)) return false;
			if (TitleNameContains(spriteName, "title_button_")) return false;
			if (TitleNameContains(spriteName, "lock")) return false;

			if (TitleNameContains(spriteName, "dialog_menu_explaintext_bg")) return true;
			if (!TitleNameEquals(actionRoot.name, "ExtraStory") && TitleNameContains(imageName, "Image")) return true;

			return false;
		}

		protected virtual bool IsLegacyTitleButtonBridge(Transform actionRoot, Image image)
		{
			if (actionRoot == null || image == null) return false;
			if (!TitleNameEquals(actionRoot.name, "Start") && !TitleNameEquals(actionRoot.name, "PlotMap")) return false;

			string imageName = image.name ?? "";
			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";
			string textureName = image.sprite != null && image.sprite.texture != null ? image.sprite.texture.name ?? "" : "";

			return TitleNameContains(imageName, "Bridge")
				|| TitleNameContains(imageName, "桥")
				|| TitleNameContains(spriteName, "桥")
				|| TitleNameContains(textureName, "桥");
		}

		protected virtual void EnsureExclusiveOpenState()
		{
			ForceHideView(mainGame);
			ForceHideView(config);
			ForceHideView(load);
			ForceHideView(gallery);
			ForceHideView(download);
			ForceHideView(FindComponentByTypeName("UI_PlotMap") as UguiView);
		}

		protected virtual void ForceHideView(UguiView view)
		{
			if (view == null || view == this) return;
			if (!view.gameObject.scene.IsValid()) return;
			if (!view.gameObject.activeSelf) return;

			view.gameObject.SetActive(false);
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

		protected virtual void EnsureButtonBindings()
		{
			if (runtimeBindingsInitialized) return;

			BindMomentaryControl("Start", OnTapStart);
			BindMomentaryControl("Load", OnTapLoad);
			BindMomentaryControl("Config", OnTapConfig);
			BindMomentaryControl("Gallery", OnTapGallery);
			BindMomentaryControl("Archive", OnTapArchive);
			BindMomentaryControl("PlotMap", OnTapPlotMap);
			BindMomentaryControl("ExtraStory", OnTapExtraStory);
			BindMomentaryControl("Exit", OnTapExit);
			BindMomentaryControl("Download", OnTapDownLoad);
			BindMomentaryControl("Button-Download", OnTapDownLoad);

			runtimeBindingsInitialized = true;
		}

		protected virtual void BindMomentaryControl(string name, Action action)
		{
			Transform target = FindChildRecursive(transform, name);
			if (target == null || action == null) return;

			Toggle toggle = target.GetComponent<Toggle>() ?? target.GetComponentInChildren<Toggle>(true);
			if (toggle != null)
			{
				toggle.onValueChanged.AddListener(isOn =>
				{
					if (!isOn) return;
					toggle.SetIsOnWithoutNotify(false);
					action();
				});
				return;
			}

			Button button = target.GetComponent<Button>() ?? target.GetComponentInChildren<Button>(true);
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}
			if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
			}
			if (button.GetType().Name == "AccessibleButton") return;
			button.onClick.AddListener(() => action());
		}

		protected virtual void EnsureActionButtonsBackdrop()
		{
			Transform actionButtons = FindChildRecursive(transform, "ActionBtns");
			if (actionButtons == null) return;

			RawImage rawImage = actionButtons.GetComponent<RawImage>();
			if (rawImage != null)
			{
				rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0f);
				rawImage.raycastTarget = false;
			}

			Image image = actionButtons.GetComponent<Image>();
			if (image != null)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
				image.raycastTarget = false;
			}
		}

		protected virtual void EnsureAppVersionLabel()
		{
			Transform appVersionRoot = FindChildRecursive(transform, "AppVersion");
			if (appVersionRoot == null) return;

			RectTransform rootRect = appVersionRoot as RectTransform;
			if (rootRect != null)
			{
				rootRect.anchorMin = new Vector2(1f, 1f);
				rootRect.anchorMax = new Vector2(1f, 1f);
				rootRect.pivot = new Vector2(1f, 1f);
				if (rootRect.sizeDelta.x < 180f || rootRect.sizeDelta.y < 32f)
				{
					rootRect.sizeDelta = new Vector2(220f, 40f);
				}
			}

			if (appVersionText == null)
			{
				appVersionText = appVersionRoot.GetComponent<Text>() ?? appVersionRoot.GetComponentInChildren<Text>(true);
			}
			if (appVersionText == null)
			{
				GameObject label = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
				label.transform.SetParent(appVersionRoot, false);
				RectTransform labelRect = label.transform as RectTransform;
				if (labelRect != null)
				{
					labelRect.anchorMin = Vector2.zero;
					labelRect.anchorMax = Vector2.one;
					labelRect.offsetMin = Vector2.zero;
					labelRect.offsetMax = Vector2.zero;
				}
				appVersionText = label.GetComponent<Text>();
			}

			if (appVersionText == null) return;

			if (appVersionText.font == null)
			{
				appVersionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			}
			appVersionText.text = LegacyTitleVersionText;
			appVersionText.fontSize = Mathf.Max(appVersionText.fontSize, 24);
			appVersionText.alignment = TextAnchor.MiddleRight;
			appVersionText.color = Color.white;
			appVersionText.raycastTarget = false;
			appVersionText.horizontalOverflow = HorizontalWrapMode.Overflow;
			appVersionText.verticalOverflow = VerticalWrapMode.Truncate;
		}

		protected virtual void EnsureSpineTitleBackground()
		{
			foreach (Component component in GetComponentsInChildren<Component>(true))
			{
				if (component == null || component.GetType().FullName != "Spine.Unity.SkeletonGraphic") continue;

				RectTransform rectTransform = component.transform as RectTransform;
				if (rectTransform != null && rectTransform.name == "Spine")
				{
					rectTransform.gameObject.SetActive(true);
					StretchTitleRect(rectTransform.parent as RectTransform, 0f);
					InitializeSpineGraphic(component);
					ConfigureTitleSpineGraphic(component, rectTransform);
					titleSpineRectTransform = rectTransform;
					titleSpineWaveBaseAnchoredPosition = rectTransform.anchoredPosition;
					titleSpineWaveBaseLocalScale = rectTransform.localScale;
					titleSpineWaveBaseLocalRotation = rectTransform.localRotation;
				}
			}
		}

		protected virtual void OnDisable()
		{
			if (deferredSpineTitleBackgroundCoroutine != null)
			{
				StopCoroutine(deferredSpineTitleBackgroundCoroutine);
				deferredSpineTitleBackgroundCoroutine = null;
			}
			if (titleSpineWaveCoroutine != null)
			{
				StopCoroutine(titleSpineWaveCoroutine);
				titleSpineWaveCoroutine = null;
			}
			ResetTitleSpineWaveTransform();
		}

		protected virtual void StartDeferredSpineTitleBackground()
		{
			if (!isActiveAndEnabled) return;

			if (deferredSpineTitleBackgroundCoroutine != null)
			{
				StopCoroutine(deferredSpineTitleBackgroundCoroutine);
			}
			deferredSpineTitleBackgroundCoroutine = StartCoroutine(CoDeferredSpineTitleBackground());
		}

		protected virtual IEnumerator CoDeferredSpineTitleBackground()
		{
			yield return null;
			EnsureSpineTitleBackground();
			yield return new WaitForEndOfFrame();
			EnsureSpineTitleBackground();
			deferredSpineTitleBackgroundCoroutine = null;
		}

		protected virtual void StartTitleSpineWave()
		{
			if (!isActiveAndEnabled) return;

			if (titleSpineWaveCoroutine != null)
			{
				StopCoroutine(titleSpineWaveCoroutine);
			}
			titleSpineWaveCoroutine = StartCoroutine(CoTitleSpineWave());
		}

		protected virtual IEnumerator CoTitleSpineWave()
		{
			float time = 0f;
			while (true)
			{
				time += Time.unscaledDeltaTime;
				if (titleSpineRectTransform != null && titleSpineRectTransform.gameObject.activeInHierarchy)
				{
					float x = Mathf.Sin(time * 0.47f) * TitleSpineWaveX + Mathf.Sin(time * 0.19f) * (TitleSpineWaveX * 0.35f);
					float y = Mathf.Cos(time * 0.31f) * TitleSpineWaveY;
					float scale = 1f + Mathf.Sin(time * 0.23f) * TitleSpineWaveScale;
					float rotation = Mathf.Sin(time * 0.17f) * TitleSpineWaveRotation;

					titleSpineRectTransform.anchoredPosition = titleSpineWaveBaseAnchoredPosition + new Vector2(x, y);
					titleSpineRectTransform.localScale = new Vector3(
						titleSpineWaveBaseLocalScale.x * scale,
						titleSpineWaveBaseLocalScale.y * scale,
						titleSpineWaveBaseLocalScale.z);
					titleSpineRectTransform.localRotation = titleSpineWaveBaseLocalRotation * Quaternion.Euler(0f, 0f, rotation);
				}
				yield return null;
			}
		}

		protected virtual void ResetTitleSpineWaveTransform()
		{
			if (titleSpineRectTransform == null) return;

			titleSpineRectTransform.anchoredPosition = titleSpineWaveBaseAnchoredPosition;
			titleSpineRectTransform.localScale = titleSpineWaveBaseLocalScale;
			titleSpineRectTransform.localRotation = titleSpineWaveBaseLocalRotation;
		}

		protected virtual void ConfigureTitleSpineGraphic(Component component, RectTransform rectTransform)
		{
			if (component == null || rectTransform == null) return;

			Graphic uiGraphic = component as Graphic;
			if (uiGraphic != null) uiGraphic.raycastTarget = false;
			EnsureSpineAdaptFitter(rectTransform);

			if (IsValidSpineGraphic(component))
			{
				ResetSpineLayoutState(component);
				SetSpineLayoutScaleMode(component, 0);
				InvokeSpineMethod(component, "MatchRectTransformWithBounds");
			}

			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			ApplySpineOverscan(rectTransform);

			if (IsValidSpineGraphic(component))
			{
				SetSpineLayoutScaleMode(component, 4);
				InvokeSpineMethod(component, "Update", 0f);
				InvokeSpineMethod(component, "UpdateMesh");
				ApplyTitleSpineMeshCover(component, rectTransform);
			}
		}

		protected virtual void ApplyTitleSpineMeshCover(Component component, RectTransform rectTransform)
		{
			RectTransform parent = rectTransform != null ? rectTransform.parent as RectTransform : null;
			if (component == null || rectTransform == null || parent == null) return;

			Bounds bounds;
			if (!TryGetSpineMeshBounds(component, out bounds)) return;

			Vector2 parentSize = parent.rect.size;
			if (parentSize.x <= 0f || parentSize.y <= 0f) return;

			float boundsWidth = Mathf.Abs(bounds.size.x);
			float boundsHeight = Mathf.Abs(bounds.size.y);
			if (boundsWidth <= 0.01f || boundsHeight <= 0.01f) return;

			Vector2 targetSize = parentSize * TitleSpineOverscan;
			float coverScale = Mathf.Max(targetSize.x / boundsWidth, targetSize.y / boundsHeight);
			if (float.IsNaN(coverScale) || float.IsInfinity(coverScale)) return;

			coverScale = Mathf.Clamp(coverScale * TitleSpineMeshCropScale, 0.25f, 8f);
			rectTransform.localScale = new Vector3(coverScale, coverScale, 1f);
			rectTransform.anchoredPosition = new Vector2(-bounds.center.x * coverScale, -bounds.center.y * coverScale);
		}

		protected virtual bool TryGetSpineMeshBounds(Component component, out Bounds bounds)
		{
			bounds = default(Bounds);
			object meshObject = InvokeSpineMethod(component, "GetLastMesh");
			Mesh mesh = meshObject as Mesh;
			if (mesh == null || mesh.vertexCount <= 0) return false;

			mesh.RecalculateBounds();
			bounds = mesh.bounds;
			return bounds.size != Vector3.zero;
		}

		protected virtual void ApplySpineOverscan(RectTransform rectTransform)
		{
			if (rectTransform == null || TitleSpineOverscan <= 1f) return;

			RectTransform parent = rectTransform.parent as RectTransform;
			if (parent == null) return;

			Vector2 parentSize = parent.rect.size;
			if (parentSize.x <= 0 || parentSize.y <= 0) return;

			rectTransform.sizeDelta = parentSize * (TitleSpineOverscan - 1f);
		}

		protected virtual void StretchTitleRect(RectTransform rectTransform, float padding)
		{
			if (rectTransform == null) return;

			Vector2 pad = new Vector2(Mathf.Max(0f, padding), Mathf.Max(0f, padding));
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = -pad;
			rectTransform.offsetMax = pad;
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
		}

		protected virtual void EnsureSpineAdaptFitter(RectTransform rectTransform)
		{
			if (rectTransform == null) return;

			Type fitterType = FindTypeByName("UI_SpineAdaptFitter");
			if (fitterType == null || !typeof(Component).IsAssignableFrom(fitterType)) return;

			if (rectTransform.GetComponent(fitterType) == null)
			{
				rectTransform.gameObject.AddComponent(fitterType);
			}
		}

		protected static Type FindTypeByName(string typeName)
		{
			if (string.IsNullOrEmpty(typeName)) return null;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(typeName);
				if (type != null) return type;
			}
			return null;
		}

		protected virtual void InitializeSpineGraphic(Component component)
		{
			InvokeSpineMethod(component, "Initialize", false);
		}

		protected virtual bool IsValidSpineGraphic(Component component)
		{
			if (component == null) return false;

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = component.GetType().GetProperty("IsValid", flags);
			if (property == null || property.PropertyType != typeof(bool)) return true;

			try
			{
				return (bool)property.GetValue(component, null);
			}
			catch
			{
				return true;
			}
		}

		protected virtual void SetSpineLayoutScaleMode(Component component, int value)
		{
			if (component == null) return;

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = component.GetType().GetField("layoutScaleMode", flags);
			if (field == null || !field.FieldType.IsEnum) return;

			try
			{
				field.SetValue(component, Enum.ToObject(field.FieldType, value));
			}
			catch { }
		}

		protected virtual object InvokeSpineMethod(Component component, string methodName, params object[] args)
		{
			if (component == null) return null;

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo method = FindSpineMethod(component.GetType(), methodName, args);
			if (method == null) return null;

			try
			{
				return method.Invoke(component, args);
			}
			catch
			{
				return null;
			}
		}

		protected virtual MethodInfo FindSpineMethod(Type type, string methodName, object[] args)
		{
			if (type == null) return null;

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo[] methods = type.GetMethods(flags);
			int argumentCount = args == null ? 0 : args.Length;
			foreach (MethodInfo method in methods)
			{
				if (method.Name != methodName) continue;

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length != argumentCount) continue;

				bool matches = true;
				for (int i = 0; i < parameters.Length; ++i)
				{
					object value = args[i];
					if (value != null && !parameters[i].ParameterType.IsInstanceOfType(value))
					{
						matches = false;
						break;
					}
				}
				if (matches) return method;
			}
			return null;
		}

		protected virtual void ResetSpineLayoutState(Component component)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			try
			{
				FieldInfo referenceScale = component.GetType().GetField("referenceScale", flags);
				if (referenceScale != null) referenceScale.SetValue(component, 1f);

				FieldInfo layoutScale = component.GetType().GetField("layoutScale", flags);
				if (layoutScale != null) layoutScale.SetValue(component, 1f);

				FieldInfo pivotOffset = component.GetType().GetField("pivotOffset", flags);
				if (pivotOffset != null) pivotOffset.SetValue(component, Vector2.zero);
			}
			catch
			{
				// Spine still renders with its serialized layout values if these internals change.
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

		protected static Transform FindDirectChild(Transform root, string name)
		{
			if (root == null) return null;

			foreach (Transform child in root)
			{
				if (child.name == name) return child;
			}
			return null;
		}

		protected static bool TitleNameEquals(string value, string expected)
		{
			return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
		}

		protected static bool TitleNameContains(string value, string fragment)
		{
			return !string.IsNullOrEmpty(value)
				&& value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		///「はじめから」ボタンが押された
		public virtual void OnTapStart()
		{
			EnsureRuntimeReferences();
			if (mainGame == null) return;
			Close();
			mainGame.OpenStartGame();
		}

		///「つづきから」ボタンが押された
		public virtual void OnTapLoad()
		{
			EnsureRuntimeReferences();
			if (load == null) return;
			Close();
			load.OpenLoad(this);
		}

		///「コンフィグ」ボタンが押された
		public virtual void OnTapConfig()
		{
			EnsureRuntimeReferences();
			if (config == null) return;
			Close();
			config.Open(this);
		}

		//「ギャラリー」ボタンが押された
		public virtual void OnTapGallery()
		{
			EnsureRuntimeReferences();
			if (gallery == null) return;
			Close();
			gallery.OpenNamedView(this, "CgGallery");
		}

		//「回想」ボタンが押された
		public virtual void OnTapArchive()
		{
			EnsureRuntimeReferences();
			if (load == null) return;
			Close();
			load.OpenLoad(this);
		}

		//「フローチャート」ボタンが押された
		public virtual void OnTapPlotMap()
		{
			UguiView plotMap = FindViewByName("UI_PlotMap");
			if (plotMap == null) return;

			Close();
			plotMap.Open(this);
			MethodInfo showMap = plotMap.GetType().GetMethod("ShowMap", new[] { typeof(bool) });
			if (showMap != null)
			{
				showMap.Invoke(plotMap, new object[] { true });
			}
			else
			{
				plotMap.SendMessage("ShowMap", true, SendMessageOptions.DontRequireReceiver);
			}
		}

		//「番外」ボタンが押された
		public virtual void OnTapExtraStory()
		{
			Component extraStory = FindComponentByTypeName("UI.UI_ExtraStory");
			if (extraStory != null)
			{
				MethodInfo refreshState = extraStory.GetType().GetMethod("RefreshState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (refreshState != null)
				{
					refreshState.Invoke(extraStory, null);
				}

				PropertyInfo isUnlocked = extraStory.GetType().GetProperty("IsUnlocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (isUnlocked != null && isUnlocked.PropertyType == typeof(bool) && !(bool)isUnlocked.GetValue(extraStory, null))
				{
					MethodInfo showLockedMessage = extraStory.GetType().GetMethod("ShowLockedMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (showLockedMessage != null)
					{
						showLockedMessage.Invoke(extraStory, null);
					}
					else
					{
						extraStory.SendMessage("OnClickExtraStory", SendMessageOptions.DontRequireReceiver);
					}
					return;
				}

				string label = extraStory.GetType().GetField("extraStoryTag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(extraStory) as string;
				label = string.IsNullOrEmpty(label)
					? "map_other_1"
					: label;
				OnTapStartLabel(label);
				return;
			}

			Transform target = FindChildRecursive(transform, "ExtraStory");
			if (target != null)
			{
				target.gameObject.SendMessage("OnClickExtraStory", SendMessageOptions.DontRequireReceiver);
				if (!gameObject.activeSelf)
				{
					return;
				}
			}

			OnTapStartLabel("map_other_1");
		}

		//「終了」ボタンが押された
		public virtual void OnTapExit()
		{
			Application.Quit();
		}

		//「ダウンロード」ボタンが押された
		public virtual void OnTapDownLoad()
		{
			EnsureRuntimeReferences();
			if (download == null) return;
			Close();
			download.Open(this);
		}

		///「指定のラベルからスタート」ボタンが押された
		public virtual void OnTapStartLabel(string label)
		{
			EnsureRuntimeReferences();
			if (mainGame == null) return;
			Close();
			mainGame.OpenStartLabel(label);
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

		protected virtual Component FindComponentByTypeName(string fullTypeName)
		{
			if (string.IsNullOrEmpty(fullTypeName)) return null;

			foreach (MonoBehaviour component in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
			{
				if (component == null) continue;
				if (!component.gameObject.scene.IsValid()) continue;
				if (component.GetType().FullName != fullTypeName) continue;
				return component;
			}
			return null;
		}


		protected virtual void OnCloseLoadChapter(string startLabel)
		{
			download.onClose.RemoveAllListeners();
			Close();
			mainGame.OpenStartLabel(startLabel);
		}
	}
}
