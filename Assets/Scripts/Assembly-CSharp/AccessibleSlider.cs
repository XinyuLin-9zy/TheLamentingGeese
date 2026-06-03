using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Slider")]
public class AccessibleSlider : UAP_BaseElement
{
	public bool m_ReadPercentages;

	public float m_Increments;

	public bool m_IncrementInPercent;

	public bool m_WholeNumbersOnly;

	private AccessibleSlider()
	{
	}

	public override bool IsElementActive()
	{
		return false;
	}

	public override bool IsInteractable()
	{
		return false;
	}

	private Slider GetSlider()
	{
		return null;
	}

	public override string GetCurrentValueAsText()
	{
		return null;
	}

	public override bool Increment()
	{
		return false;
	}

	public override bool Decrement()
	{
		return false;
	}

	private void ModifySliderValue(float change)
	{
	}

	protected override void OnHoverHighlight(bool enable)
	{
	}

	public override bool AutoFillTextLabel()
	{
		return false;
	}
}
