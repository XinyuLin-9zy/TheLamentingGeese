using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Label")]
public class AccessibleLabel : UAP_BaseElement
{
	private AccessibleLabel()
	{
	}

	public override bool IsElementActive()
	{
		return false;
	}

	public override bool AutoFillTextLabel()
	{
		return false;
	}

	protected Component GetTextMeshLabel()
	{
		return null;
	}

	protected override string GetMainText()
	{
		return null;
	}

	private Text GetLabel()
	{
		return null;
	}

	protected override void AutoInitialize()
	{
	}
}
