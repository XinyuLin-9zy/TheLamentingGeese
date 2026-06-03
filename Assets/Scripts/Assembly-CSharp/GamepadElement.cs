using System;
using UnityEngine;

public class GamepadElement : MonoBehaviour
{
	public GamepadRoot parent;

	public GamepadElementType elementType;

	public GamepadElementDetailType detailType;

	[SerializeField]
	private GamepadElement up;

	[SerializeField]
	private GamepadElement down;

	[SerializeField]
	private GamepadElement left;

	[SerializeField]
	private GamepadElement right;

	public Action<GamepadElement> OnInit;

	public Action<GamepadElement> OnSelecting;

	public Action<GamepadElement> OnDeSelecting;

	public Action<GamepadElement> OnPressed;

	public Action<GamepadElement> OnDePressed;

	public Transform linkTarget;

	public bool TryGetNavigationElement(GamepadNavigationType type, out GamepadElement target)
	{
		target = null;
		return false;
	}

	public void SetNavigation(GamepadElement up, GamepadElement down, GamepadElement left, GamepadElement right)
	{
	}

	public void SetNavigation(GamepadNavigationType direction, GamepadElement target)
	{
	}
}
