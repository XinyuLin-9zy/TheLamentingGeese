using UnityEngine;

[AddComponentMenu("Accessibility/Helper/GameObject Enabler")]
public class UAP_GameObjectEnabler : MonoBehaviour
{
	public GameObject[] m_ObjectsToEnable;

	public GameObject[] m_ObjectsToDisable;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void SetActiveState()
	{
	}

	private void OnDisable()
	{
	}

	public void Accessibility_StateChange(bool newState)
	{
	}
}
