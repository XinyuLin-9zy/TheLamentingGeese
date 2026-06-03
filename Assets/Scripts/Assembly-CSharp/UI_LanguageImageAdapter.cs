using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utage;

[RequireComponent(typeof(Image))]
public class UI_LanguageImageAdapter : MonoBehaviour
{
	private Image image;

	public Sprite SC;

	public Sprite TC;

	public Sprite English;

	public Sprite Japanese;

	public Sprite Russian;

	private LanguageManagerBase subscribedLanguageManager;

	private void Awake()
	{
		image = GetComponent<Image>();
		RefreshImage();
	}

	private void OnEnable()
	{
		image = image != null ? image : GetComponent<Image>();
		SubscribeLanguageChanged();
		RefreshImage();
	}

	private void OnDisable()
	{
		UnsubscribeLanguageChanged();
	}

	private void RefreshImage()
	{
		if (image == null) return;

		Sprite sprite = GetCurrentSprite();
		if (sprite != null)
		{
			image.sprite = sprite;
		}
	}

	private void HandleNotification(INotification notification)
	{
		RefreshImage();
	}

	private void SubscribeLanguageChanged()
	{
		LanguageManagerBase languageManager = LanguageManagerBase.Instance;
		if (languageManager == null || subscribedLanguageManager == languageManager) return;

		UnsubscribeLanguageChanged();
		subscribedLanguageManager = languageManager;
		subscribedLanguageManager.OnChangeLanguage += RefreshImage;
	}

	private void UnsubscribeLanguageChanged()
	{
		if (subscribedLanguageManager == null) return;
		subscribedLanguageManager.OnChangeLanguage -= RefreshImage;
		subscribedLanguageManager = null;
	}

	private Sprite GetCurrentSprite()
	{
		LanguageManagerBase languageManager = LanguageManagerBase.Instance;
		string language = languageManager != null ? languageManager.CurrentLanguage : "";
		Sprite sprite;
		switch (NormalizeLanguage(language))
		{
			case "tc":
				sprite = TC;
				break;
			case "english":
				sprite = English;
				break;
			case "japanese":
				sprite = Japanese;
				break;
			case "russian":
				sprite = Russian;
				break;
			default:
				sprite = SC;
				break;
		}
		return sprite != null ? sprite : SC;
	}

	private static string NormalizeLanguage(string language)
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
