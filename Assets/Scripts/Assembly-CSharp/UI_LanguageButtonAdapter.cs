using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utage;

[RequireComponent(typeof(Button))]
public class UI_LanguageButtonAdapter : MonoBehaviour
{
	private Button[] buttons;

	public ButtonSprites SC;

	public ButtonSprites TC;

	public ButtonSprites English;

	public ButtonSprites Japanese;

	public ButtonSprites Russian;

	private LanguageManagerBase subscribedLanguageManager;

	private void Awake()
	{
		CacheButtons();
		RefreshButtonSprite();
	}

	private void OnEnable()
	{
		CacheButtons();
		SubscribeLanguageChanged();
		RefreshButtonSprite();
	}

	private void OnDisable()
	{
		UnsubscribeLanguageChanged();
	}

	private void RefreshButtonSprite()
	{
		if (buttons == null || buttons.Length == 0)
		{
			CacheButtons();
		}
		if (buttons == null || buttons.Length == 0) return;

		ButtonSprites sprites = GetCurrentSprites();
		if (sprites == null || !HasAnySprite(sprites)) return;

		Image image = GetComponent<Image>();
		if (image != null && sprites.normal != null)
		{
			image.sprite = sprites.normal;
		}

		foreach (Button targetButton in buttons)
		{
			if (targetButton == null) continue;

			Image targetImage = targetButton.image != null ? targetButton.image : image ?? targetButton.GetComponent<Image>();
			if (targetImage != null)
			{
				if (sprites.normal != null)
				{
					targetImage.sprite = sprites.normal;
				}
				targetButton.targetGraphic = targetImage;
			}

			SpriteState spriteState = targetButton.spriteState;
			if (sprites.highlighted != null) spriteState.highlightedSprite = sprites.highlighted;
			if (sprites.Pressed != null) spriteState.pressedSprite = sprites.Pressed;
			if (sprites.selected != null) spriteState.selectedSprite = sprites.selected;
			if (sprites.disabled != null) spriteState.disabledSprite = sprites.disabled;
			targetButton.spriteState = spriteState;
			targetButton.transition = Selectable.Transition.SpriteSwap;
		}
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
		ButtonSprites sprites;
		switch (NormalizeLanguage(language))
		{
			case "tc":
				sprites = TC;
				break;
			case "english":
				sprites = English;
				break;
			case "japanese":
				sprites = Japanese;
				break;
			case "russian":
				sprites = Russian;
				break;
			default:
				sprites = SC;
				break;
		}
		return HasAnySprite(sprites) ? sprites : SC;
	}

	private static bool HasAnySprite(ButtonSprites sprites)
	{
		return sprites != null
		       && (sprites.normal != null
		       || sprites.highlighted != null
		       || sprites.Pressed != null
		       || sprites.selected != null
		       || sprites.disabled != null);
	}

	private void CacheButtons()
	{
		buttons = GetComponents<Button>();
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
