using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Toggle")]
public class AccessibleToggle : UAP_BaseElement
{
	public bool m_UseCustomOnOff;

	public string m_CustomOn;

	public string m_CustomOff;

	public bool m_CustomHintsAreLocalizationKeys;

	public AudioClip m_CustomOnAudio;

	public AudioClip m_CustomOffAudio;

	public override bool IsElementActive()
	{
		return false;
	}

	public override bool IsInteractable()
	{
		return false;
	}

	protected override void OnInteract()
	{
	}

	protected Toggle GetToggle()
	{
		return null;
	}

	public override string GetCurrentValueAsText()
	{
		return null;
	}

	public bool IsChecked()
	{
		return false;
	}

	public void SetToggleState(bool toggleState)
	{
	}

	public override AudioClip GetCurrentValueAsAudio()
	{
		return null;
	}

	public override bool AutoFillTextLabel()
	{
		return false;
	}

	protected override void OnHoverHighlight(bool enable)
	{
	}

	protected override void AutoInitialize()
	{
	}
}
