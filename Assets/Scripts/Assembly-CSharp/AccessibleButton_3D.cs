using UnityEngine;

[AddComponentMenu("Accessibility/UI/Accessible Button 3D")]
public class AccessibleButton_3D : UAP_BaseElement_3D
{
	private AccessibleButton_3D()
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
}
