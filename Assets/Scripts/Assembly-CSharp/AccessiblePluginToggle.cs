using UnityEngine;

[AddComponentMenu("Accessibility/UI/Accessible Plugin Toggle")]
public class AccessiblePluginToggle : AccessibleToggle
{
	public bool m_HandleActivation;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	public void OnToggleStateChanged(bool newState)
	{
	}

	public void AccessibilitiyPlugin_StateChanged(bool newEnabledState)
	{
	}

	private void UpdateToggleState()
	{
	}
}
