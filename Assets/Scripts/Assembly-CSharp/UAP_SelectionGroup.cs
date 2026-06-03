using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Accessibility/Helper/Group Focus Notification")]
public class UAP_SelectionGroup : MonoBehaviour
{
	private List<UAP_BaseElement> m_AllElements;

	private bool m_Selected;

	private GameObject m_LastFocusObject;

	public void AddElement(UAP_BaseElement element)
	{
	}

	public void RemoveElement(UAP_BaseElement element)
	{
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}
}
