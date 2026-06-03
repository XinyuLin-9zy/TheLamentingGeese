using UnityEngine;
using UnityEngine.UI;
using Utage;

public class VoiceStop : MonoBehaviour
{
	public UtageUguiConfig config;

	public Button click;

	public Button voice;

	private bool isInitialized;

	private static readonly Color OptionNormalColor = Color.white;

	private static readonly Color OptionSelectedColor = new Color(0.68f, 0.67f, 0.82f, 1f);

	private static readonly Color OptionLabelColor = new Color(0.08f, 0.08f, 0.09f, 1f);

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		CheckVoiceStop();
	}

	private void OnEnable()
	{
		Initialize();
		SubscribeConfig();
		CheckVoiceStop();
	}

	private void OnDisable()
	{
		UnsubscribeConfig();
	}

	public void CheckVoiceStop()
	{
		Initialize();
		AdvEngine engine = config != null ? config.Engine : null;
		if (engine == null) return;

		bool stopOnClick = engine.Config.VoiceStopType == VoiceStopType.OnClick;
		SetSelected(click, stopOnClick);
		SetSelected(voice, !stopOnClick);
	}

	private void Initialize()
	{
		if (isInitialized) return;

		if (config == null)
		{
			config = GetComponentInParent<UtageUguiConfig>(true);
		}

		if (click == null) click = FindButton("Click", "OnClick");
		if (voice == null) voice = FindButton("NextVoice", "Voice", "OnNextVoice");

		Bind(click, () => SetVoiceStopType(VoiceStopType.OnClick));
		Bind(voice, () => SetVoiceStopType(VoiceStopType.OnNextVoice));

		isInitialized = true;
	}

	private void Bind(Button button, UnityEngine.Events.UnityAction action)
	{
		if (button == null || action == null) return;
		if (button.targetGraphic == null)
		{
			button.targetGraphic = button.GetComponent<Graphic>() ?? button.GetComponentInChildren<Graphic>(true);
		}
		button.onClick.RemoveListener(action);
		button.onClick.AddListener(action);
	}

	private void SetVoiceStopType(VoiceStopType type)
	{
		AdvEngine engine = config != null ? config.Engine : null;
		if (engine == null) return;

		engine.Config.VoiceStopType = type;
		CheckVoiceStop();
	}

	private void SetSelected(Button button, bool selected)
	{
		if (button == null) return;
		ApplyOptionButtonStyle(button, selected);
		button.interactable = !selected;
	}

	private void ApplyOptionButtonStyle(Button button, bool selected)
	{
		Graphic target = button.targetGraphic ?? button.GetComponent<Graphic>() ?? button.GetComponentInChildren<Graphic>(true);
		if (target != null)
		{
			button.targetGraphic = target;
			target.color = selected ? OptionSelectedColor : OptionNormalColor;
		}

		ColorBlock colors = button.colors;
		colors.normalColor = OptionNormalColor;
		colors.highlightedColor = new Color(0.94f, 0.94f, 0.97f, 1f);
		colors.pressedColor = new Color(0.78f, 0.78f, 0.88f, 1f);
		colors.selectedColor = colors.highlightedColor;
		colors.disabledColor = OptionSelectedColor;
		colors.colorMultiplier = 1f;
		colors.fadeDuration = 0.02f;
		button.colors = colors;

		foreach (Text text in button.GetComponentsInChildren<Text>(true))
		{
			if (text == null) continue;
			text.color = OptionLabelColor;
			text.raycastTarget = false;
		}
	}

	private void SubscribeConfig()
	{
		if (config == null) return;
		config.OnLoadValues.RemoveListener(CheckVoiceStop);
		config.OnLoadValues.AddListener(CheckVoiceStop);
	}

	private void UnsubscribeConfig()
	{
		if (config == null) return;
		config.OnLoadValues.RemoveListener(CheckVoiceStop);
	}

	private Button FindButton(params string[] names)
	{
		foreach (string name in names)
		{
			Transform target = FindChildRecursive(transform, name);
			if (target == null) continue;

			Button button = target.GetComponent<Button>() ?? target.GetComponentInChildren<Button>(true);
			if (button != null) return button;
		}
		return null;
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
