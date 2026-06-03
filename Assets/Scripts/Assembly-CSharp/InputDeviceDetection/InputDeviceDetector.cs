using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;

namespace InputDeviceDetection
{
	public class InputDeviceDetector : MonoBehaviour
	{
		[Header("Options")]
		[SerializeField]
		private bool detectUIInputOnly;

		[SerializeField]
		private bool hideCursorAtBeginning;

		[Space(10f)]
		[SerializeField]
		private UnityEvent onSwitchToMouse;

		[SerializeField]
		private UnityEvent onSwitchToKeyboard;

		[SerializeField]
		private UnityEvent onSwitchToGamepad;

		[FormerlySerializedAs("onSwitchToTouchScreen")]
		[SerializeField]
		private UnityEvent onSwitchToTouchscreen;

		private Dictionary<InputDevice, UnityEvent> deviceSwitchTable;

		private InputDevice currentDevice;

		private Mouse mouse;

		private Keyboard keyboard;

		private Gamepad gamepad;

		private Touchscreen touchscreen;

		private InputSystemUIInputModule UIInputModule;

		private static InputDeviceDetector instance;

		private int curDeviceID;

		public static UnityEvent OnSwitchToMouse => null;

		public static UnityEvent OnSwitchToKeyboard => null;

		public static UnityEvent OnSwitchToGamepad => null;

		public static UnityEvent OnSwitchToTouchscreen => null;

		public static event Action<int> OnInputDevice
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
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

		public void ClearAllDeviceSwitchEvents()
		{
		}

		private void DetectCurrentInputDevice(object obj, InputActionChange change)
		{
		}

		public static void OnExitAccessibilityMode()
		{
		}

		public static void ShowCursor()
		{
		}

		public static void HideCursor()
		{
		}
	}
}
