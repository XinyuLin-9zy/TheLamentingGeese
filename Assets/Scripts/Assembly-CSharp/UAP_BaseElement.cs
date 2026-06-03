using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class UAP_BaseElement : MonoBehaviour
{
	[Serializable]
	public class UAPBoolCallback : UnityEvent<bool>
	{
	}

	public enum EHighlightSource
	{
		Internal = 0,
		UserInput = 1,
		TouchExplore = 2
	}

	[Serializable]
	public class UAPHighlightCallback : UnityEvent<bool, EHighlightSource>
	{
	}

	public UnityEvent m_OnInteractionStart;

	public UnityEvent m_OnInteractionEnd;

	public UnityEvent m_OnInteractionAbort;

	public bool m_ForceStartHere;

	public int m_ManualPositionOrder;

	public GameObject m_ManualPositionParent;

	public bool m_UseTargetForOutline;

	public int m_PositionOrder;

	public int m_SecondaryOrder;

	public Vector2 m_Pos;

	[Header("Element Name")]
	public AudioClip m_TextAsAudio;

	public string m_Prefix;

	public bool m_PrefixIsLocalizationKey;

	public bool m_PrefixIsPostFix;

	public bool m_FilterText;

	public string m_Text;

	public GameObject m_NameLabel;

	public List<GameObject> m_AdditionalNameLabels;

	public GameObject[] m_TestList;

	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey;

	public bool m_TryToReadLabel;

	public GameObject m_ReferenceElement;

	public bool m_AllowVoiceOver;

	public bool m_ReadType;

	[HideInInspector]
	public bool m_WasJustAdded;

	[HideInInspector]
	public AccessibleUIGroupRoot.EUIElement m_Type;

	private AccessibleUIGroupRoot AUIContainer;

	public bool m_CustomHint;

	public AudioClip m_HintAsAudio;

	public string m_Hint;

	public bool m_HintIsLocalizationKey;

	[HideInInspector]
	public bool m_IsInsideScrollView;

	private bool m_HasStarted;

	[HideInInspector]
	public bool m_IsInitialized;

	public UAPHighlightCallback m_CallbackOnHighlight;

	private void Reset()
	{
	}

	public void Initialize()
	{
	}

	protected virtual void AutoInitialize()
	{
	}

	private void OnEnable()
	{
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void GetContainer()
	{
	}

	internal AccessibleUIGroupRoot GetUIGroupContainer()
	{
		return null;
	}

	private void RegisterWithContainer()
	{
	}

	public void SetAsStartItem()
	{
	}

	private void RefreshContainerNextFrame()
	{
	}

	private void LogErrorNoValidParent()
	{
	}

	public virtual bool Is3DElement()
	{
		return false;
	}

	public virtual bool AutoFillTextLabel()
	{
		return false;
	}

	private void OnDestroy()
	{
	}

	public virtual bool IsInteractable()
	{
		return false;
	}

	public void Interact()
	{
	}

	protected virtual void OnInteract()
	{
	}

	public void InteractEnd()
	{
	}

	protected virtual void OnInteractEnd()
	{
	}

	public void InteractAbort()
	{
	}

	protected virtual void OnInteractAbort()
	{
	}

	protected string CombinePrefix(string text)
	{
		return null;
	}

	public static string FilterText(string text)
	{
		return null;
	}

	private static void RemoveSubsting(ref string text, string substring)
	{
	}

	public string GetTextToRead()
	{
		return null;
	}

	protected virtual string GetMainText()
	{
		return null;
	}

	public void SetCustomText(string itemText)
	{
	}

	public virtual string GetCurrentValueAsText()
	{
		return null;
	}

	public virtual AudioClip GetCurrentValueAsAudio()
	{
		return null;
	}

	public virtual bool IsElementActive()
	{
		return false;
	}

	public bool SelectItem(bool forceRepeatItem = false)
	{
		return false;
	}

	private bool SelectItem_Internal(bool forceRepeatItem)
	{
		return false;
	}

	public virtual bool Increment()
	{
		return false;
	}

	public virtual bool Decrement()
	{
		return false;
	}

	public virtual void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{
	}

	protected virtual void OnHoverHighlight(bool enable)
	{
	}

	public GameObject GetTargetGameObject()
	{
		return null;
	}

	public bool IsNameLocalizationKey()
	{
		return false;
	}

	public string GetCustomHint()
	{
		return null;
	}

	public void SetCustomHintText(string hintText, bool isLocalizationKey = false)
	{
	}

	public void ResetHintText()
	{
	}

	protected string GetTextFromTextMeshPro(Component textMeshProLabel)
	{
		return null;
	}

	protected virtual string GetLabelText(GameObject go)
	{
		return null;
	}

	protected Component GetTextMeshProLabelInChildren()
	{
		return null;
	}
}
