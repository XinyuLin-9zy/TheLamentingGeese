using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsTool
{
	public static string resWidth;

	public static string resHeight;

	private static int curWidth;

	private static int curHeight;

	private const uint SWP_SHOWWINDOW = 64u;

	private const int GWL_STYLE = -16;

	private const int WS_BORDER = 1;

	private const int WS_POPUP = 8388608;

	private const int SW_SHOWMINIMIZED = 2;

	private const int SWP_DRAWFRAME = 32;

	public static int CurWidthSetting
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public static int CurHeightSetting
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	[PreserveSig]
	private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

	[PreserveSig]
	private static extern IntPtr GetForegroundWindow();

	[PreserveSig]
	private static extern IntPtr SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

	[PreserveSig]
	private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[PreserveSig]
	private static extern bool ReleaseCapture();

	[PreserveSig]
	private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	public static void SetMinWindows()
	{
	}

	public static void SetNoFrameWindow(Rect rect, bool needFrame = false)
	{
	}

	public static void DragWindow(IntPtr window)
	{
	}
}
