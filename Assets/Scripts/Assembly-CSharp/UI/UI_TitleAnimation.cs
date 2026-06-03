using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace UI
{
	public class UI_TitleAnimation : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		[SerializeField]
		private AdvEngine engine;

		public float speedUpRate;

		public float timer;

		public Shader shader_bg;

		public Shader shader_title;

		public Shader shader_particle;

		public ParticleSystem particle;

		private ParticleSystemRenderer particleRenderer;

		private Image image_title;

		private Image image_titleShadow;

		private Image image_titleBG;

		private Image image_ChapterBg;

		private Image image_foreground;

		private Image image_filmGrain;

		private Image image_shelter;

		public Transform titleDestination;

		private GameObject animationLinker;

		private float clickTime;

		private int clickCount;

		private CanvasGroup canvasGroup;

		private Coroutine animationCoroutine;

		private bool isClosing;

		private bool hasTitleBaseTransform;

		private Vector2 titleBaseAnchorMin;

		private Vector2 titleBaseAnchorMax;

		private Vector2 titleBasePivot;

		private Vector2 titleBaseAnchoredPosition;

		private Vector2 titleBaseSizeDelta;

		private Vector3 titleBaseScale;

		private bool titleBasePreserveAspect;

		private const float BackgroundMotionPaddingX = 32f;
		private const float BackgroundMotionPaddingY = 16f;
		private const float TitleShadowScale = 1f;
		private const float TitleShadowAlpha = 0f;
		private const float TitleShadowOffsetX = 0f;
		private const float TitleShadowOffsetY = 0f;
		private const float TitleOffsetReferenceX = 265f;
		private const float TimelineInitialBlackDuration = 0.4f;
		private const float TimelineRevealDuration = 0.21f;
		private const float TimelineStableDuration = 2.01f;
		private const float TimelineParticleDuration = 1.9f;
		private const float TimelineParticleSettleDuration = 0.65f;
		private const float TimelineFadeToBlackDuration = 1.05f;
		private const float TimelineBlackHoldDuration = 1.88f;
		private const float TimelineReturnFadeDuration = 0.41f;
		private const float TimelineIntroMaxDelta = 1f / 30f;
		private const float TimelineMaxDelta = 0.2f;
		private const float TitleIntroSlideDuration = 0.72f;
		private const float TitleIntroOffscreenPadding = 80f;
		private const float ParticleBurstRampProgress = 0.58f;
		private const float TitleDissolveStartParticleProgress = 0.62f;
		private const float TitleDissolveParticlePhaseWeight = 0.4f;
		private const float TitleDissolveSettlePhaseWeight = 0.82f;
		private const float TitleDissolveEndFadeToBlackProgress = 0.22f;
		private const int FilmGrainTextureSize = 128;
		private const float FilmGrainPixelsPerUnit = 100f;
		private const float FilmGrainRefreshInterval = 1f / 18f;
		private const float FilmGrainBaseAlpha = 0.36f;
		private const float FilmGrainBurstAlpha = 0.22f;

		private static readonly int ShaderStrengthId = Shader.PropertyToID("_Strength");
		private static readonly int ShaderSpeedId = Shader.PropertyToID("_Speed");
		private static readonly int ShaderWaveId = Shader.PropertyToID("_Wave");
		private static readonly int ShaderScaleId = Shader.PropertyToID("_Scale");

		private Material rippleMaterialInstance;

		private Material titleMaterialInstance;

		private Material particleMaterialInstance;

		private float particleBaseEmissionRate;

		private float particleBaseStartSpeed;

		private float particleBaseLifetime;

		private Color particleBaseMinColor;

		private Color particleBaseMaxColor;

		private Texture2D filmGrainTexture;

		private Sprite filmGrainSprite;

		private Color32[] filmGrainPixels;

		private float nextFilmGrainRefreshTime;

		private int filmGrainSeed;

		private bool isPlaying;

		public bool IsPlaying => isPlaying;

		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);

		private void Awake()
		{
			Init();
		}

		private void Init()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
			DisableUnexpectedRootLayoutGroups();

			image_ChapterBg = image_ChapterBg != null ? image_ChapterBg : FindImage("BG");
			image_titleBG = image_titleBG != null ? image_titleBG : FindImage("TitleBG");
			image_title = image_title != null ? image_title : FindImage("Title");
			EnsureTitleShadowImage();
			image_foreground = image_foreground != null ? image_foreground : FindImage("Fg");
			image_filmGrain = image_filmGrain != null ? image_filmGrain : FindImage("RuntimeFilmGrain");
			image_shelter = image_shelter != null ? image_shelter : FindImage("Shelter");
			if (particle == null) particle = GetComponentInChildren<ParticleSystem>(true);
			NormalizeParticleParent();

			if (image_ChapterBg != null) image_ChapterBg.raycastTarget = false;
			if (image_titleBG != null) image_titleBG.raycastTarget = false;
			if (image_title != null) image_title.raycastTarget = false;
			if (image_titleShadow != null) image_titleShadow.raycastTarget = false;
			if (image_foreground != null) image_foreground.raycastTarget = false;
			if (image_filmGrain != null) image_filmGrain.raycastTarget = false;
			if (image_shelter != null)
			{
				image_shelter.raycastTarget = true;
				EnsureSolidImage(image_shelter);
			}
			if (particleRenderer == null && particle != null) particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
			EnsureVisualLayerOrder();
			CacheTitleBaseTransform();
		}

		private void NormalizeParticleParent()
		{
			// The prefab positions the emitter under Title; that authored parent controls the burst origin.
		}

		private void DisableUnexpectedRootLayoutGroups()
		{
			LayoutGroup[] layoutGroups = GetComponents<LayoutGroup>();
			foreach (LayoutGroup layoutGroup in layoutGroups)
			{
				if (layoutGroup != null && layoutGroup.enabled)
				{
					layoutGroup.enabled = false;
				}
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			++clickCount;
			Close();
		}

		private void OnDestroy()
		{
			isPlaying = false;
			if (animationCoroutine != null)
			{
				StopCoroutine(animationCoroutine);
			}
			if (animationLinker != null)
			{
				DestroyObjectSafe(animationLinker);
			}
			if (rippleMaterialInstance != null)
			{
				DestroyObjectSafe(rippleMaterialInstance);
			}
			if (titleMaterialInstance != null)
			{
				DestroyObjectSafe(titleMaterialInstance);
			}
			if (particleMaterialInstance != null)
			{
				DestroyObjectSafe(particleMaterialInstance);
			}
			if (filmGrainSprite != null)
			{
				DestroyObjectSafe(filmGrainSprite);
			}
			if (filmGrainTexture != null)
			{
				DestroyObjectSafe(filmGrainTexture);
			}
		}

		public void SetInfo(Sprite titleBg, Sprite chapterBg, Sprite titleText, TitleAnimationType type, bool isSpeedUp, int bgOffset, float scale)
		{
			Init();
			isClosing = false;
			gameObject.SetActive(true);
			StretchToParent(transform as RectTransform);

			SetImage(image_ChapterBg, titleBg, false);
			SetImage(image_titleBG, chapterBg, false);
			SetImage(image_title, titleText, titleBasePreserveAspect);
			SyncTitleShadowSprite();
			EnsureVisualLayerOrder();

			float fixedScale = scale <= 0 ? 1f : scale;
			FitImageToParentCover(image_ChapterBg, Vector2.zero, 1f, new Vector2(BackgroundMotionPaddingX, BackgroundMotionPaddingY));
			StretchImageToParent(image_titleBG);
			ApplyTitleOffsetAndScale(image_title, bgOffset, fixedScale);
			ApplyTitleShadowOffsetAndScale();
			PrepareRippleLayer();
			PrepareFilmGrainLayer();
			PrepareTitleDisappearLayer();
			PrepareParticleLayer();
			EnsureVisualLayerOrder();
			ApplyTypeVisualProfile(type);
			isPlaying = true;
			timer = 0f;
			filmGrainSeed = 0;
			nextFilmGrainRefreshTime = 0f;

			if (canvasGroup != null)
			{
				canvasGroup.alpha = 1;
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
			if (image_shelter != null)
			{
				image_shelter.color = Color.black;
				image_shelter.material = null;
			}
			if (particle != null)
			{
				ResetParticlePlayback();
			}

			if (animationCoroutine != null)
			{
				StopCoroutine(animationCoroutine);
			}
			animationCoroutine = StartCoroutine(CoPlay(isSpeedUp));
		}

		private IEnumerator CoPlay(bool isSpeedUp)
		{
			float rate = isSpeedUp && speedUpRate > 0 ? speedUpRate : 1f;
			float tempo = Mathf.Max(0.25f, rate);
			float initialBlackDuration = TimelineInitialBlackDuration / tempo;
			float revealDuration = TimelineRevealDuration / tempo;
			float stableDuration = TimelineStableDuration / tempo;
			float particleDuration = TimelineParticleDuration / tempo;
			float particleSettleDuration = TimelineParticleSettleDuration / tempo;
			float fadeToBlackDuration = TimelineFadeToBlackDuration / tempo;
			float blackHoldDuration = TimelineBlackHoldDuration / tempo;
			float returnFadeDuration = TimelineReturnFadeDuration / tempo;
			float totalDuration =
				initialBlackDuration +
				revealDuration +
				stableDuration +
				particleDuration +
				particleSettleDuration +
				fadeToBlackDuration +
				blackHoldDuration +
				returnFadeDuration;

			RectTransform bgRect = image_ChapterBg != null ? image_ChapterBg.rectTransform : null;
			RectTransform titleBgRect = image_titleBG != null ? image_titleBG.rectTransform : null;
			RectTransform fgRect = image_foreground != null ? image_foreground.rectTransform : null;
			RectTransform titleRect = image_title != null ? image_title.rectTransform : null;
			Vector2 startPosition = bgRect != null ? bgRect.anchoredPosition : Vector2.zero;
			Vector2 titleBgStartPosition = titleBgRect != null ? titleBgRect.anchoredPosition : Vector2.zero;
			Vector2 fgStartPosition = fgRect != null ? fgRect.anchoredPosition : Vector2.zero;
			Vector3 fgStartScale = fgRect != null ? fgRect.localScale : Vector3.one;
			Vector2 titleStartPosition = titleRect != null ? titleRect.anchoredPosition : Vector2.zero;
			Vector3 titleStartScale = titleRect != null ? titleRect.localScale : Vector3.one;
			Vector2 titleOffscreenStartPosition = titleRect != null
				? GetTitleOffscreenStartPosition(titleRect, titleStartPosition, titleStartScale)
				: Vector2.zero;
			TitleAnimationType visualType = currentType;
			ApplyAnimatedVisuals(
				visualType,
				0f,
				initialBlackDuration,
				revealDuration,
				stableDuration,
				particleDuration,
				particleSettleDuration,
				fadeToBlackDuration,
				blackHoldDuration,
				returnFadeDuration,
				bgRect,
				startPosition,
				titleBgRect,
				titleBgStartPosition,
				fgRect,
				fgStartPosition,
				fgStartScale,
				titleRect,
				titleOffscreenStartPosition,
				titleStartPosition,
				titleStartScale);
			float elapsed = 0f;
			while (elapsed < totalDuration && !isClosing)
			{
				timer = elapsed;
				ApplyAnimatedVisuals(
					visualType,
					elapsed,
					initialBlackDuration,
					revealDuration,
					stableDuration,
					particleDuration,
					particleSettleDuration,
					fadeToBlackDuration,
					blackHoldDuration,
					returnFadeDuration,
					bgRect,
					startPosition,
					titleBgRect,
					titleBgStartPosition,
					fgRect,
					fgStartPosition,
					fgStartScale,
					titleRect,
					titleOffscreenStartPosition,
					titleStartPosition,
					titleStartScale);
				yield return null;
				float maxDelta = elapsed < initialBlackDuration + revealDuration ? TimelineIntroMaxDelta : TimelineMaxDelta;
				elapsed += Mathf.Min(Time.unscaledDeltaTime, maxDelta);
			}

			if (!isClosing)
			{
				timer = totalDuration;
				ApplyAnimatedVisuals(
					visualType,
					totalDuration,
					initialBlackDuration,
					revealDuration,
					stableDuration,
					particleDuration,
					particleSettleDuration,
					fadeToBlackDuration,
					blackHoldDuration,
					returnFadeDuration,
					bgRect,
					startPosition,
					titleBgRect,
					titleBgStartPosition,
					fgRect,
					fgStartPosition,
					fgStartScale,
					titleRect,
					titleOffscreenStartPosition,
					titleStartPosition,
					titleStartScale);
				SetShelterAlpha(0f);
				DestroySelf();
			}
		}

		private IEnumerator FadeShelter(float from, float to, float duration)
		{
			if (image_shelter == null) yield break;

			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				Color color = image_shelter.color;
				color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
				image_shelter.color = color;
				yield return null;
			}
			Color finalColor = image_shelter.color;
			finalColor.a = to;
			image_shelter.color = finalColor;
		}

		private IEnumerator FadeCanvas(float from, float to, float duration)
		{
			if (canvasGroup == null) yield break;

			if (to <= 0f)
			{
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
				yield return null;
			}
			canvasGroup.alpha = to;
		}

		private void Close()
		{
			if (isClosing) return;
			isClosing = true;
			if (animationCoroutine != null)
			{
				StopCoroutine(animationCoroutine);
			}
			StartCoroutine(CoClose());
		}

		private IEnumerator CoClose()
		{
			SetShelterAlpha(0f);
			yield return FadeCanvas(canvasGroup != null ? canvasGroup.alpha : 1, 0, 0.14f);
			DestroySelf();
		}

		private Image FindImage(string targetName)
		{
			Transform target = FindChildRecursive(transform, targetName);
			return target != null ? target.GetComponent<Image>() : null;
		}

		private void SetImage(Image image, Sprite sprite, bool preserveAspect)
		{
			if (image == null) return;
			if (sprite != null)
			{
				image.sprite = sprite;
			}
			image.enabled = image.sprite != null;
			image.preserveAspect = preserveAspect;
			image.color = Color.white;
		}

		private void EnsureSolidImage(Image image)
		{
			if (image == null || image.sprite != null) return;

			Texture2D texture = Texture2D.whiteTexture;
			image.sprite = Sprite.Create(
				texture,
				new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f),
				100f);
			image.type = Image.Type.Simple;
			image.enabled = image.sprite != null;
		}

		private void ApplyTitleOffsetAndScale(Image image, int bgOffset, float scale)
		{
			if (image == null) return;

			RectTransform rectTransform = image.rectTransform;
			RestoreTitleBaseTransform(rectTransform);
			image.preserveAspect = titleBasePreserveAspect;
			float normalizedOffset = bgOffset == 0 ? 0f : bgOffset - TitleOffsetReferenceX;
			rectTransform.anchoredPosition = titleBaseAnchoredPosition + new Vector2(normalizedOffset, 0f);
			rectTransform.localScale = Vector3.Scale(titleBaseScale, Vector3.one * scale);
		}

		private void StretchImageToParent(Image image)
		{
			if (image == null) return;

			StretchToParent(image.rectTransform);
			image.preserveAspect = false;
		}

		private void FitImageToParentCover(Image image, Vector2 anchoredPosition, float scale, Vector2 padding)
		{
			if (image == null || image.sprite == null) return;

			RectTransform rectTransform = image.rectTransform;
			RectTransform parent = rectTransform.parent as RectTransform;
			if (parent == null) return;

			Vector2 parentSize = ResolveRectSize(parent);
			if (parentSize.x <= 0 || parentSize.y <= 0) return;

			float spriteWidth = image.sprite.rect.width;
			float spriteHeight = image.sprite.rect.height;
			if (spriteWidth <= 0 || spriteHeight <= 0) return;

			float spriteAspect = spriteWidth / spriteHeight;
			float parentAspect = parentSize.x / parentSize.y;
			Vector2 coverSize = parentAspect > spriteAspect
				? new Vector2(parentSize.x, parentSize.x / spriteAspect)
				: new Vector2(parentSize.y * spriteAspect, parentSize.y);

			float safeScale = Mathf.Max(1f, scale);
			Vector2 safePadding = new Vector2(Mathf.Max(0f, padding.x), Mathf.Max(0f, padding.y));
			if (safePadding.x > 0 || safePadding.y > 0)
			{
				float paddedScaleX = (coverSize.x + safePadding.x * 2f) / coverSize.x;
				float paddedScaleY = (coverSize.y + safePadding.y * 2f) / coverSize.y;
				safeScale *= Mathf.Max(paddedScaleX, paddedScaleY);
			}
			coverSize *= safeScale;

			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.sizeDelta = coverSize;
			rectTransform.localScale = Vector3.one;
			image.preserveAspect = false;
		}

		private Vector2 ResolveRectSize(RectTransform rectTransform)
		{
			if (rectTransform == null) return Vector2.zero;

			Vector2 size = rectTransform.rect.size;
			RectTransform parent = rectTransform.parent as RectTransform;
			if (parent != null && rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.one)
			{
				Vector2 parentSize = ResolveRectSize(parent);
				Vector2 stretchedSize = parentSize + rectTransform.offsetMax - rectTransform.offsetMin;
				if (stretchedSize.x > 0 && stretchedSize.y > 0)
				{
					size = stretchedSize;
				}
			}

			return size;
		}

		private void StretchToParent(RectTransform rectTransform)
		{
			if (rectTransform == null) return;

			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
		}

		private void CacheTitleBaseTransform()
		{
			if (hasTitleBaseTransform || image_title == null) return;

			RectTransform rectTransform = image_title.rectTransform;
			titleBaseAnchorMin = rectTransform.anchorMin;
			titleBaseAnchorMax = rectTransform.anchorMax;
			titleBasePivot = rectTransform.pivot;
			titleBaseAnchoredPosition = rectTransform.anchoredPosition;
			titleBaseSizeDelta = rectTransform.sizeDelta;
			titleBaseScale = rectTransform.localScale;
			titleBasePreserveAspect = image_title.preserveAspect;
			hasTitleBaseTransform = true;
		}

		private void RestoreTitleBaseTransform(RectTransform rectTransform)
		{
			if (rectTransform == null) return;
			if (!hasTitleBaseTransform)
			{
				CacheTitleBaseTransform();
			}
			if (!hasTitleBaseTransform) return;

			rectTransform.anchorMin = titleBaseAnchorMin;
			rectTransform.anchorMax = titleBaseAnchorMax;
			rectTransform.pivot = titleBasePivot;
			rectTransform.sizeDelta = titleBaseSizeDelta;
			rectTransform.localScale = titleBaseScale;
		}

		private TitleAnimationType currentType;

		private void PrepareRippleLayer()
		{
			if (image_foreground == null) return;

			if (rippleMaterialInstance == null)
			{
				Material sourceMaterial = image_foreground.material;
				if (sourceMaterial != null)
				{
					rippleMaterialInstance = new Material(sourceMaterial);
					rippleMaterialInstance.name = sourceMaterial.name + " Runtime";
				}
				else if (shader_bg != null)
				{
					rippleMaterialInstance = new Material(shader_bg);
					rippleMaterialInstance.name = shader_bg.name + " Runtime";
				}
			}

			if (rippleMaterialInstance != null)
			{
				image_foreground.material = rippleMaterialInstance;
			}
		}

		private void PrepareFilmGrainLayer()
		{
			if (image_filmGrain == null)
			{
				GameObject grainObject = new GameObject("RuntimeFilmGrain", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				grainObject.transform.SetParent(transform, false);
				image_filmGrain = grainObject.GetComponent<Image>();
			}

			EnsureFilmGrainSprite();
			image_filmGrain.sprite = filmGrainSprite;
			image_filmGrain.type = Image.Type.Tiled;
			image_filmGrain.preserveAspect = false;
			image_filmGrain.raycastTarget = false;
			image_filmGrain.material = null;
			image_filmGrain.color = new Color(1f, 1f, 1f, 0f);
			image_filmGrain.enabled = filmGrainSprite != null;
			StretchToParent(image_filmGrain.rectTransform);
		}

		private void EnsureFilmGrainSprite()
		{
			if (filmGrainSprite != null && filmGrainTexture != null) return;

			filmGrainTexture = new Texture2D(FilmGrainTextureSize, FilmGrainTextureSize, TextureFormat.RGBA32, false, true);
			filmGrainTexture.name = "RuntimeChapterTitleFilmGrain";
			filmGrainTexture.wrapMode = TextureWrapMode.Repeat;
			filmGrainTexture.filterMode = FilterMode.Point;
			filmGrainPixels = new Color32[FilmGrainTextureSize * FilmGrainTextureSize];

			filmGrainSprite = Sprite.Create(
				filmGrainTexture,
				new Rect(0f, 0f, FilmGrainTextureSize, FilmGrainTextureSize),
				new Vector2(0.5f, 0.5f),
				FilmGrainPixelsPerUnit);
			filmGrainSprite.name = "RuntimeChapterTitleFilmGrainSprite";
			GenerateFilmGrainTexture(0);
		}

		private void GenerateFilmGrainTexture(int seed)
		{
			if (filmGrainTexture == null || filmGrainPixels == null) return;

			uint seedValue = unchecked((uint)(seed * 747796405 + 2891336453));
			for (int i = 0; i < filmGrainPixels.Length; ++i)
			{
				uint hash = Hash((uint)i ^ seedValue);
				bool bright = (hash & 0x10000) != 0;
				byte value = bright
					? (byte)(176 + (hash & 0x37))
					: (byte)(10 + (hash & 0x1f));
				byte alpha = (byte)(12 + ((hash >> 8) & 0x1f));
				if ((hash & 0x1f) == 0)
				{
					alpha = (byte)Mathf.Min(68, alpha + 26);
				}
				filmGrainPixels[i] = new Color32(value, value, value, alpha);
			}

			filmGrainTexture.SetPixels32(filmGrainPixels);
			filmGrainTexture.Apply(false, false);
		}

		private static uint Hash(uint value)
		{
			unchecked
			{
				value ^= value >> 16;
				value *= 0x7feb352d;
				value ^= value >> 15;
				value *= 0x846ca68b;
				value ^= value >> 16;
				return value;
			}
		}

		private void PrepareTitleDisappearLayer()
		{
			if (image_title == null || shader_title == null) return;

			if (titleMaterialInstance == null || titleMaterialInstance.shader != shader_title)
			{
				if (titleMaterialInstance != null)
				{
					DestroyObjectSafe(titleMaterialInstance);
				}
				titleMaterialInstance = new Material(shader_title);
				titleMaterialInstance.name = shader_title.name + " Runtime";
			}

			image_title.material = titleMaterialInstance;
			UpdateTitleDisappearMaterial(0f);
		}

		private void PrepareParticleLayer()
		{
			if (particle == null) return;
			if (particleRenderer == null) particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
			if (particleRenderer == null) return;

			Material sourceMaterial = particleRenderer.sharedMaterial;
			if (sourceMaterial != null)
			{
				if (particleMaterialInstance == null || particleMaterialInstance.shader != sourceMaterial.shader)
				{
					if (particleMaterialInstance != null)
					{
						DestroyObjectSafe(particleMaterialInstance);
					}
					particleMaterialInstance = new Material(sourceMaterial);
					particleMaterialInstance.name = sourceMaterial.name + " Runtime";
				}
			}
			else if (shader_particle != null && (particleMaterialInstance == null || particleMaterialInstance.shader != shader_particle))
			{
				if (particleMaterialInstance != null)
				{
					DestroyObjectSafe(particleMaterialInstance);
				}
				particleMaterialInstance = new Material(shader_particle);
				particleMaterialInstance.name = shader_particle.name + " Runtime";
			}

			if (particleMaterialInstance != null)
			{
				particleRenderer.material = particleMaterialInstance;
			}
		}

		private void ApplyTypeVisualProfile(TitleAnimationType type)
		{
			currentType = type;

			if (image_foreground != null)
			{
				image_foreground.enabled = image_foreground.sprite != null;
				image_foreground.color = GetForegroundColor(type, 0f);
			}
			if (image_filmGrain != null)
			{
				image_filmGrain.enabled = image_filmGrain.sprite != null;
				image_filmGrain.color = new Color(1f, 1f, 1f, 0f);
			}

			if (image_titleBG != null)
			{
				image_titleBG.color = Color.white;
			}
			if (image_title != null)
			{
				image_title.color = Color.white;
			}
			if (image_titleShadow != null)
			{
				image_titleShadow.color = new Color(0f, 0f, 0f, TitleShadowAlpha);
			}

			ConfigureParticle(type);
			UpdateRippleMaterial(0f, type);
		}

		private void ConfigureParticle(TitleAnimationType type)
		{
			if (particle == null) return;

			Color minColor;
			Color maxColor;
			float rateOverTime;
			switch (type)
			{
				case TitleAnimationType.BadEnd:
					minColor = new Color(0.85f, 0.45f, 0.45f, 0.85f);
					maxColor = new Color(1f, 0.88f, 0.88f, 0.65f);
					rateOverTime = 150f;
					break;
				case TitleAnimationType.NormalEnd:
					minColor = new Color(0.95f, 0.82f, 0.62f, 0.85f);
					maxColor = new Color(1f, 0.95f, 0.82f, 0.7f);
					rateOverTime = 185f;
					break;
				case TitleAnimationType.TrueEnd:
					minColor = new Color(1f, 0.95f, 0.75f, 0.9f);
					maxColor = new Color(1f, 1f, 0.92f, 0.78f);
					rateOverTime = 210f;
					break;
				case TitleAnimationType.Sui:
					minColor = new Color(0.88f, 0.9f, 1f, 0.85f);
					maxColor = new Color(0.98f, 1f, 1f, 0.7f);
					rateOverTime = 195f;
					break;
				default:
					minColor = new Color(0.9f, 0.92f, 1f, 0.85f);
					maxColor = new Color(1f, 1f, 1f, 0.72f);
					rateOverTime = 190f;
					break;
			}

			ParticleSystem.MainModule main = particle.main;
			main.useUnscaledTime = true;
			main.loop = true;
			main.playOnAwake = false;
			particleBaseMinColor = minColor;
			particleBaseMaxColor = maxColor;
			main.startColor = new ParticleSystem.MinMaxGradient(minColor, maxColor);
			particleBaseStartSpeed = main.startSpeed.constant;
			particleBaseLifetime = main.startLifetime.constant;
			ParticleSystem.EmissionModule emission = particle.emission;
			particleBaseEmissionRate = rateOverTime;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
			if (particleMaterialInstance != null && particleMaterialInstance.HasProperty("_Color"))
			{
				particleMaterialInstance.SetColor("_Color", maxColor);
			}
		}

		private void ApplyAnimatedVisuals(
			TitleAnimationType type,
			float elapsed,
			float initialBlackDuration,
			float revealDuration,
			float stableDuration,
			float particleDuration,
			float particleSettleDuration,
			float fadeToBlackDuration,
			float blackHoldDuration,
			float returnFadeDuration,
			RectTransform bgRect,
			Vector2 bgStart,
			RectTransform titleBgRect,
			Vector2 titleBgStart,
			RectTransform fgRect,
			Vector2 fgStart,
			Vector3 fgStartScale,
			RectTransform titleRect,
			Vector2 titleOffscreenStart,
			Vector2 titleStart,
			Vector3 titleStartScale)
		{
			float intensity = GetWaveIntensity(type);
			float revealProgress = GetSegmentProgress(elapsed, initialBlackDuration, revealDuration);
			float particleStart = initialBlackDuration + revealDuration + stableDuration;
			float particleProgress = GetSegmentProgress(elapsed, particleStart, particleDuration);
			float particleSettleStart = particleStart + particleDuration;
			float particleSettleProgress = GetSegmentProgress(elapsed, particleSettleStart, particleSettleDuration);
			float fadeToBlackStart = particleSettleStart + particleSettleDuration;
			float fadeToBlackProgress = GetSegmentProgress(elapsed, fadeToBlackStart, fadeToBlackDuration);
			float returnFadeStart = fadeToBlackStart + fadeToBlackDuration + blackHoldDuration;
			float returnFadeProgress = GetSegmentProgress(elapsed, returnFadeStart, returnFadeDuration);
			float titleDissolveProgress = GetTitleDissolveProgress(particleProgress, particleSettleProgress, fadeToBlackProgress);
			float contentReveal = elapsed < initialBlackDuration ? 0f : 1f;
			float contentVisibility = contentReveal * (1f - fadeToBlackProgress);
			float titleVisibility = contentVisibility * (1f - titleDissolveProgress);
			float titleBgDimming = Mathf.Clamp01((titleDissolveProgress - 0.35f) / 0.65f);
			float titleBgVisibility = contentVisibility * (1f - titleBgDimming * 0.22f);
			float fgVisibility = contentVisibility * (1f - fadeToBlackProgress * 0.72f);
			float activeWeight = 1f - fadeToBlackProgress * 0.65f;
			float titleSlideProgress = EaseOutCubic(Mathf.InverseLerp(
				initialBlackDuration,
				initialBlackDuration + TitleIntroSlideDuration,
				elapsed));
			float shelterAlpha;
			if (elapsed < initialBlackDuration)
			{
				shelterAlpha = 1f;
			}
			else if (elapsed < initialBlackDuration + revealDuration)
			{
				shelterAlpha = 1f - revealProgress;
			}
			else if (elapsed >= returnFadeStart)
			{
				shelterAlpha = 1f - returnFadeProgress;
			}
			else
			{
				shelterAlpha = fadeToBlackProgress;
			}
			SetShelterAlpha(shelterAlpha);

			if (bgRect != null)
			{
				bgRect.anchoredPosition = bgStart + new Vector2(
					Mathf.Sin(elapsed * 0.82f) * 12f * intensity * activeWeight,
					Mathf.Cos(elapsed * 0.55f) * 6f * intensity * activeWeight);
			}
			if (image_ChapterBg != null)
			{
				Color chapterBgColor = image_ChapterBg.color;
				chapterBgColor.a = elapsed >= returnFadeStart ? 0f : contentReveal;
				image_ChapterBg.color = chapterBgColor;
			}

			if (titleBgRect != null)
			{
				titleBgRect.anchoredPosition = titleBgStart + new Vector2(
					Mathf.Sin(elapsed * 0.45f + 0.35f) * 4f * intensity * activeWeight,
					Mathf.Cos(elapsed * 0.32f + 0.2f) * 2f * intensity * activeWeight);
			}

			if (fgRect != null)
			{
				float fgPulse = Mathf.Sin(elapsed * 1.7f + 0.25f);
				fgRect.anchoredPosition = fgStart + new Vector2(
					Mathf.Sin(elapsed * 1.12f) * 5f * intensity * activeWeight,
					Mathf.Cos(elapsed * 0.88f + 0.6f) * 3f * intensity * activeWeight);
				fgRect.localScale = fgStartScale * (1f + (0.016f * intensity + fgPulse * 0.007f * intensity) * activeWeight);
				if (image_foreground != null)
				{
					Color foregroundColor = GetForegroundColor(type, fgPulse);
					foregroundColor.a *= fgVisibility;
					image_foreground.color = foregroundColor;
				}
			}

			if (titleRect != null)
			{
				Vector2 titleIntroPosition = Vector2.LerpUnclamped(titleOffscreenStart, titleStart, titleSlideProgress);
				float settledWeight = titleSlideProgress;
				titleRect.anchoredPosition = titleIntroPosition + new Vector2(
					Mathf.Sin(elapsed * 0.58f + 0.85f) * 2f * intensity * activeWeight * settledWeight,
					Mathf.Sin(elapsed * 1.02f) * intensity * activeWeight * settledWeight);
				float titlePulse = 1f + Mathf.Sin(elapsed * 0.86f + 0.4f) * 0.009f * intensity * activeWeight;
				float titleIntroScale = Mathf.Lerp(0.97f, 1f, titleSlideProgress);
				titleRect.localScale = titleStartScale * (titleIntroScale * titlePulse);
				SyncTitleShadowTransform(titleVisibility);
			}

			if (image_title != null)
			{
				Color titleColor = image_title.color;
				titleColor.a = titleVisibility;
				image_title.color = titleColor;
			}
			if (image_titleShadow != null)
			{
				Color shadowColor = image_titleShadow.color;
				shadowColor.a = TitleShadowAlpha * titleVisibility;
				image_titleShadow.color = shadowColor;
			}

			if (image_titleBG != null)
			{
				Color titleBgColor = image_titleBG.color;
				titleBgColor.a = titleBgVisibility;
				image_titleBG.color = titleBgColor;
			}

			UpdateParticleFade(revealProgress, particleProgress, particleSettleProgress, fadeToBlackProgress);
			UpdateRippleMaterial(elapsed, type);
			UpdateFilmGrain(elapsed, type, contentVisibility, particleProgress, fadeToBlackProgress);
			UpdateTitleDisappearMaterial(titleDissolveProgress);
		}

		private float GetSegmentProgress(float elapsed, float start, float duration)
		{
			if (duration <= 0f) return elapsed >= start ? 1f : 0f;
			return EaseInOut(Mathf.InverseLerp(start, start + duration, elapsed));
		}

		private float EaseInOut(float value)
		{
			value = Mathf.Clamp01(value);
			return value * value * (3f - 2f * value);
		}

		private float EaseOutCubic(float value)
		{
			value = Mathf.Clamp01(value);
			float inverse = 1f - value;
			return 1f - inverse * inverse * inverse;
		}

		private float GetParticleBurstProgress(float particleProgress)
		{
			return Mathf.Clamp01(particleProgress / ParticleBurstRampProgress);
		}

		private float GetTitleDissolveProgress(float particleProgress, float particleSettleProgress, float fadeToBlackProgress)
		{
			if (particleProgress < TitleDissolveStartParticleProgress) return 0f;

			float combinedProgress;
			if (particleProgress < 1f)
			{
				float particlePhase = Mathf.InverseLerp(TitleDissolveStartParticleProgress, 1f, particleProgress);
				combinedProgress = particlePhase * TitleDissolveParticlePhaseWeight;
			}
			else if (particleSettleProgress < 1f)
			{
				combinedProgress = Mathf.Lerp(TitleDissolveParticlePhaseWeight, TitleDissolveSettlePhaseWeight, particleSettleProgress);
			}
			else
			{
				float blackPhase = Mathf.InverseLerp(0f, TitleDissolveEndFadeToBlackProgress, fadeToBlackProgress);
				combinedProgress = Mathf.Lerp(TitleDissolveSettlePhaseWeight, 1f, blackPhase);
			}
			return EaseInOut(combinedProgress);
		}

		private Vector2 GetTitleOffscreenStartPosition(RectTransform titleRect, Vector2 finalPosition, Vector3 finalScale)
		{
			RectTransform parent = titleRect != null ? titleRect.parent as RectTransform : null;
			if (parent == null) return finalPosition + new Vector2(-Screen.width - TitleIntroOffscreenPadding, 0f);

			Rect parentRect = parent.rect;
			Vector2 parentSize = ResolveRectSize(parent);
			if (parentSize.x > 0f && parentSize.y > 0f)
			{
				parentRect = new Rect(
					-parent.pivot.x * parentSize.x,
					-parent.pivot.y * parentSize.y,
					parentSize.x,
					parentSize.y);
			}

			float anchorX = (titleRect.anchorMin.x + titleRect.anchorMax.x) * 0.5f;
			float anchorReferenceX = Mathf.Lerp(parentRect.xMin, parentRect.xMax, anchorX);
			float titleWidth = Mathf.Max(1f, titleRect.rect.width) * Mathf.Abs(finalScale.x);
			float finalRightEdge = anchorReferenceX + finalPosition.x + (1f - titleRect.pivot.x) * titleWidth;
			float targetRightEdge = parentRect.xMin - TitleIntroOffscreenPadding;
			float offsetX = targetRightEdge - finalRightEdge;
			return finalPosition + new Vector2(offsetX, 0f);
		}

		private void SetShelterAlpha(float alpha)
		{
			if (image_shelter == null) return;

			Color color = image_shelter.color;
			color.r = 0f;
			color.g = 0f;
			color.b = 0f;
			color.a = Mathf.Clamp01(alpha);
			image_shelter.color = color;
			image_shelter.enabled = true;
		}

		private Color GetForegroundColor(TitleAnimationType type, float pulse)
		{
			Color baseColor;
			switch (type)
			{
				case TitleAnimationType.BadEnd:
					baseColor = new Color(1f, 0.94f, 0.94f, 0.1f);
					break;
				case TitleAnimationType.NormalEnd:
					baseColor = new Color(1f, 0.98f, 0.92f, 0.095f);
					break;
				case TitleAnimationType.TrueEnd:
					baseColor = new Color(1f, 1f, 0.96f, 0.105f);
					break;
				case TitleAnimationType.Sui:
					baseColor = new Color(0.94f, 0.98f, 1f, 0.095f);
					break;
				default:
					baseColor = new Color(0.98f, 0.99f, 1f, 0.095f);
					break;
			}

			baseColor.a = Mathf.Clamp01(baseColor.a + pulse * 0.012f);
			return baseColor;
		}

		private void EnsureVisualLayerOrder()
		{
			MoveVisualLayerToFront(image_ChapterBg != null ? image_ChapterBg.transform : null);
			MoveVisualLayerToFront(image_titleBG != null ? image_titleBG.transform : null);
			MoveVisualLayerToFront(image_foreground != null ? image_foreground.transform : null);
			MoveVisualLayerToFront(image_filmGrain != null ? image_filmGrain.transform : null);
			MoveVisualLayerToFront(particle != null ? particle.transform : null);
			MoveVisualLayerToFront(image_titleShadow != null ? image_titleShadow.transform : null);
			MoveVisualLayerToFront(image_title != null ? image_title.transform : null);
			MoveVisualLayerToFront(image_shelter != null ? image_shelter.transform : null);
		}

		private void MoveVisualLayerToFront(Transform target)
		{
			Transform layer = GetRootVisualLayer(target);
			if (layer == null || layer.parent == null) return;
			layer.SetAsLastSibling();
		}

		private Transform GetRootVisualLayer(Transform target)
		{
			if (target == null) return null;
			if (target == transform) return target;

			Transform current = target;
			while (current.parent != null && current.parent != transform)
			{
				current = current.parent;
			}

			return current.parent == transform ? current : target;
		}

		private void EnsureTitleShadowImage()
		{
			if (image_titleShadow != null)
			{
				image_titleShadow.enabled = false;
			}
		}

		private void SyncTitleShadowSprite()
		{
			if (image_title == null || image_titleShadow == null) return;

			image_titleShadow.sprite = image_title.sprite;
			image_titleShadow.enabled = image_title.enabled && image_title.sprite != null;
			image_titleShadow.type = image_title.type;
			image_titleShadow.preserveAspect = image_title.preserveAspect;
			image_titleShadow.color = new Color(0f, 0f, 0f, TitleShadowAlpha);
		}

		private void ApplyTitleShadowOffsetAndScale()
		{
			SyncTitleShadowTransform(image_title != null ? image_title.color.a : 1f);
		}

		private void SyncTitleShadowTransform(float alpha)
		{
			if (image_title == null || image_titleShadow == null) return;

			RectTransform source = image_title.rectTransform;
			RectTransform shadow = image_titleShadow.rectTransform;
			shadow.anchorMin = source.anchorMin;
			shadow.anchorMax = source.anchorMax;
			shadow.pivot = source.pivot;
			shadow.sizeDelta = source.sizeDelta;
			shadow.anchoredPosition = source.anchoredPosition + new Vector2(TitleShadowOffsetX, TitleShadowOffsetY);
			shadow.localScale = source.localScale * TitleShadowScale;
			shadow.localRotation = source.localRotation;
			image_titleShadow.enabled = image_title.enabled && image_title.sprite != null;
			Color shadowColor = image_titleShadow.color;
			shadowColor.a = TitleShadowAlpha * Mathf.Clamp01(alpha);
			image_titleShadow.color = shadowColor;
		}

		private float GetWaveIntensity(TitleAnimationType type)
		{
			switch (type)
			{
				case TitleAnimationType.BadEnd:
					return 1.08f;
				case TitleAnimationType.TrueEnd:
					return 0.9f;
				case TitleAnimationType.NormalEnd:
					return 0.96f;
				case TitleAnimationType.Sui:
					return 1.02f;
				default:
					return 1f;
			}
		}

		private void UpdateRippleMaterial(float elapsed, TitleAnimationType type)
		{
			if (rippleMaterialInstance == null) return;

			float intensity = GetWaveIntensity(type);
			float strength = 0.0145f + 0.0045f * intensity + Mathf.Sin(elapsed * 1.35f) * 0.0025f * intensity;
			float speed = 0.065f + 0.015f * intensity;
			float wave = 0.034f + Mathf.Sin(elapsed * 0.78f + 0.35f) * 0.008f * intensity;
			float scaleValue = 0.5f + Mathf.Sin(elapsed * 0.48f + 0.8f) * 0.03f;

			if (rippleMaterialInstance.HasProperty(ShaderStrengthId))
			{
				rippleMaterialInstance.SetFloat(ShaderStrengthId, strength);
			}
			if (rippleMaterialInstance.HasProperty(ShaderSpeedId))
			{
				rippleMaterialInstance.SetFloat(ShaderSpeedId, speed);
			}
			if (rippleMaterialInstance.HasProperty(ShaderWaveId))
			{
				rippleMaterialInstance.SetFloat(ShaderWaveId, wave);
			}
			if (rippleMaterialInstance.HasProperty(ShaderScaleId))
			{
				rippleMaterialInstance.SetFloat(ShaderScaleId, scaleValue);
			}
		}

		private void UpdateFilmGrain(float elapsed, TitleAnimationType type, float contentVisibility, float particleProgress, float fadeToBlackProgress)
		{
			if (image_filmGrain == null) return;

			if (elapsed >= nextFilmGrainRefreshTime)
			{
				GenerateFilmGrainTexture(++filmGrainSeed);
				nextFilmGrainRefreshTime = elapsed + FilmGrainRefreshInterval;
			}

			float intensity = GetWaveIntensity(type);
			float burst = Mathf.SmoothStep(0f, 1f, particleProgress);
			float flicker = 0.5f + Mathf.Sin(elapsed * 19.7f + filmGrainSeed * 0.37f) * 0.5f;
			float fadeGate = 1f - fadeToBlackProgress * 0.32f;
			float alpha = (FilmGrainBaseAlpha + FilmGrainBurstAlpha * burst + flicker * 0.04f) * intensity * contentVisibility * fadeGate;
			alpha = Mathf.Clamp01(alpha);

			image_filmGrain.enabled = alpha > 0.004f && image_filmGrain.sprite != null;
			Color color = image_filmGrain.color;
			color.a = alpha;
			image_filmGrain.color = color;
		}

		private void UpdateTitleDisappearMaterial(float fadeProgress)
		{
			if (titleMaterialInstance == null) return;

			if (titleMaterialInstance.HasProperty(ShaderStrengthId))
			{
				titleMaterialInstance.SetFloat(ShaderStrengthId, fadeProgress);
			}
			if (titleMaterialInstance.HasProperty(ShaderScaleId))
			{
				titleMaterialInstance.SetFloat(ShaderScaleId, 0.5f + fadeProgress * 0.45f);
			}
		}

		private void UpdateParticleFade(float revealProgress, float particleProgress, float particleSettleProgress, float fadeToBlackProgress)
		{
			if (particle == null) return;

			float burstGate = GetParticleBurstProgress(particleProgress);
			float settleGate = 1f - Mathf.SmoothStep(0f, 1f, particleSettleProgress);
			float blackGate = Mathf.Pow(1f - fadeToBlackProgress, 2f);
			float particleVisibility = Mathf.Clamp01(burstGate * Mathf.Lerp(1f, 0.45f, 1f - settleGate) * blackGate * 1.18f);

			ParticleSystem.EmissionModule emission = particle.emission;
			float emissionRate = Mathf.Max(0f, particleBaseEmissionRate * particleVisibility * Mathf.Lerp(0.65f, 1.05f, burstGate));
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(emissionRate);

			ParticleSystem.MainModule main = particle.main;
			Color visibleMinColor = particleBaseMinColor;
			visibleMinColor.a *= particleVisibility;
			Color visibleMaxColor = particleBaseMaxColor;
			visibleMaxColor.a *= particleVisibility;
			main.startColor = new ParticleSystem.MinMaxGradient(visibleMinColor, visibleMaxColor);
			if (particleBaseStartSpeed > 0f)
			{
				float speedScale = Mathf.Lerp(0.75f, 1.12f, particleProgress) * Mathf.Lerp(1f, 0.3f, Mathf.Max(particleSettleProgress, fadeToBlackProgress));
				main.startSpeed = new ParticleSystem.MinMaxCurve(Mathf.Max(0.01f, particleBaseStartSpeed * speedScale));
			}
			if (particleBaseLifetime > 0f)
			{
				float lifetimeScale = Mathf.Lerp(0.82f, 1f, revealProgress) * Mathf.Lerp(1f, 0.42f, Mathf.Max(particleSettleProgress, fadeToBlackProgress));
				main.startLifetime = new ParticleSystem.MinMaxCurve(Mathf.Max(0.01f, particleBaseLifetime * lifetimeScale));
			}
			if (particleMaterialInstance != null && particleMaterialInstance.HasProperty("_Color"))
			{
				Color color = particleMaterialInstance.GetColor("_Color");
				color.a = particleVisibility;
				particleMaterialInstance.SetColor("_Color", color);
			}

			if (fadeToBlackProgress >= 0.985f && particle.isPlaying)
			{
				particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			else if (particleVisibility > 0.01f && !particle.isPlaying)
			{
				particle.Play(true);
			}
		}

		private void ResetParticlePlayback()
		{
			if (particle == null) return;

			particle.gameObject.SetActive(true);
			ParticleSystem.EmissionModule emission = particle.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
			particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			particle.Clear(true);
		}

		private void DestroySelf(bool keepVisibleThisFrame = false)
		{
			isPlaying = false;
			if (!keepVisibleThisFrame)
			{
				SetShelterAlpha(0f);
			}
			if (canvasGroup != null)
			{
				canvasGroup.alpha = keepVisibleThisFrame ? 1f : 0f;
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
			DestroyObjectSafe(gameObject);
		}

		private void DestroyObjectSafe(Object target)
		{
			if (target == null) return;

			if (Application.isPlaying)
			{
				Destroy(target);
			}
			else
			{
				DestroyImmediate(target);
			}
		}

		private static Transform FindChildRecursive(Transform root, string targetName)
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
