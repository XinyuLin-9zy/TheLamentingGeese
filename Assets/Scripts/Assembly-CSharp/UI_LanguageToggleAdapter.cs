using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class UI_LanguageToggleAdapter : MonoBehaviour
{
	private Toggle toggle;

	public ButtonSprites SC;

	public ButtonSprites TC;

	public ButtonSprites English;

	public ButtonSprites Japanese;

	public ButtonSprites Russian;

	private LanguageManagerBase subscribedLanguageManager;

	private void Awake()
	{
		toggle = GetComponent<Toggle>();
		RefreshButtonSprite();
	}

	private void OnEnable()
	{
		toggle = toggle != null ? toggle : GetComponent<Toggle>();
		SubscribeLanguageChanged();
		RefreshButtonSprite();
	}

	private void OnDisable()
	{
		UnsubscribeLanguageChanged();
	}

	private void RefreshButtonSprite()
	{
		if (toggle == null) return;

		ButtonSprites sprites = GetCurrentSprites();
		if (sprites == null || !HasAnySprite(sprites)) return;

		Image image = toggle.targetGraphic as Image ?? GetComponent<Image>();
		if (image != null && sprites.normal != null)
		{
			image.sprite = sprites.normal;
			toggle.targetGraphic = image;
		}

		SpriteState spriteState = toggle.spriteState;
		if (sprites.highlighted != null) spriteState.highlightedSprite = sprites.highlighted;
		if (sprites.Pressed != null) spriteState.pressedSprite = sprites.Pressed;
		if (sprites.selected != null) spriteState.selectedSprite = sprites.selected;
		if (sprites.disabled != null) spriteState.disabledSprite = sprites.disabled;
		toggle.spriteState = spriteState;
		toggle.transition = Selectable.Transition.SpriteSwap;
	}

	private void HandleNotification(INotification notification)
	{
		RefreshButtonSprite();
	}

	private void SubscribeLanguageChanged()
	{
		LanguageManagerBase languageManager = LanguageManagerBase.Instance;
		if (languageManager == null || subscribedLanguageManager == languageManager) return;

		UnsubscribeLanguageChanged();
		subscribedLanguageManager = languageManager;
		subscribedLanguageManager.OnChangeLanguage += RefreshButtonSprite;
	}

	private void UnsubscribeLanguageChanged()
	{
		if (subscribedLanguageManager == null) return;
		subscribedLanguageManager.OnChangeLanguage -= RefreshButtonSprite;
		subscribedLanguageManager = null;
	}

	private ButtonSprites GetCurrentSprites()
	{
		LanguageManagerBase languageManager = LanguageManagerBase.Instance;
		string language = languageManager != null ? languageManager.CurrentLanguage : "";
		switch (NormalizeLanguage(language))
		{
			case "tc": return TC;
			case "english": return English;
			case "japanese": return Japanese;
			case "russian": return Russian;
			default: return SC;
		}
	}

	private static bool HasAnySprite(ButtonSprites sprites)
	{
		if (sprites == null) return false;
		return sprites.normal != null
		       || sprites.highlighted != null
		       || sprites.Pressed != null
		       || sprites.selected != null
		       || sprites.disabled != null;
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
