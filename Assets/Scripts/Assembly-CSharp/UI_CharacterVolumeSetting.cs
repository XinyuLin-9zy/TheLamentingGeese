using UnityEngine;
using UnityEngine.UI;
using Utage;

public class UI_CharacterVolumeSetting : MonoBehaviour
{
	public UtageUguiConfig parent;

	public Slider sliderChaVolume1;

	public Slider sliderChaVolume2;

	public Slider sliderChaVolume3;

	public Slider sliderChaVolume4;

	public Slider sliderChaVolume5;

	public Slider sliderChaVolume6;

	public Slider sliderChaVolume7;

	public Slider sliderChaVolume8;

	public Slider sliderChaVolume9;

	public UtageUguiConfigTaggedMasterVolume tag1;

	public UtageUguiConfigTaggedMasterVolume tag2;

	public UtageUguiConfigTaggedMasterVolume tag3;

	public UtageUguiConfigTaggedMasterVolume tag4;

	public UtageUguiConfigTaggedMasterVolume tag5;

	public UtageUguiConfigTaggedMasterVolume tag6;

	public UtageUguiConfigTaggedMasterVolume tag7;

	public UtageUguiConfigTaggedMasterVolume tag8;

	public UtageUguiConfigTaggedMasterVolume tag9;

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		Refresh();
	}

	private void OnEnable()
	{
		Initialize();
		SubscribeConfig();
		Refresh();
	}

	private void OnDisable()
	{
		UnsubscribeConfig();
	}

	public void Refresh()
	{
		Initialize();
		AdvConfig advConfig = parent != null && parent.Engine != null ? parent.Engine.Config : null;

		UtageUguiConfigTaggedMasterVolume[] tags = GetTags();
		Slider[] sliders = GetSliders();
		for (int i = 0; i < tags.Length && i < sliders.Length; ++i)
		{
			UtageUguiConfigTaggedMasterVolume tag = tags[i];
			Slider slider = sliders[i];
			if (tag == null || slider == null) continue;

			if (tag.config == null) tag.config = parent;
			if (advConfig == null || string.IsNullOrEmpty(tag.volumeTag)) continue;

			float volume;
			if (advConfig.TryGetTaggedMasterVolume(tag.volumeTag, out volume))
			{
				slider.SetValueWithoutNotify(volume);
			}
		}
	}

	public void ResetVolumeSetting()
	{
		Initialize();
		UtageUguiConfigTaggedMasterVolume[] tags = GetTags();
		Slider[] sliders = GetSliders();

		for (int i = 0; i < tags.Length && i < sliders.Length; ++i)
		{
			UtageUguiConfigTaggedMasterVolume tag = tags[i];
			Slider slider = sliders[i];
			if (tag == null || slider == null) continue;

			slider.value = 1f;
			tag.OnValugeChanged(1f);
		}
		Refresh();
	}

	public void Open()
	{
		gameObject.SetActive(true);
		Refresh();
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}

	private Button openButton;
	private Button closeButton;
	private bool isInitialized;

	private void Initialize()
	{
		if (isInitialized) return;

		if (parent == null)
		{
			parent = GetComponentInParent<UtageUguiConfig>(true);
		}

		ResolveTags();
		ResolveSliders();
		BindSliders();
		BindButtons();

		isInitialized = true;
	}

	private void ResolveTags()
	{
		UtageUguiConfigTaggedMasterVolume[] found = GetComponentsInChildren<UtageUguiConfigTaggedMasterVolume>(true);
		tag1 = tag1 ?? FindTag(found, "SuLianyan 1", 0);
		tag2 = tag2 ?? FindTag(found, "SuLianyan 2", 1);
		tag3 = tag3 ?? FindTag(found, "LinPianpian", 2);
		tag4 = tag4 ?? FindTag(found, "Qionghua", 3);
		tag5 = tag5 ?? FindTag(found, "FangZhiyou", 4);
		tag6 = tag6 ?? FindTag(found, "Sui", 5);
		tag7 = tag7 ?? FindTag(found, "Liang", 6);
		tag8 = tag8 ?? FindTag(found, "Other", 8);
		tag9 = tag9 ?? FindTag(found, "WangSheng", 7);

		foreach (UtageUguiConfigTaggedMasterVolume tag in GetTags())
		{
			if (tag != null && tag.config == null)
			{
				tag.config = parent;
			}
		}
	}

	private void ResolveSliders()
	{
		if (sliderChaVolume1 == null) sliderChaVolume1 = FindSlider(tag1);
		if (sliderChaVolume2 == null) sliderChaVolume2 = FindSlider(tag2);
		if (sliderChaVolume3 == null) sliderChaVolume3 = FindSlider(tag3);
		if (sliderChaVolume4 == null) sliderChaVolume4 = FindSlider(tag4);
		if (sliderChaVolume5 == null) sliderChaVolume5 = FindSlider(tag5);
		if (sliderChaVolume6 == null) sliderChaVolume6 = FindSlider(tag6);
		if (sliderChaVolume7 == null) sliderChaVolume7 = FindSlider(tag7);
		if (sliderChaVolume8 == null) sliderChaVolume8 = FindSlider(tag8);
		if (sliderChaVolume9 == null) sliderChaVolume9 = FindSlider(tag9);
	}

	private void BindSliders()
	{
		UtageUguiConfigTaggedMasterVolume[] tags = GetTags();
		Slider[] sliders = GetSliders();
		for (int i = 0; i < tags.Length && i < sliders.Length; ++i)
		{
			UtageUguiConfigTaggedMasterVolume tag = tags[i];
			Slider slider = sliders[i];
			if (tag == null || slider == null) continue;

			slider.onValueChanged.RemoveListener(tag.OnValugeChanged);
			slider.onValueChanged.AddListener(tag.OnValugeChanged);
		}
	}

	private void BindButtons()
	{
		if (closeButton == null) closeButton = FindButton(transform, "CloseBtn", "CloseButton", "ButtonBack");
		Bind(closeButton, Close);

		if (openButton == null)
		{
			Transform searchRoot = parent != null ? parent.transform : transform.parent;
			openButton = FindButton(searchRoot, "OpenCharacterVolumeSetting", "CharacterVolumeSetting");
		}
		Bind(openButton, Open);
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

	private void SubscribeConfig()
	{
		if (parent == null) return;
		parent.OnLoadValues.RemoveListener(Refresh);
		parent.OnLoadValues.AddListener(Refresh);
	}

	private void UnsubscribeConfig()
	{
		if (parent == null) return;
		parent.OnLoadValues.RemoveListener(Refresh);
	}

	private UtageUguiConfigTaggedMasterVolume FindTag(UtageUguiConfigTaggedMasterVolume[] tags, string name, int fallbackIndex)
	{
		if (tags == null) return null;
		foreach (UtageUguiConfigTaggedMasterVolume tag in tags)
		{
			if (tag != null && tag.name == name)
			{
				return tag;
			}
		}
		return fallbackIndex >= 0 && fallbackIndex < tags.Length ? tags[fallbackIndex] : null;
	}

	private Slider FindSlider(UtageUguiConfigTaggedMasterVolume tag)
	{
		return tag != null ? tag.GetComponentInChildren<Slider>(true) : null;
	}

	private Button FindButton(Transform root, params string[] names)
	{
		if (root == null) return null;
		foreach (string name in names)
		{
			Transform target = FindChildRecursive(root, name);
			if (target == null) continue;

			Button button = target.GetComponent<Button>() ?? target.GetComponentInChildren<Button>(true);
			if (button != null) return button;
		}
		return null;
	}

	private UtageUguiConfigTaggedMasterVolume[] GetTags()
	{
		return new[]
		{
			tag1, tag2, tag3, tag4, tag5, tag6, tag7, tag8, tag9
		};
	}

	private Slider[] GetSliders()
	{
		return new[]
		{
			sliderChaVolume1,
			sliderChaVolume2,
			sliderChaVolume3,
			sliderChaVolume4,
			sliderChaVolume5,
			sliderChaVolume6,
			sliderChaVolume7,
			sliderChaVolume8,
			sliderChaVolume9
		};
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
