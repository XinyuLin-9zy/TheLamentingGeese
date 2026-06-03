using UnityEngine;

public abstract class UAP_BaseElement_3D : UAP_BaseElement
{
	public Camera m_CameraRenderingThisObject;

	public override bool AutoFillTextLabel()
	{
		return false;
	}

	public override bool Is3DElement()
	{
		return false;
	}

	public float GetPixelHeight()
	{
		return 0f;
	}

	public float GetPixelWidth()
	{
		return 0f;
	}

	public override void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{
	}

	protected override string GetLabelText(GameObject go)
	{
		return null;
	}
}
