// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Utage
{

	/// <summary>
	/// ロード待ち画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiLoadWait")]
	public class UtageUguiLoadWait : UguiView
	{
		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;

		/// <summary>スターター</summary>
		public AdvEngineStarter Starter
		{
			get { return this.GetComponentCacheFindIfMissing(ref starter); }
		}

		[SerializeField] protected AdvEngineStarter starter;

		public bool isAutoCacheFileLoad;

		public UtageUguiTitle title;

		public string bootSceneName;

		public GameObject buttonSkip;
		public GameObject buttonBack;
		public GameObject buttonDownload;
		public GameObject loadingBarRoot;
		public Image loadingBar;
		[HideIfTMP] public Text textMain;
		[HideIfLegacyText] public TextMeshProUGUI textMainTmp;
		[HideIfTMP] public Text textCount;
		[HideIfLegacyText] public TextMeshProUGUI textCountTmp;

		const string CodexLoadingRootName = "CodexRuntimeLoadingView";
		const string CodexProgressRootName = "CodexRuntimeProgress";
		const string CodexLoadingMainTextName = "CodexRuntimeMainText";
		const string CodexLoadingCountTextName = "CodexRuntimeCountText";
		const float CodexMinimumVisibleTime = 0.75f;

		RectTransform codexLoadingRoot;
		Image codexProgressGlow;
		Text codexMainText;
		Text codexCountText;
		Coroutine codexLoadingCoroutine;
		int codexActiveStarterLoadCount;
		bool codexWaitingForRecovery;
		float codexDisplayedProgress;
		int codexMaxDownloadCount;

		/// <summary>
		/// ダイアログ呼び出し
		/// </summary>
		public virtual OpenDialogEvent OnOpenDialog
		{
			set { this.onOpenDialog = value; }
			get
			{
				//ダイアログイベントに登録がないなら、SystemUiのダイアログを使う
				if (this.onOpenDialog.GetPersistentEventCount() == 0)
				{
					if (SystemUi.GetInstance() != null)
					{
						onOpenDialog.RemoveAllListeners();
						onOpenDialog.AddListener(SystemUi.GetInstance().OpenDialog);
					}
				}

				return onOpenDialog;
			}
		}

		[SerializeField] protected OpenDialogEvent onOpenDialog;

		protected enum State
		{
			Start,
			Downloding,
			DownlodFinished,
		};

		protected virtual State CurrentState { get; set; }

		protected enum Type
		{
			Default,
			Boot,
			ChapterDownload,
		};

		protected virtual Type DownloadType { get; set; }

		//すでにキャッシュファイルからロードしようとした
		//二回目からはダイアログで確認
		protected virtual bool AreadyTryReadCache { get; set; }


		//起動時に開く
		public virtual void OpenOnBoot()
		{
			DownloadType = Type.Boot;
			PrepareBootLoadingVisuals();
			this.Open();
		}

		public virtual bool ShouldOpenOnBootAutomatically()
		{
			AdvEngineStarter target = Starter;
			if (target == null) return false;
			return target.Strage != AdvEngineStarter.StrageType.Local;
		}

		public virtual void PrepareBootLoadingVisuals()
		{
			EnsureCodexLoadingVisuals();
			transform.SetAsLastSibling();
			ApplyCodexProgress(0.02f);
			SetCodexLoadingText("资源更新准备中", "正在准备资源清单...");
			if (codexLoadingRoot != null) codexLoadingRoot.gameObject.SetActive(true);
		}

		//章データのロードとして開く
		public virtual void OpenOnChapter()
		{
			DownloadType = Type.ChapterDownload;
			this.Open();
		}

		protected virtual void OnClose()
		{
			if (codexLoadingCoroutine != null)
			{
				StopCoroutine(codexLoadingCoroutine);
				codexLoadingCoroutine = null;
			}
			codexWaitingForRecovery = false;
			DownloadType = Type.Default;
		}

		protected virtual void OnOpen()
		{
			EnsureCodexLoadingVisuals();
			transform.SetAsLastSibling();
			if (codexLoadingRoot != null)
			{
				codexLoadingRoot.gameObject.SetActive(true);
				codexLoadingRoot.SetAsLastSibling();
			}

			switch (DownloadType)
			{
				case Type.Boot:
					if (this.buttonBack) this.buttonBack.SetActive(false);
					if (this.buttonSkip) this.buttonSkip.SetActive(false);
					if (this.buttonDownload) this.buttonDownload.SetActive(false);
					break;
				case Type.Default:
					if (this.buttonBack) this.buttonBack.SetActive(true);
					if (this.buttonSkip) this.buttonSkip.SetActive(false);
					if (this.buttonDownload) this.buttonDownload.SetActive(false);
					break;
				case Type.ChapterDownload:
					if (this.buttonBack) this.buttonBack.SetActive(false);
					if (this.buttonSkip) this.buttonSkip.SetActive(false);
					if (this.buttonDownload) this.buttonDownload.SetActive(false);
					break;
			}

			AdvEngineStarter target = Starter;
			if (target == null || !target.IsLoadStart)
			{
				ChangeState(State.Start);
			}
			else
			{
				ChangeState(State.Downloding);
			}
		}

		protected virtual void ChangeState(State state)
		{
			this.CurrentState = state;
			switch (state)
			{
				case State.Start:
					if (buttonDownload) buttonDownload.SetActive(false);
					if (loadingBarRoot) loadingBarRoot.SetActive(true);
					ApplyCodexProgress(0.02f);
					SetCodexLoadingText("资源更新准备中", "正在连接资源服务器...");
					StartLoadEngine();
					break;
				case State.Downloding:
					if (buttonDownload) buttonDownload.SetActive(false);
					if (loadingBarRoot) loadingBarRoot.SetActive(true);
					if (codexLoadingCoroutine != null)
					{
						StopCoroutine(codexLoadingCoroutine);
					}
					codexLoadingCoroutine = StartCoroutine(CoUpdateLoading());
					break;
				case State.DownlodFinished:
					OnFinished();
					break;
			}
		}

		protected virtual void OnFinished()
		{
			switch (DownloadType)
			{
				case Type.Boot:
					this.Close();
					if (title != null) title.Open();
					break;
				case Type.Default:
					if (buttonDownload) buttonDownload.SetActive(false);
					if (loadingBarRoot) loadingBarRoot.SetActive(false);
					SetTextMain("资源更新完成");
					SetTextCount("");
					break;
				case Type.ChapterDownload:
					this.Close();
					break;
			}
		}

		//スキップボタン
		public virtual void OnTapSkip()
		{
			this.Close();
			if (title != null) title.Open();
		}

		//ｷｬｯｼｭｸﾘｱして最初のシーンを起動
		public virtual void OnTapReDownload()
		{
			AssetFileManager.GetInstance().AssetBundleInfoManager.DeleteAllCache();
			if (string.IsNullOrEmpty(bootSceneName))
			{
				WrapperUnityVersion.LoadScene(0);
			}
			else
			{
				WrapperUnityVersion.LoadScene(bootSceneName);
			}
		}

		//ローディング中の表示
		protected virtual IEnumerator CoUpdateLoading()
		{
			codexMaxDownloadCount = 0;
			float openedTime = Time.unscaledTime;
			if (loadingBarRoot) loadingBarRoot.SetActive(true);
			ApplyCodexProgress(Mathf.Max(codexDisplayedProgress, 0.02f));
			SetCodexLoadingText("资源更新中", "正在准备资源清单...");

			while (true)
			{
				if (Starter != null && Starter.IsLoadErrorOnAwake)
				{
					Starter.IsLoadErrorOnAwake = false;
					OnFailedLoadEngine();
				}

				AssetBundleInfoManager manifest = AssetFileManager.GetInstance().AssetBundleInfoManager;
				bool isManifestLoading = manifest != null && manifest.IsLoadingManifest;
				bool isBootLoading = Engine != null && Engine.IsWaitBootLoading;
				int countDownLoading = AssetFileManager.CountDownloading();
				bool isAssetQueueLoading = countDownLoading > 0 || !AssetFileManager.IsDownloadEnd();
				bool isStarterLoading = IsCodexStarterLoading();
				bool isMinimumVisible = Time.unscaledTime - openedTime < CodexMinimumVisibleTime;

				if (codexWaitingForRecovery)
				{
					SetCodexLoadingText("资源更新失败", "请在弹窗中选择读取缓存或重试");
				}
				else
				{
					UpdateCodexLoadingProgress(manifest, isManifestLoading, isBootLoading, isAssetQueueLoading, countDownLoading);
				}

				if (!isManifestLoading && !isBootLoading && !isAssetQueueLoading && !isStarterLoading && !codexWaitingForRecovery && !isMinimumVisible)
				{
					break;
				}

				yield return null;
			}

			ApplyCodexProgress(1.0f);
			SetCodexLoadingText("资源更新完成", "100%");
			yield return new WaitForSecondsRealtime(0.15f);
			if (loadingBarRoot) loadingBarRoot.gameObject.SetActive(false);
			codexLoadingCoroutine = null;
			ChangeState(State.DownlodFinished);
		}


		//ロード開始
		protected virtual void StartLoadEngine()
		{
			AdvEngineStarter target = Starter;
			if (target == null)
			{
				OnFailedLoadEngine();
				return;
			}

			if (!target.IsLoadStart)
			{
				StartTrackedLoadEngine(target.LoadEngineAsync(OnFailedLoadEngine));
			}
			ChangeState(State.Downloding);
		}

		protected virtual void StartTrackedLoadEngine(IEnumerator loadRoutine)
		{
			if (loadRoutine == null) return;
			codexWaitingForRecovery = false;
			StartCoroutine(CoStartLoadEngine(loadRoutine));
		}

		protected virtual IEnumerator CoStartLoadEngine(IEnumerator loadRoutine)
		{
			++codexActiveStarterLoadCount;
			yield return loadRoutine;
			codexActiveStarterLoadCount = Mathf.Max(0, codexActiveStarterLoadCount - 1);
		}

		protected virtual bool IsCodexStarterLoading()
		{
			return codexActiveStarterLoadCount > 0;
		}

		protected virtual void UpdateCodexLoadingProgress(
			AssetBundleInfoManager manifest,
			bool isManifestLoading,
			bool isBootLoading,
			bool isAssetQueueLoading,
			int countDownLoading)
		{
			float targetProgress = codexDisplayedProgress;
			string mainText = "资源更新中";
			string countText = "";

			if (isManifestLoading)
			{
				float manifestProgress = manifest != null ? manifest.ManifestProgress : 0;
				targetProgress = Mathf.Lerp(0.04f, 0.35f, manifestProgress);
				mainText = "正在校验资源清单";
				countText = FormatPercent(targetProgress);
			}
			else if (isAssetQueueLoading)
			{
				codexMaxDownloadCount = Mathf.Max(codexMaxDownloadCount, countDownLoading);
				int countDownLoaded = Mathf.Max(0, codexMaxDownloadCount - countDownLoading);
				float assetProgress = codexMaxDownloadCount > 0
					? 1.0f * countDownLoaded / codexMaxDownloadCount
					: Mathf.PingPong(Time.unscaledTime * 0.2f, 0.2f);
				targetProgress = Mathf.Lerp(0.35f, 0.92f, assetProgress);
				mainText = countDownLoading > 0 ? "正在缓存游戏资源" : "正在整理资源缓存";
				countText = codexMaxDownloadCount > 0
					? string.Format("已完成 {0}/{1}  {2}", countDownLoaded, codexMaxDownloadCount, FormatPercent(targetProgress))
					: FormatPercent(targetProgress);
			}
			else if (isBootLoading)
			{
				targetProgress = Mathf.Lerp(0.65f, 0.96f, Mathf.PingPong(Time.unscaledTime * 0.12f, 0.75f));
				mainText = "正在初始化游戏";
				countText = FormatPercent(targetProgress);
			}
			else if (IsCodexStarterLoading())
			{
				targetProgress = Mathf.Lerp(0.08f, 0.55f, Mathf.PingPong(Time.unscaledTime * 0.15f, 0.85f));
				mainText = "正在连接资源服务器";
				countText = FormatPercent(targetProgress);
			}
			else
			{
				targetProgress = 0.98f;
				mainText = "正在进入游戏";
				countText = FormatPercent(targetProgress);
			}

			codexDisplayedProgress = Mathf.MoveTowards(
				codexDisplayedProgress,
				Mathf.Clamp01(Mathf.Max(codexDisplayedProgress, targetProgress)),
				Time.unscaledDeltaTime * 0.5f);
			ApplyCodexProgress(codexDisplayedProgress);
			SetCodexLoadingText(mainText, countText);
		}

		protected virtual string FormatPercent(float progress)
		{
			return string.Format("{0:0}%", Mathf.Clamp01(progress) * 100f);
		}

		protected virtual void SetCodexLoadingText(string mainText, string countText)
		{
			SetTextMain(mainText);
			SetTextCount(countText);
		}

		protected virtual void ApplyCodexProgress(float progress)
		{
			codexDisplayedProgress = Mathf.Clamp01(progress);
			if (loadingBar != null)
			{
				loadingBar.fillAmount = codexDisplayedProgress;
			}
			if (codexProgressGlow != null)
			{
				RectTransform rectTransform = codexProgressGlow.rectTransform;
				rectTransform.anchorMin = new Vector2(codexDisplayedProgress, 0.5f);
				rectTransform.anchorMax = new Vector2(codexDisplayedProgress, 0.5f);
				rectTransform.anchoredPosition = Vector2.zero;
			}
		}

		protected virtual void EnsureCodexLoadingVisuals()
		{
			if (codexLoadingRoot != null)
			{
				return;
			}

			GameObject oldLoadingBarRoot = loadingBarRoot;
			Text oldTextMain = textMain;
			Text oldTextCount = textCount;
			TextMeshProUGUI oldTextMainTmp = textMainTmp;
			TextMeshProUGUI oldTextCountTmp = textCountTmp;
			Font font = ResolveLoadingFont(oldTextMain, oldTextCount);

			codexLoadingRoot = CreateRectChild(transform, CodexLoadingRootName);
			StretchRect(codexLoadingRoot);
			codexLoadingRoot.SetAsLastSibling();
			CanvasGroup canvasGroup = codexLoadingRoot.gameObject.GetComponent<CanvasGroup>();
			if (canvasGroup == null) canvasGroup = codexLoadingRoot.gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 1;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = true;

			CreateLoadingBackground(codexLoadingRoot);

			Image veil = CreateImageChild(codexLoadingRoot, "Veil");
			StretchRect(veil.rectTransform);
			veil.color = new Color(0.02f, 0.018f, 0.017f, 0.72f);
			veil.raycastTarget = true;

			Image logo = CreateImageChild(codexLoadingRoot, "Logo");
			Sprite logoSprite = FindTitleLogoSprite();
			logo.sprite = logoSprite;
			logo.enabled = logoSprite != null;
			logo.preserveAspect = true;
			logo.color = new Color(1f, 1f, 1f, 0.94f);
			SetRectAnchored(logo.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(720f, 220f), Vector2.zero);

			Image band = CreateImageChild(codexLoadingRoot, "BottomBand");
			RectTransform bandRect = band.rectTransform;
			bandRect.anchorMin = new Vector2(0f, 0f);
			bandRect.anchorMax = new Vector2(1f, 0f);
			bandRect.pivot = new Vector2(0.5f, 0f);
			bandRect.anchoredPosition = Vector2.zero;
			bandRect.sizeDelta = new Vector2(0f, 286f);
			band.color = new Color(0.03f, 0.025f, 0.024f, 0.84f);
			band.raycastTarget = false;

			codexMainText = CreateTextChild(bandRect, CodexLoadingMainTextName, font, 28, FontStyle.Normal);
			SetRectAnchored(codexMainText.rectTransform, new Vector2(0.5f, 0f), new Vector2(840f, 48f), new Vector2(0f, 178f));
			codexMainText.alignment = TextAnchor.MiddleCenter;
			codexMainText.color = new Color(0.96f, 0.92f, 0.88f, 1f);

			codexCountText = CreateTextChild(bandRect, CodexLoadingCountTextName, font, 18, FontStyle.Normal);
			SetRectAnchored(codexCountText.rectTransform, new Vector2(0.5f, 0f), new Vector2(840f, 34f), new Vector2(0f, 88f));
			codexCountText.alignment = TextAnchor.MiddleCenter;
			codexCountText.color = new Color(0.78f, 0.72f, 0.68f, 1f);

			Image track = CreateImageChild(bandRect, CodexProgressRootName);
			loadingBarRoot = track.gameObject;
			SetRectAnchored(track.rectTransform, new Vector2(0.5f, 0f), new Vector2(760f, 16f), new Vector2(0f, 132f));
			track.sprite = GetDefaultUiSprite();
			track.type = Image.Type.Sliced;
			track.color = new Color(0f, 0f, 0f, 0.62f);
			track.raycastTarget = false;

			Image fill = CreateImageChild(track.rectTransform, "Fill");
			loadingBar = fill;
			StretchRect(fill.rectTransform, 2f);
			fill.sprite = GetDefaultUiSprite();
			fill.type = Image.Type.Filled;
			fill.fillMethod = Image.FillMethod.Horizontal;
			fill.fillOrigin = 0;
			fill.fillAmount = 0;
			fill.color = new Color(0.72f, 0.16f, 0.18f, 1f);
			fill.raycastTarget = false;

			codexProgressGlow = CreateImageChild(track.rectTransform, "Glow");
			codexProgressGlow.sprite = GetDefaultUiSprite();
			codexProgressGlow.color = new Color(1f, 0.82f, 0.58f, 0.7f);
			codexProgressGlow.raycastTarget = false;
			SetRectAnchored(codexProgressGlow.rectTransform, new Vector2(0f, 0.5f), new Vector2(18f, 28f), Vector2.zero);

			textMain = codexMainText;
			textCount = codexCountText;
			textMainTmp = null;
			textCountTmp = null;

			HideLegacyLoadingArtifact(oldLoadingBarRoot);
			HideLegacyText(oldTextMain, codexMainText);
			HideLegacyText(oldTextCount, codexCountText);
			HideLegacyText(oldTextMainTmp, null);
			HideLegacyText(oldTextCountTmp, null);
		}

		protected virtual void CreateLoadingBackground(RectTransform parent)
		{
			Image image = CreateImageChild(parent, "BackgroundImage");
			StretchRect(image.rectTransform);
			Sprite backgroundSprite = FindTitleBackgroundSprite();
			image.sprite = backgroundSprite;
			image.enabled = backgroundSprite != null;
			image.preserveAspect = true;
			image.color = new Color(1f, 1f, 1f, 0.42f);
			image.raycastTarget = false;
			if (backgroundSprite != null)
			{
				ApplyEnvelopeAspect(image.rectTransform, GetSpriteAspect(backgroundSprite));
			}

			RawImage rawImage = CreateRawImageChild(parent, "BackgroundRaw");
			StretchRect(rawImage.rectTransform);
			Texture backgroundTexture = backgroundSprite == null ? FindTitleBackgroundTexture() : null;
			rawImage.texture = backgroundTexture;
			rawImage.enabled = backgroundTexture != null;
			rawImage.color = new Color(1f, 1f, 1f, 0.42f);
			rawImage.raycastTarget = false;
			if (backgroundTexture != null)
			{
				ApplyEnvelopeAspect(rawImage.rectTransform, GetTextureAspect(backgroundTexture));
			}

			if (!image.enabled && !rawImage.enabled)
			{
				Image fallback = CreateImageChild(parent, "BackgroundFallback");
				StretchRect(fallback.rectTransform);
				fallback.color = new Color(0.07f, 0.065f, 0.06f, 1f);
				fallback.raycastTarget = false;
			}
		}

		protected virtual Font ResolveLoadingFont(Text oldTextMain, Text oldTextCount)
		{
			if (oldTextMain != null && oldTextMain.font != null) return oldTextMain.font;
			if (oldTextCount != null && oldTextCount.font != null) return oldTextCount.font;
			return Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		protected virtual RectTransform CreateRectChild(Transform parent, string objectName)
		{
			Transform child = parent.Find(objectName);
			if (child != null)
			{
				RectTransform childRect = child as RectTransform;
				if (childRect != null) return childRect;
			}

			GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.SetParent(parent, false);
			return rectTransform;
		}

		protected virtual Image CreateImageChild(Transform parent, string objectName)
		{
			RectTransform rectTransform = CreateRectChild(parent, objectName);
			Image image = rectTransform.GetComponent<Image>();
			if (image == null) image = rectTransform.gameObject.AddComponent<Image>();
			return image;
		}

		protected virtual RawImage CreateRawImageChild(Transform parent, string objectName)
		{
			RectTransform rectTransform = CreateRectChild(parent, objectName);
			RawImage rawImage = rectTransform.GetComponent<RawImage>();
			if (rawImage == null) rawImage = rectTransform.gameObject.AddComponent<RawImage>();
			return rawImage;
		}

		protected virtual Text CreateTextChild(Transform parent, string objectName, Font font, int fontSize, FontStyle fontStyle)
		{
			RectTransform rectTransform = CreateRectChild(parent, objectName);
			Text text = rectTransform.GetComponent<Text>();
			if (text == null) text = rectTransform.gameObject.AddComponent<Text>();
			text.font = font;
			text.fontSize = fontSize;
			text.fontStyle = fontStyle;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 12;
			text.resizeTextMaxSize = fontSize;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Truncate;
			text.raycastTarget = false;
			return text;
		}

		protected virtual void StretchRect(RectTransform rectTransform, float inset = 0f)
		{
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(-inset * 2f, -inset * 2f);
		}

		protected virtual void SetRectAnchored(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
		{
			rectTransform.anchorMin = anchor;
			rectTransform.anchorMax = anchor;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.sizeDelta = size;
			rectTransform.anchoredPosition = anchoredPosition;
		}

		protected virtual void ApplyEnvelopeAspect(RectTransform rectTransform, float aspectRatio)
		{
			if (rectTransform == null || aspectRatio <= 0f) return;
			AspectRatioFitter aspectFitter = rectTransform.GetComponent<AspectRatioFitter>();
			if (aspectFitter == null) aspectFitter = rectTransform.gameObject.AddComponent<AspectRatioFitter>();
			aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
			aspectFitter.aspectRatio = aspectRatio;
		}

		protected virtual float GetSpriteAspect(Sprite sprite)
		{
			return sprite != null && sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 0f;
		}

		protected virtual float GetTextureAspect(Texture texture)
		{
			return texture != null && texture.height > 0 ? 1.0f * texture.width / texture.height : 0f;
		}

		protected virtual Sprite GetDefaultUiSprite()
		{
			return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
		}

		protected virtual Sprite FindTitleBackgroundSprite()
		{
			if (title == null) return null;

			Image best = null;
			int bestScore = int.MinValue;
			foreach (Image image in title.GetComponentsInChildren<Image>(true))
			{
				if (image == null || image.sprite == null || image.sprite.texture == null) continue;
				int score = GetTitleGraphicScore(image, true);
				if (score > bestScore)
				{
					best = image;
					bestScore = score;
				}
			}
			return best != null ? best.sprite : null;
		}

		protected virtual Texture FindTitleBackgroundTexture()
		{
			if (title == null) return null;

			RawImage best = null;
			int bestScore = int.MinValue;
			foreach (RawImage rawImage in title.GetComponentsInChildren<RawImage>(true))
			{
				if (rawImage == null || rawImage.texture == null) continue;
				int score = GetTitleGraphicScore(rawImage, true);
				if (score > bestScore)
				{
					best = rawImage;
					bestScore = score;
				}
			}
			return best != null ? best.texture : null;
		}

		protected virtual Sprite FindTitleLogoSprite()
		{
			if (title == null) return null;

			Image best = null;
			int bestScore = int.MinValue;
			foreach (Image image in title.GetComponentsInChildren<Image>(true))
			{
				if (image == null || image.sprite == null || image.sprite.texture == null) continue;
				int score = GetTitleGraphicScore(image, false);
				if (score > bestScore)
				{
					best = image;
					bestScore = score;
				}
			}
			return best != null && bestScore > 0 ? best.sprite : null;
		}

		protected virtual int GetTitleGraphicScore(Graphic graphic, bool background)
		{
			Texture texture = null;
			string names = graphic.name ?? "";
			Image image = graphic as Image;
			if (image != null && image.sprite != null)
			{
				texture = image.sprite.texture;
				names += " " + image.sprite.name;
				if (image.sprite.texture != null) names += " " + image.sprite.texture.name;
			}
			RawImage rawImage = graphic as RawImage;
			if (rawImage != null && rawImage.texture != null)
			{
				texture = rawImage.texture;
				names += " " + rawImage.texture.name;
			}

			int area = texture != null ? Mathf.Clamp(texture.width * texture.height / 1024, 0, 2000000) : 0;
			int score = area;
			bool isTitleRoot = title != null && graphic.transform == title.transform;
			bool isCover = NameContains(names, "cover") || NameContains(names, "封面");
			bool isTitleBackground = NameContains(names, "title_bg") || NameContains(names, "titlebg");
			bool isLogo = NameContains(names, "logo") || NameContains(names, "title_logo");
			bool isButton = NameContains(names, "button") || NameContains(names, "btn") || NameContains(names, "lock");
			if (isTitleRoot) score += background ? 1200000 : -1000000;
			if (isCover || isTitleBackground) score += background ? 1500000 : -500000;
			if (NameContains(names, "bg") || NameContains(names, "background")) score += background ? 1000000 : -250000;
			if (NameContains(names, "spine")) score += background ? 500000 : -250000;
			if (isLogo) score += background ? -500000 : 1500000;
			if (NameContains(names, "title") && !isTitleRoot && !isTitleBackground) score += background ? -250000 : 1000000;
			if (isButton) score -= 750000;
			if (NameContains(names, "config") || NameContains(names, "exit")) score -= 750000;
			return score;
		}

		protected virtual bool NameContains(string text, string value)
		{
			return !string.IsNullOrEmpty(text)
				&& text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		protected virtual void HideLegacyLoadingArtifact(GameObject oldRoot)
		{
			if (oldRoot == null) return;
			if (oldRoot == loadingBarRoot) return;
			if (oldRoot.name == CodexProgressRootName) return;
			oldRoot.SetActive(false);
		}

		protected virtual void HideLegacyText(Graphic oldText, Graphic newText)
		{
			if (oldText == null || oldText == newText) return;
			oldText.gameObject.SetActive(false);
		}

		//ロード失敗
		protected virtual void OnFailedLoadEngine()
		{
			SetCodexLoadingText("资源更新失败", "请检查网络连接后重试");
			//キャッシュファイルから起動する
			if (isAutoCacheFileLoad && !AreadyTryReadCache)
			{
				AreadyTryReadCache = true;
				SetCodexLoadingText("资源更新失败", "正在尝试读取本地缓存...");
				if (Starter != null)
				{
					StartTrackedLoadEngine(Starter.LoadEngineAsyncFromCacheManifest(OnFailedLoadEngine));
				}
			}
			else
			{
				codexWaitingForRecovery = true;
				string text = LanguageSystemText.LocalizeText(SystemText.WarningNotOnline);
				List<ButtonEventInfo> buttons = new List<ButtonEventInfo>
				{
					new ButtonEventInfo(
						LanguageSystemText.LocalizeText(SystemText.Yes),
						() =>
						{
							SetCodexLoadingText("正在读取本地缓存", "请稍候...");
							if (Starter != null)
							{
								StartTrackedLoadEngine(Starter.LoadEngineAsyncFromCacheManifest(OnFailedLoadEngine));
							}
						}
					),
					new ButtonEventInfo(
						LanguageSystemText.LocalizeText(SystemText.Retry),
						() =>
						{
							SetCodexLoadingText("正在重新连接资源服务器", "请稍候...");
							if (Starter != null)
							{
								StartTrackedLoadEngine(Starter.LoadEngineAsync(OnFailedLoadEngine));
							}
						}
					),
				};
				OnOpenDialog.Invoke(text, buttons);
			}
		}

		//モバイルでのネットワークがオフラインになっているか
		protected bool IsMobileOffLine()
		{
			switch (Application.internetReachability)
			{
				//ネットにつながらないときに
				//キャッシュファイルがあるならそっちを使う
				case NetworkReachability.NotReachable:
					return true;
				case NetworkReachability.ReachableViaCarrierDataNetwork: //キャリア
				case NetworkReachability.ReachableViaLocalAreaNetwork: //Wifi
				default:
					return false;
			}
		}

		public virtual void SetTextMain(string text)
		{
			TextComponentWrapper.SetText(textMain, textMainTmp, text);
		}

		public virtual void SetTextCount(string text)
		{
			TextComponentWrapper.SetText(textCount, textCountTmp, text);
		}

	}
}
