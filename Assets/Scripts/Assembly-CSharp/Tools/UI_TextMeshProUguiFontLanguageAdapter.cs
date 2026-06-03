using PureMVC.Interfaces;
using TMPro;
using UnityEngine;

namespace Tools
{
	public class UI_TextMeshProUguiFontLanguageAdapter : MonoBehaviour
	{
		private TextMeshProUGUI text;

		private RubyTextMeshProUGUI ruby_text;

		public TMP_FontAsset SC;

		public TMP_FontAsset TC;

		public TMP_FontAsset English;

		public TMP_FontAsset Japanese;

		public TMP_FontAsset Russian;

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void RefreshFont()
		{
		}

		private void HandleNotification(INotification notification)
		{
		}
	}
}
