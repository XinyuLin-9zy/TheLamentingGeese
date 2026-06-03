using UnityEngine;
using UnityEngine.UI;
using Utage;
using UI;
using Config;

public class CustomCommander : AdvCustomCommandManager
{
	public Canvas titleAnimationRoot;

	public GameObject titleAnimationPrefab;

	public TitleAnimationScaleData titleAnimationScaleData;

	public GameObject screenTextPrefab;

	public GameObject staffAnimationPrefab;

	public override void OnBootInit()
	{
		AdvCommandParser.OnCreateCustomCommandFromID += CreatPlotMapCommand;
	}

	private void OnDestroy()
	{
		AdvCommandParser.OnCreateCustomCommandFromID -= CreatPlotMapCommand;
	}

	private void CreatPlotMapCommand(string id, StringGridRow row, AdvSettingDataManager datamanager, ref AdvCommand command)
	{
		switch (id)
		{
			case "AddSteamAchiKey":
				command = new AddSteamAchiKey(row);
				break;
			case "AutoSave":
				command = new AutoSaveCommand(row);
				break;
			case "ChapterAnimation":
				command = new TitleAnimationCommand(row);
				break;
			case "GetMoney":
				command = new AdvCommandGetMoney(row);
				break;
			case "MoneyCmd":
				command = new MoneyControlCommand(row);
				break;
			case "RemainMoney":
				command = new AdvCommandRemianMoney(row);
				break;
			case "ScreenText":
				command = new ScreenTextCommand(row);
				break;
			case "SetChapterName":
				command = new ChapterCommand(row);
				break;
			case "ShowStaffAnimation":
				command = new StaffAnimationCommand(row);
				break;
			case "UnlockPlotMap":
				command = new UnLockMapCommand(row);
				break;
			case "UseMoney":
				command = new AdvCommandUseMoney(row);
				break;
		}
	}

	public override void OnClear()
	{
		AdvCommandParser.OnCreateCustomCommandFromID -= CreatPlotMapCommand;
	}

	public UI_TitleAnimation ShowTitleAnimation(Sprite titleBg, Sprite chapterBg, Sprite titleText, TitleAnimationType type, bool isSpeedUp, int bgOffset, float scale)
	{
		Canvas root = titleAnimationRoot;
		if (root == null)
		{
			GameObject rootObject = GameObject.Find("Canvas-ChapterTitle");
			if (rootObject != null)
			{
				root = rootObject.GetComponent<Canvas>();
			}
		}
		if (root == null) return null;

		root.gameObject.SetActive(true);
		root.enabled = true;
		root.transform.localScale = Vector3.one;

		GameObject go = titleAnimationPrefab != null
			? GameObject.Instantiate(titleAnimationPrefab, root.transform, false)
			: CreateFallbackTitleAnimation(root.transform);
		go.SetActive(true);
		StretchToParent(go.transform as RectTransform);

		UI_TitleAnimation animation = go.GetComponent<UI_TitleAnimation>();
		if (animation == null)
		{
			animation = go.AddComponent<UI_TitleAnimation>();
		}
		animation.SetInfo(titleBg, chapterBg, titleText, type, isSpeedUp, bgOffset, scale);
		return animation;
	}

	public float GetTitleAnimationScale(string titleName)
	{
		TitleAnimationScaleData scaleData = GetTitleAnimationScaleData();
		if (scaleData == null || scaleData.allTitleAnimationData == null || string.IsNullOrEmpty(titleName))
		{
			return 1f;
		}

		TitleRateData titleRateData;
		if (!scaleData.allTitleAnimationData.TryGetValue(titleName, out titleRateData) || titleRateData == null || titleRateData.LanguageTitleRateList == null)
		{
			return 1f;
		}

		LanguageType languageType = GetCurrentTitleLanguageType();
		foreach (LanguageTitleRateData languageRateData in titleRateData.LanguageTitleRateList)
		{
			if (languageRateData != null && languageRateData.languageType == languageType && languageRateData.rate > 0)
			{
				return languageRateData.rate;
			}
		}

		return 1f;
	}

