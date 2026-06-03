using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

public class UI_DialogMsg : MonoBehaviour
{
	private const string MoneyParamName = "money";
	private const string IsMoneyParamName = "IsMoney";
	private const float FadeDuration = 0.2f;
	private const float OnceVisibleTime = 1.6f;

	[SerializeField]
	protected AdvEngine engine;

	[FormerlySerializedAs("moneyPanel")]
	[FormerlySerializedAs("useMoney")]
	public CanvasGroup useMoneyPanel;

	[FormerlySerializedAs("curMoney")]
	public CanvasGroup curMoneyPanel;

	public CanvasGroup collectVoice;

	public Text text_curMoney;

	public Text text_useMoney;

	public Text text_collectVoice;

	private Tween moneyShowTween;

	private Tween collectTween;

	private Tween moneyUseTween;

	private int lastMoney = int.MinValue;

	private bool lastMoneyVisible;

	private Coroutine moneyUseHideCoroutine;

	private Coroutine collectHideCoroutine;

	private bool rootLayoutGroupsDisabled;

	public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);

	private void Awake()
	{
		ResolveReferences();
		ApplyRuntimeLayout();
		RefreshPlaceholderText();
		SetPanelImmediate(useMoneyPanel, false);
		SetPanelImmediate(collectVoice, false);
		RefreshMoney();
		bool initialMoneyVisible = ReadBoolParam(IsMoneyParamName);
		SetPanelImmediate(curMoneyPanel, initialMoneyVisible);
		lastMoneyVisible = initialMoneyVisible;
	}

	private void OnEnable()
	{
		ResolveReferences();
		ApplyRuntimeLayout();
		RefreshPlaceholderText();
	}

	private void Update()
	{
		AdvEngine advEngine = Engine;
		if (advEngine == null || advEngine.Param == null) return;

		ApplyRuntimeLayout();
		int money = ReadMoney();
		bool shouldShowMoney = ReadBoolParam(IsMoneyParamName);
		if (money != lastMoney)
		{
			RefreshMoney();
		}
		if (shouldShowMoney != lastMoneyVisible)
		{
			SetMoneyShowState(shouldShowMoney);
		}
	}

	private void LateUpdate()
	{
		ApplyRuntimeLayout();
	}

	private void OnDestroy()
	{
		KillTween(moneyShowTween);
		KillTween(collectTween);
		KillTween(moneyUseTween);
		if (moneyUseHideCoroutine != null) StopCoroutine(moneyUseHideCoroutine);
		if (collectHideCoroutine != null) StopCoroutine(collectHideCoroutine);
	}

	private void OnLanguageChange(string curLanguage, GameLanguageType type)
	{
		RefreshMoney();
	}

	private void RefreshMoney()
	{
		int money = ReadMoney();
		lastMoney = money;

		if (text_curMoney != null)
		{
			text_curMoney.text = string.Format("{0}: {1} {2}",
				Localize("text_money_current", "Silver remaining"),
				money,
				LocalizeMoneyUnit());
		}
	}

	public void SetMoneyShowState(bool isShow)
	{
		lastMoneyVisible = isShow;
		FadePanel(curMoneyPanel, isShow, ref moneyShowTween);
	}

	public void ShowMoney(MoneyShowState state, MoneyUseType useType = MoneyUseType.None, int count = 0)
	{
		RefreshMoney();

		switch (state)
		{
			case MoneyShowState.Show:
				SetBoolParam(IsMoneyParamName, true);
				SetMoneyShowState(true);
				return;
			case MoneyShowState.Hide:
				SetBoolParam(IsMoneyParamName, false);
				SetMoneyShowState(false);
				FadePanel(useMoneyPanel, false, ref moneyUseTween);
				return;
			case MoneyShowState.Once:
				ShowUseMoneyOnce(useType, count);
				return;
			case MoneyShowState.None:
			default:
				return;
		}
	}

	public void SetCollectVoiceState(bool isCollect)
	{
		if (text_collectVoice != null)
		{
			text_collectVoice.text = Localize(isCollect ? "voice_collect" : "voice_collect_remove",
				isCollect ? "Saved to favorites" : "Unfavorited");
		}

		ShowPanelOnce(collectVoice, ref collectTween, ref collectHideCoroutine, OnceVisibleTime);
	}

	public void OnUseMoney(int amount)
	{
		ShowMoney(MoneyShowState.Once, MoneyUseType.Use, amount);
	}

	public void OnGetMoney(int amount)
	{
		ShowMoney(MoneyShowState.Once, MoneyUseType.Get, amount);
	}

	public void OnLostMoney(int amount)
	{
		ShowMoney(MoneyShowState.Once, MoneyUseType.Lose, amount);
	}

	private void ResolveReferences()
	{
		if (useMoneyPanel == null) useMoneyPanel = FindOrCreateCanvasGroup("UseMoney");
		if (curMoneyPanel == null) curMoneyPanel = FindOrCreateCanvasGroup("CurMoney");
		if (collectVoice == null) collectVoice = FindOrCreateCanvasGroup("CollectVoice");

		if (text_curMoney == null && curMoneyPanel != null)
		{
			text_curMoney = curMoneyPanel.GetComponentInChildren<Text>(true);
		}
		if (text_useMoney == null && useMoneyPanel != null)
		{
			text_useMoney = useMoneyPanel.GetComponentInChildren<Text>(true);
		}
		if (text_collectVoice == null && collectVoice != null)
		{
			text_collectVoice = collectVoice.GetComponentInChildren<Text>(true);
		}
	}

	private CanvasGroup FindOrCreateCanvasGroup(string targetName)
	{
		Transform target = FindChildRecursive(transform, targetName);
		if (target == null) return null;

		CanvasGroup group = target.GetComponent<CanvasGroup>();
		if (group == null)
		{
			group = target.gameObject.AddComponent<CanvasGroup>();
		}
		return group;
	}

	private void ApplyRuntimeLayout()
	{
		DisableRootLayoutGroups();
		IgnoreLayout(curMoneyPanel);
		IgnoreLayout(useMoneyPanel);
		IgnoreLayout(collectVoice);
		SetBottomLeft(curMoneyPanel, new Vector2(32, 32), new Vector2(393, 47));
		SetBottomLeft(useMoneyPanel, new Vector2(32, 88), new Vector2(735, 97));
		SetBottomLeft(collectVoice, new Vector2(32, 144), new Vector2(320, 58));
	}

	private void DisableRootLayoutGroups()
	{
		if (rootLayoutGroupsDisabled) return;

		LayoutGroup[] layoutGroups = GetComponents<LayoutGroup>();
		foreach (LayoutGroup layoutGroup in layoutGroups)
		{
			if (layoutGroup != null) layoutGroup.enabled = false;
		}
		rootLayoutGroupsDisabled = true;
	}

	private void IgnoreLayout(CanvasGroup group)
	{
		if (group == null) return;

		LayoutElement layoutElement = group.GetComponent<LayoutElement>();
		if (layoutElement == null)
		{
			layoutElement = group.gameObject.AddComponent<LayoutElement>();
		}
		layoutElement.ignoreLayout = true;
	}

	private void SetBottomLeft(CanvasGroup group, Vector2 anchoredPosition, Vector2 size)
	{
		if (group == null) return;

		RectTransform rectTransform = group.transform as RectTransform;
		if (rectTransform == null) return;

		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.zero;
		rectTransform.pivot = Vector2.zero;
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = size;
	}

	private void ShowUseMoneyOnce(MoneyUseType useType, int count)
	{
		if (useMoneyPanel == null || text_useMoney == null) return;

		text_useMoney.text = BuildUseMoneyText(useType, count);
		ShowPanelOnce(useMoneyPanel, ref moneyUseTween, ref moneyUseHideCoroutine, OnceVisibleTime);
	}

	private string BuildUseMoneyText(MoneyUseType useType, int count)
	{
		string label;
		switch (useType)
		{
			case MoneyUseType.Get:
				label = Localize("text_money_get", "Get Silver");
				break;
			case MoneyUseType.Use:
				label = Localize("text_money_use", "Use Silver");
				break;
			case MoneyUseType.Lose:
				label = Localize("text_money_lost", "Lost Silver");
				break;
			case MoneyUseType.None:
			default:
				label = Localize("text_money_current", "Silver");
				break;
		}

		return string.Format("{0}: {1} {2}", label, Mathf.Abs(count), LocalizeMoneyUnit());
	}

	private int ReadMoney()
	{
		AdvEngine advEngine = Engine;
		if (advEngine == null || advEngine.Param == null) return 0;

		object value;
		if (advEngine.Param.TryGetParameter(MoneyParamName, out value))
		{
			if (value is int) return (int)value;
			if (value is float) return Mathf.RoundToInt((float)value);
		}

		return 0;
	}

	private bool ReadBoolParam(string key)
	{
		AdvEngine advEngine = Engine;
		if (advEngine == null || advEngine.Param == null) return false;

		object value;
		return advEngine.Param.TryGetParameter(key, out value) && value is bool && (bool)value;
	}

	private void SetBoolParam(string key, bool value)
	{
		AdvEngine advEngine = Engine;
		if (advEngine == null || advEngine.Param == null) return;
		advEngine.Param.TrySetParameter(key, value);
	}

	private void SetPanelImmediate(CanvasGroup group, bool isShow)
	{
		if (group == null) return;

		group.gameObject.SetActive(true);
		group.alpha = isShow ? 1 : 0;
		group.interactable = false;
		group.blocksRaycasts = false;
	}

	private void FadePanel(CanvasGroup group, bool isShow, ref Tween tween)
	{
		if (group == null) return;

		group.gameObject.SetActive(true);
		group.interactable = false;
		group.blocksRaycasts = false;
		KillTween(tween);
		tween = group.DOFade(isShow ? 1 : 0, FadeDuration).SetUpdate(true);
	}

	private void ShowPanelOnce(CanvasGroup group, ref Tween tween, ref Coroutine coroutine, float delay)
	{
		if (group == null) return;

		KillTween(tween);
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
		SetPanelImmediate(group, true);
		coroutine = StartCoroutine(CoHidePanelAfterDelay(group, delay));
	}

	private System.Collections.IEnumerator CoHidePanelAfterDelay(CanvasGroup group, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		if (group != null)
		{
			group.DOFade(0, FadeDuration).SetUpdate(true);
		}
	}

	private void KillTween(Tween tween)
	{
		if (tween != null)
		{
			tween.Kill();
		}
	}

	private string Localize(string key, string fallback)
	{
		if (LanguageManagerBase.Instance != null)
		{
			string text;
			if (LanguageManagerBase.Instance.TryLocalizeText(key, out text) && !string.IsNullOrEmpty(text))
			{
				return text;
			}
		}

		return fallback;
	}

	private string LocalizeMoneyUnit()
	{
		return Localize("tael", "两");
	}

	private void RefreshPlaceholderText()
	{
		if (text_curMoney != null && (string.IsNullOrEmpty(text_curMoney.text) || LooksCorrupted(text_curMoney.text)))
		{
			text_curMoney.text = "银两剩余: 0 两";
		}

		if (text_useMoney != null && (string.IsNullOrEmpty(text_useMoney.text) || LooksCorrupted(text_useMoney.text)))
		{
			text_useMoney.text = "使用银两: 0 两";
		}

		if (text_collectVoice != null && (string.IsNullOrEmpty(text_collectVoice.text) || LooksCorrupted(text_collectVoice.text)))
		{
			text_collectVoice.text = "已收藏语音";
		}
	}

	private static bool LooksCorrupted(string text)
	{
		if (string.IsNullOrEmpty(text)) return false;
		return text.IndexOf('�') >= 0 || text.IndexOf('ä') >= 0 || text.IndexOf('é') >= 0;
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
