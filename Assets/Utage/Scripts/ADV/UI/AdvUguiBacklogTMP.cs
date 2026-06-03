// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UtageExtensions;

namespace Utage
{
	/// バックログ用UI(TextMeshPro版)
	[AddComponentMenu("Utage/ADV/TMP/AdvUguiBacklogTMP")]
	public class AdvUguiBacklogTMP
		: AdvUguiBacklog
			, IUsingTextMeshPro
	{
		public TextMeshProNovelText TextMeshProLogText => textMeshProLogText;
		public TextMeshProNovelText TextMeshProCharacterName => textMeshProCharacterName;


		//ボイスが複数ある場合の初期化を行う
		protected override void InitVoiceIfMulti()
		{
			if (TextMeshProLogText == null || TextMeshProLogText.TextMeshPro == null)
			{
				return;
			}

			TextMeshProLogText.TextMeshPro.raycastTarget = true;
			AdvUguiBacklogTMPEventTrigger trigger = TextMeshProLogText.gameObject.GetComponentCreateIfMissing<AdvUguiBacklogTMPEventTrigger>();
			trigger.enabled = true;
			trigger.InitAsBackLog(this);
		}

		protected override void ClearTextVoiceClickListener()
		{
			base.ClearTextVoiceClickListener();

			if (TextMeshProLogText == null)
			{
				return;
			}

			AdvUguiBacklogTMPEventTrigger trigger = TextMeshProLogText.gameObject.GetComponent<AdvUguiBacklogTMPEventTrigger>();
			if (trigger != null)
			{
				trigger.Clear();
			}
		}

		public void OnClicked(AdvBacklog.AdvBacklogDataInPage dataInPage )
		{
			if (dataInPage == null)
			{
				return;
			}
			OnClicked(dataInPage.VoiceFileName);
		}

		public void OnClickedMainVoice()
		{
			if (Data == null)
			{
				return;
			}
			OnClicked(Data.MainVoiceFileName);
		}
	}
}

