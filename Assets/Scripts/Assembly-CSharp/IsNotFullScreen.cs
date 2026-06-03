using UnityEngine;
using UnityEngine.UI;
using Utage;

public class IsNotFullScreen : MonoBehaviour
{
	public UtageUguiConfig config;

	public Button full;

	public Button window;

	public Button noFrame;

	private Transform noFrameRoot;

	private Transform noFrameTemplate;

	private Text noFrameLabel;

	private int screenWidth;

	private int screenHeight;

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
		CheckFullscreen();
	}

	private void OnEnable()
	{
		Initialize();
		SubscribeConfig();
		CheckFullscreen();
	}

	private void OnDisable()
	{
		UnsubscribeConfig();
	}

	public void CheckFullscreen()
	{
		Initialize();
		AdvEngine engine = config != null ? config.Engine : null;
		if (engine == null) return;

		bool isFullScreen = engine.ScreenResolution.IsFullScreen;
		SetSelected(full, isFullScreen);
		SetSelected(window, !isFullScreen);
		SetSelected(noFrame, false);
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		HideNoFrameTemplate();
		UpdateNoFrameLabel();
	}

	private void Initialize()
	{
		if (isInitialized) return;

		if (config == null)
		{
			config = GetComponentInParent<UtageUguiConfig>(true);
		}

		if (full == null) full = FindButton("FullScreen", "Fullscreen", "Full");
		if (window == null) window = FindButton("Window", "Windowed");
		if (noFrameRoot == null)
		{
			noFrameRoot = FindChildRecursive(transform, "NoFrame")
				?? FindChildRecursive(transform, "Borderless")
				?? FindChildRecursive(transform, "Dropdown");
		}
		if (noFrame == null)
		{
			noFrame = noFrameRoot != null ? noFrameRoot.GetComponent<Button>() : FindButton("NoFrame", "Borderless", "Dropdown");
			if (noFrame == null && noFrameRoot != null && noFrameRoot.GetComponent<Selectable>() == null)
			{
				noFrame = noFrameRoot.gameObject.AddComponent<Button>();
			}
		}
		if (noFrameRoot != null)
		{
			Dropdown dropdown = noFrameRoot.GetComponent<Dropdown>();
			if (dropdown != null)
			{
				dropdown.Hide();
				dropdown.enabled = false;
			}
			noFrameTemplate = FindChildRecursive(noFrameRoot, "Template");
			Transform label = FindChildRecursive(noFrameRoot, "Label");
			if (label != null) noFrameLabel = label.GetComponent<Text>();
			HideNoFrameTemplate();
			UpdateNoFrameLabel();
		}

		Bind(full, () => SetFullScreen(true));
		Bind(window, () => SetFullScreen(false));
		Bind(noFrame, () => SetFullScreen(false));

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

	private void SetFullScreen(bool value)
	{
		AdvEngine engine = config != null ? config.Engine : null;
		if (engine == null) return;

		engine.ScreenResolution.IsFullScreen = value;
		CheckFullscreen();
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

	private void HideNoFrameTemplate()
	{
		if (noFrameTemplate == null && noFrameRoot != null)
		{
			noFrameTemplate = FindChildRecursive(noFrameRoot, "Template");
		}
		if (noFrameTemplate != null && noFrameTemplate.gameObject.activeSelf)
		{
			noFrameTemplate.gameObject.SetActive(false);
		}
	}

	private void UpdateNoFrameLabel()
	{
		if (noFrameLabel == null) return;
		noFrameLabel.text = string.Format("{0} x {1}", Screen.width, Screen.height);
	}

	private void SubscribeConfig()
	{
		if (config == null) return;
		config.OnLoadValues.RemoveListener(CheckFullscreen);
		config.OnLoadValues.AddListener(CheckFullscreen);
	}

	private void UnsubscribeConfig()
	{
		if (config == null) return;
		config.OnLoadValues.RemoveListener(CheckFullscreen);
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
