using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using Utage;
using ZeroPlay;

public class DeviceCenter : SingletonMono<DeviceCenter>, IMediator
{
	[CompilerGenerated]
	private sealed class _003CScrollDelay_003Ed__34 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action callback;

		private float _003Cclock_003E5__2;

		private float _003Ctime_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CScrollDelay_003Ed__34(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[SerializeField]
	protected AdvEngine engine;

	[SerializeField]
	private UAP_AccessibilityManager accessibilityManager;

	private PlayerControls controls;

	[SerializeField]
	private GamepadElement _selectingElement;

	private Coroutine coroutine;

	public AdvEngine Engine => null;

	public ControlState State { get; private set; }

	private UAP_AccessibilityManager AccessibilityManager => null;

	private PlayerControls Controls => null;

	private GamepadElement SelectingElement
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string MediatorName => null;

	public object ViewComponent { get; set; }

	public void Init()
	{
	}

	private void InitInputDeviceControl()
	{
	}

	public void HighlightSelectElement()
	{
	}

	private void InitGamepadControl()
	{
	}

	private void OnClickMoveUp(InputAction.CallbackContext obj)
	{
	}

	private void OnClickMoveDown(InputAction.CallbackContext obj)
	{
	}

	private void OnClickMoveLeft(InputAction.CallbackContext obj)
	{
	}

	private void OnClickMoveRight(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollUp(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollDown(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollLeft(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollRight(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollUpCancel(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollDownCancel(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollRightCancel(InputAction.CallbackContext obj)
	{
	}

	private void OnScrollLeftCancel(InputAction.CallbackContext obj)
	{
	}

	[IteratorStateMachine(typeof(_003CScrollDelay_003Ed__34))]
	private IEnumerator ScrollDelay(Action callback)
	{
		return null;
	}

	private void OnClick(InputAction.CallbackContext obj)
	{
	}

	private void OnBack(InputAction.CallbackContext obj)
	{
	}

	public void RegisterSelectingElement(GamepadElement target)
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
	}

	private void HideCursor()
	{
	}

	private void ShowCursor()
	{
	}

	private void OnSwitchToMouse()
	{
	}

	private void OnSwitchToTouchscreen()
	{
	}

	private void OnSwitchToKeyboard()
	{
	}

	private void OnSwitchToGamepad()
	{
	}

	private void OnSwitchToAccessibility()
	{
	}

	private void OnExitAccessibility()
	{
	}

	private void OnLanguageChange(string language, GameLanguageType type)
	{
	}

	public string[] ListNotificationInterests()
	{
		return null;
	}

	public void HandleNotification(INotification notification)
	{
	}

	public void OnRegister()
	{
	}

	public void OnRemove()
	{
	}
}
