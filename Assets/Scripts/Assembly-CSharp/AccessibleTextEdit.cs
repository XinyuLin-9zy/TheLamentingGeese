using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Text Edit")]
public class AccessibleTextEdit : UAP_BaseElement
{
	private string prevText;

	private string deltaText;

	private AccessibleTextEdit()
	{
	}

	public override bool IsElementActive()
	{
		return false;
	}

	private InputField GetInputField()
	{
		return null;
	}

	private Component GetTMPInputField()
	{
		return null;
	}

	public override string GetCurrentValueAsText()
	{
		return null;
	}

	private string GetValueFromEditBox()
	{
		return null;
	}

	private bool IsPassword()
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

	private void OnInputFinished(string editedText, bool wasConfirmed)
	{
	}

	public void ValueChangeCheck()
	{
	}

	protected override void OnInteractAbort()
	{
	}

	protected override void OnInteractEnd()
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
