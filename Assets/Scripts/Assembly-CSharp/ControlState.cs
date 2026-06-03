using UnityEngine;

public enum ControlState
{
	None = 0,
	[InspectorName("鼠标")]
	Mouse = 1,
	[InspectorName("触摸屏")]
	Touchscreen = 2,
	[InspectorName("键盘")]
	Keyboard = 3,
	[InspectorName("手柄")]
	Gamepad = 4,
	[InspectorName("无障碍")]
	Accessibility = 5
}
