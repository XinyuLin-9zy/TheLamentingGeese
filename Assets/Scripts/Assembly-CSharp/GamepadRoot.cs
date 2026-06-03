using System;
using UnityEngine;

public class GamepadRoot : MonoBehaviour
{
	public bool autoRegister;

	public bool allowBack;

	public bool isUtageBase;

	public bool isHide;

	public GameObject linkGameObject;

	public long index;

	[SerializeField]
	private GamepadElement defaultElement;

	[SerializeField]
	private GamepadElement selectingElement;

	public GamepadElement[] elements;

	public Action OnBack;

	public GamepadElement SelectingElement
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void SearchAllElement()
	{
	}

	public void InitElement()
	{
	}

	public void OnPressElement(GamepadElement targetElement)
	{
	}

	public void ResetAllData()
	{
	}

	public void ResetAllData(GamepadElement selectedElement)
	{
	}
}
