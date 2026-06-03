using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Dropdown")]
public class AccessibleDropdown : UAP_BaseElement
{
	[Header("Other")]
	public List<AudioClip> m_ValuesAsAudio;

	private int prevSelectedIndex;

	private AccessibleDropdown()
	{
	}

	private void Awake()
	{
	}

	private Dropdown GetDropdown()
	{
		return null;
	}

	private Component GetTMPDropDown()
	{
		return null;
	}

	public override bool IsInteractable()
	{
		return false;
	}

	public override bool IsElementActive()
	{
		return false;
	}

	public override string GetCurrentValueAsText()
	{
		return null;
	}

	public override AudioClip GetCurrentValueAsAudio()
	{
		return null;
	}

	protected override void OnInteract()
	{
	}

	protected override void OnInteractEnd()
	{
	}

	protected override void OnInteractAbort()
	{
	}

	public override bool Increment()
	{
		return false;
	}

	public override bool Decrement()
	{
		return false;
	}

	public int GetItemCount()
	{
		return 0;
	}

	public int GetSelectedItemIndex()
	{
		return 0;
	}

	protected override void OnHoverHighlight(bool enable)
	{
	}
}
