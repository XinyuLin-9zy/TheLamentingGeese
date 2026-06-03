using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace UI
{
	public class UI_Staff : MonoBehaviour
	{
		public float totalHeight;

		[SerializeField]
		private AdvEngine engine;

		public RectTransform bgRoot;

		public Slider slider_skip;

		public float skipTime;

		private float clock;

		public CanvasGroup groupSkip;

		public AudioSource audioPlayer;

		public AudioClip[] bgm;

		private CanvasGroup canvasGroup;

		private Coroutine bgmCoroutine;

		private bool ending;

		private float skipClock;

		private float duration;

		public bool IsPlaying { get; private set; }

		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);

		private void Awake()
		{
			EnsureRuntimeReferences();
		}

		private void Update()
		{
			if (!IsPlaying || ending) return;

			clock += Time.unscaledDeltaTime;
			UpdateScroll();
			UpdateSkipHold();

			if (clock >= duration)
			{
				StartCoroutine(CoFinish());
			}
		}

		private IEnumerator PlayBGM()
		{
			if (audioPlayer == null || bgm == null || bgm.Length == 0) yield break;

			int index = 0;
			while (IsPlaying)
			{
				AudioClip clip = bgm[index % bgm.Length];
				++index;
				if (clip == null)
				{
					yield return null;
					continue;
				}

				audioPlayer.clip = clip;
				audioPlayer.loop = false;
				audioPlayer.Play();
				float elapsed = 0f;
				while (elapsed < clip.length && IsPlaying)
				{
					elapsed += Time.unscaledDeltaTime;
					yield return null;
				}
			}
		}

		public void SetInfo()
		{
			EnsureRuntimeReferences();
			gameObject.SetActive(true);
			IsPlaying = true;
			ending = false;
			clock = 0f;
			skipClock = 0f;
			duration = Mathf.Clamp(Mathf.Max(totalHeight, 2400f) / 750f, 25f, 75f);

			if (canvasGroup != null)
			{
				canvasGroup.alpha = 1f;
				canvasGroup.blocksRaycasts = true;
				canvasGroup.interactable = true;
			}
			if (groupSkip != null)
			{
				groupSkip.alpha = 0f;
			}
			if (slider_skip != null)
			{
				slider_skip.minValue = 0f;
				slider_skip.maxValue = 1f;
				slider_skip.SetValueWithoutNotify(0f);
			}
			if (bgRoot != null)
			{
				Vector2 position = bgRoot.anchoredPosition;
				position.y = 0f;
				bgRoot.anchoredPosition = position;
			}

			if (bgmCoroutine != null)
			{
				StopCoroutine(bgmCoroutine);
			}
			bgmCoroutine = StartCoroutine(PlayBGM());
		}

		private void OnDestroy()
		{
			IsPlaying = false;
			if (bgmCoroutine != null)
			{
				StopCoroutine(bgmCoroutine);
			}
			if (audioPlayer != null)
			{
				audioPlayer.Stop();
			}
		}

		private void EnsureRuntimeReferences()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}
			if (bgRoot == null)
			{
				Transform bg = FindChildRecursive(transform, "BG");
				bgRoot = bg as RectTransform;
			}
			if (slider_skip == null)
			{
				slider_skip = GetComponentInChildren<Slider>(true);
			}
			if (groupSkip == null && slider_skip != null)
			{
				groupSkip = slider_skip.GetComponentInParent<CanvasGroup>(true);
			}
			if (audioPlayer == null)
			{
				audioPlayer = GetComponentInChildren<AudioSource>(true);
			}
			if (audioPlayer != null)
			{
				audioPlayer.volume = 1f;
			}
		}

		private void UpdateScroll()
		{
			if (bgRoot == null) return;

			float progress = Mathf.Clamp01(clock / Mathf.Max(0.1f, duration));
			float eased = Mathf.SmoothStep(0f, 1f, progress);
			Vector2 position = bgRoot.anchoredPosition;
			position.y = Mathf.Lerp(0f, Mathf.Max(totalHeight, 0f), eased);
			bgRoot.anchoredPosition = position;
		}

		private void UpdateSkipHold()
		{
			bool holding = InputUtil.IsInputGuiClose()
				|| Input.GetMouseButton(0)
				|| Input.GetKey(KeyCode.Space)
				|| Input.GetKey(KeyCode.Return);

			float holdTime = Mathf.Max(0.1f, skipTime);
			skipClock = holding
				? Mathf.Min(holdTime, skipClock + Time.unscaledDeltaTime)
				: Mathf.Max(0f, skipClock - Time.unscaledDeltaTime * 1.5f);

			float value = Mathf.Clamp01(skipClock / holdTime);
			if (slider_skip != null)
			{
				slider_skip.SetValueWithoutNotify(value);
			}
			if (groupSkip != null)
			{
				groupSkip.alpha = value > 0f ? Mathf.Lerp(0.25f, 1f, value) : 0f;
			}
			if (value >= 1f)
			{
				StartCoroutine(CoFinish());
			}
		}

		private IEnumerator CoFinish()
		{
			if (ending) yield break;

			ending = true;
			float from = canvasGroup != null ? canvasGroup.alpha : 1f;
			float elapsed = 0f;
			const float fadeTime = 0.6f;
			while (elapsed < fadeTime)
			{
				elapsed += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(elapsed / fadeTime);
				if (canvasGroup != null)
				{
					canvasGroup.alpha = Mathf.Lerp(from, 0f, t);
				}
				if (audioPlayer != null)
				{
					audioPlayer.volume = Mathf.Lerp(1f, 0f, t);
				}
				yield return null;
			}

			IsPlaying = false;
			Destroy(gameObject);
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
