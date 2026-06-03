using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace UI
{
	public class UI_ScreenText : MonoBehaviour
	{
		[SerializeField]
		private AdvEngine engine;

		private GameObject animationLinker;

		private TextMeshProUGUI text_content;

		public TMP_FontAsset[] fonts;

		private CanvasGroup canvasGroup;

		private Coroutine animationCoroutine;

		public bool IsPlaying { get; private set; }

		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);

		private void Awake()
		{
			Init();
		}

		private void OnDestroy()
		{
			if (animationCoroutine != null)
			{
				StopCoroutine(animationCoroutine);
			}
			if (animationLinker != null)
			{
				Destroy(animationLinker);
			}
		}

		private void Init()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			if (text_content == null)
			{
				Transform text = FindChildRecursive(transform, "Text") ?? FindChildRecursive(transform, "Content");
				text_content = text != null ? text.GetComponent<TextMeshProUGUI>() : null;
			}
			if (text_content == null)
			{
				GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
				textObject.transform.SetParent(transform, false);
				text_content = textObject.GetComponent<TextMeshProUGUI>();
			}

			RectTransform rect = text_content.transform as RectTransform;
			if (rect != null)
			{
				rect.anchorMin = new Vector2(0f, 0.5f);
				rect.anchorMax = new Vector2(1f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = Vector2.zero;
				rect.sizeDelta = new Vector2(0f, Mathf.Max(260f, rect.sizeDelta.y));
				rect.localScale = Vector3.one;
			}

			text_content.alignment = TextAlignmentOptions.Center;
			text_content.enableWordWrapping = true;
			text_content.raycastTarget = false;
		}

		public void SetInfo(string content, string textColor, float fadeInTime, float waitTime, float fadeOutTime, float fontSize = 150f, int fontIndex = 1)
		{
			Init();
			gameObject.SetActive(true);
			ApplyText(content, textColor, fontSize, fontIndex);

			if (animationCoroutine != null)
			{
				StopCoroutine(animationCoroutine);
			}
			animationCoroutine = StartCoroutine(CoPlay(Mathf.Max(0.01f, fadeInTime), Mathf.Max(0f, waitTime), Mathf.Max(0.01f, fadeOutTime)));
		}

		private void ApplyText(string content, string textColor, float fontSize, int fontIndex)
		{
			if (text_content == null) return;

			text_content.text = content ?? "";
			text_content.fontSize = Mathf.Max(12f, fontSize);
			if (fonts != null && fonts.Length > 0)
			{
				text_content.font = fonts[Mathf.Clamp(fontIndex, 0, fonts.Length - 1)];
			}
			text_content.color = ParseColor(textColor);
		}

		private IEnumerator CoPlay(float fadeInTime, float waitTime, float fadeOutTime)
		{
			IsPlaying = true;
			if (canvasGroup != null)
			{
				canvasGroup.alpha = 0f;
				canvasGroup.blocksRaycasts = true;
				canvasGroup.interactable = false;
			}

			yield return FadeCanvas(0f, 1f, fadeInTime);
			if (waitTime > 0f)
			{
				yield return new WaitForSecondsRealtime(waitTime);
			}
			yield return FadeCanvas(1f, 0f, fadeOutTime);

			IsPlaying = false;
			Destroy(gameObject);
		}

		private IEnumerator FadeCanvas(float from, float to, float duration)
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				if (canvasGroup != null)
				{
					canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
				}
				yield return null;
			}
			if (canvasGroup != null)
			{
				canvasGroup.alpha = to;
			}
		}

		private Color ParseColor(string raw)
		{
			if (string.IsNullOrEmpty(raw)) return Color.white;

			string colorText = raw.Trim();
			if (!colorText.StartsWith("#") && colorText.Length == 6)
			{
				colorText = "#" + colorText;
			}

			Color color;
			return ColorUtility.TryParseHtmlString(colorText, out color) ? color : Color.white;
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