	public UI_ScreenText ShowScreenText(string content, string textColor, float fadeInTime, float waitTime, float fadeOutTime, float fontSize = 150f, int fontIndex = 1)
	{
		Canvas root = ResolveOverlayCanvas();
		if (root == null || screenTextPrefab == null) return null;

		GameObject go = GameObject.Instantiate(screenTextPrefab, root.transform, false);
		StretchToParent(go.transform as RectTransform);

		UI_ScreenText screenText = go.GetComponent<UI_ScreenText>();
		if (screenText == null)
		{
			screenText = go.AddComponent<UI_ScreenText>();
		}
		screenText.SetInfo(content, textColor, fadeInTime, waitTime, fadeOutTime, fontSize, fontIndex);
		return screenText;
	}

	public UI_Staff ShowStaffAnimation()
	{
		Canvas root = ResolveOverlayCanvas();
		if (root == null || staffAnimationPrefab == null) return null;

		GameObject go = GameObject.Instantiate(staffAnimationPrefab, root.transform, false);
		StretchToParent(go.transform as RectTransform);

		UI_Staff staff = go.GetComponent<UI_Staff>();
		if (staff == null)
		{
			staff = go.AddComponent<UI_Staff>();
		}
		staff.SetInfo();
		return staff;
	}

	private GameObject CreateFallbackTitleAnimation(Transform parent)
	{
		GameObject root = new GameObject("UI_ChapterTitle", typeof(RectTransform), typeof(CanvasGroup), typeof(UI_TitleAnimation));
		root.transform.SetParent(parent, false);
		StretchToParent(root.transform as RectTransform);

		CreateImageChild(root.transform, "BG", Color.white);
		CreateImageChild(root.transform, "TitleBG", Color.white);
		CreateImageChild(root.transform, "Title", Color.white);
		CreateImageChild(root.transform, "Shelter", Color.black);
		return root;
	}

	private TitleAnimationScaleData GetTitleAnimationScaleData()
	{
		if (titleAnimationScaleData != null)
		{
			return titleAnimationScaleData;
		}

#if UNITY_EDITOR
		titleAnimationScaleData = UnityEditor.AssetDatabase.LoadAssetAtPath<TitleAnimationScaleData>("Assets/MonoBehaviour/TitleAnimationScaleData.asset");
#endif
		return titleAnimationScaleData;
	}

	private LanguageType GetCurrentTitleLanguageType()
	{
		string current = LanguageManagerBase.Instance != null ? LanguageManagerBase.Instance.CurrentLanguage : "";
		if (string.IsNullOrEmpty(current))
		{
			return LanguageType.SC;
		}

		string lower = current.ToLowerInvariant();
		if (lower.Contains("tc") || lower.Contains("traditional"))
		{
			return LanguageType.TC;
		}
		if (lower.Contains("english") || lower == "en" || lower.Contains("russian") || lower == "ru")
		{
			return LanguageType.English;
		}
		if (lower.Contains("japanese") || lower == "ja")
		{
			return LanguageType.Japanese;
		}
		return LanguageType.SC;
	}

	private void CreateImageChild(Transform parent, string name, Color color)
	{
		GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		child.transform.SetParent(parent, false);
		StretchToParent(child.transform as RectTransform);
		Image image = child.GetComponent<Image>();
		image.color = color;
		image.raycastTarget = false;
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

	private Canvas ResolveOverlayCanvas()
	{
		Canvas root = titleAnimationRoot;
		if (root != null)
		{
			root.gameObject.SetActive(true);
			root.enabled = true;
			root.transform.localScale = Vector3.one;
			return root;
		}

		GameObject rootObject = GameObject.Find("Canvas-AdvUI");
		if (rootObject == null)
		{
			Canvas[] canvases = FindObjectsOfType<Canvas>(true);
			foreach (Canvas canvas in canvases)
			{
				if (canvas != null && canvas.gameObject.scene.IsValid())
				{
					rootObject = canvas.gameObject;
					break;
				}
			}
		}

		if (rootObject != null)
		{
			root = rootObject.GetComponent<Canvas>();
		}

		if (root == null)
		{
			GameObject fallback = new GameObject("Canvas-Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			root = fallback.GetComponent<Canvas>();
			root.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		if (root != null)
		{
			root.gameObject.SetActive(true);
			root.enabled = true;
			root.transform.localScale = Vector3.one;
		}

		return root;
	}
}
