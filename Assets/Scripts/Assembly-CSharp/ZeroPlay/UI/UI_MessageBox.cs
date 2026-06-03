using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroPlay.UI
{
	public class UI_MessageBox : MonoBehaviour
	{
		[SerializeField]
		private Text text_title;

		[SerializeField]
		private Button buttonYes;

		[SerializeField]
		private Button buttonNo;

		public Action onYes;

		public Action onNo;

		public void Init(string textTitle, MessageBoxType type)
		{
		}

		private void OnClickYes()
		{
		}

		private void OnClickNo()
		{
		}
	}
}
