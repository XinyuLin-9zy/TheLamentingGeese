using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerControls : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct UIActions
	{
		private PlayerControls m_Wrapper;

		public InputAction Click => null;

		public InputAction Back => null;

		public InputAction SelectMoveUp => null;

		public InputAction SelectMoveDown => null;

		public InputAction SelectMoveLeft => null;

		public InputAction SelectMoveRight => null;

		public InputAction ScrollUp => null;

		public InputAction ScrollDown => null;

		public InputAction ScrollRight => null;

		public InputAction ScrollLeft => null;

		public bool enabled => false;

		public UIActions(PlayerControls wrapper)
		{
			m_Wrapper = null;
		}

		public InputActionMap Get()
		{
			return null;
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return null;
		}

		public void AddCallbacks(IUIActions instance)
		{
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
		}

		public void RemoveCallbacks(IUIActions instance)
		{
		}

		public void SetCallbacks(IUIActions instance)
		{
		}
	}

	public interface IUIActions
	{
		void OnClick(InputAction.CallbackContext context);

		void OnBack(InputAction.CallbackContext context);

		void OnSelectMoveUp(InputAction.CallbackContext context);

		void OnSelectMoveDown(InputAction.CallbackContext context);

		void OnSelectMoveLeft(InputAction.CallbackContext context);

		void OnSelectMoveRight(InputAction.CallbackContext context);

		void OnScrollUp(InputAction.CallbackContext context);

		void OnScrollDown(InputAction.CallbackContext context);

		void OnScrollRight(InputAction.CallbackContext context);

		void OnScrollLeft(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_Back;

	private readonly InputAction m_UI_SelectMoveUp;

	private readonly InputAction m_UI_SelectMoveDown;

	private readonly InputAction m_UI_SelectMoveLeft;

	private readonly InputAction m_UI_SelectMoveRight;

	private readonly InputAction m_UI_ScrollUp;

	private readonly InputAction m_UI_ScrollDown;

	private readonly InputAction m_UI_ScrollRight;

	private readonly InputAction m_UI_ScrollLeft;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => default(ReadOnlyArray<InputControlScheme>);

	public IEnumerable<InputBinding> bindings => null;

	public UIActions UI => default(UIActions);

	~PlayerControls()
	{
	}

	public void Dispose()
	{
	}

	public bool Contains(InputAction action)
	{
		return false;
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}

	public void Enable()
	{
	}

	public void Disable()
	{
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return null;
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		action = null;
		return 0;
	}
}
