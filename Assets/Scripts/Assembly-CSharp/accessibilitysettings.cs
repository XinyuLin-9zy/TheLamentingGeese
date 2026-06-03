using UnityEngine;
using UnityEngine.UI;

public class accessibilitysettings : MonoBehaviour
{
	public Toggle m_EnableAccessibility;

	public Slider m_SpeechRateSlider;

	public GameObject m_AccessibilityConfirmation;

	public void OnEnable()
	{
	}

	public void OnAccessibilityEnabledToggleChanged(bool newValue)
	{
	}

	public void OnSpeechRateSliderChanged()
	{
	}

	public void OnEnableCancel()
	{
	}

	public void OnEnableConfirm()
	{
	}

	public void OnCloseSettings()
	{
	}
}
