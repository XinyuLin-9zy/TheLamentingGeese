using UnityEngine;

namespace ZeroPlay
{
	public class FrameShower : MonoBehaviour
	{
		[SerializeField]
		private bool showFPS;

		[SerializeField]
		private float updateInterval;

		[SerializeField]
		private int fontSize;

		[SerializeField]
		private Color fontColor;

		[SerializeField]
		private int margin;

		[SerializeField]
		private TextAnchor alignment;

		private GUIStyle guiStyle;

		private Rect rect;

		private int frames;

		private float fps;

		private float lastInterval;

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void OnGUI()
		{
		}
	}
}
