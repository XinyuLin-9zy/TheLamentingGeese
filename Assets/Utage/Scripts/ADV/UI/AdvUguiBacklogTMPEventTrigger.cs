// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UtageExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;


namespace Utage
{
	
	// 複数サウンドが設定されている場合のバックログで、TextMeshPro用の当たり判定をとるのに必要なイベントトリガー
	[AddComponentMenu("Utage/TextMeshPro/AdvUguiBacklogTMPEventTrigger")]
	public class AdvUguiBacklogTMPEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
	{
		AdvUguiBacklogTMP BacklogTMP { get; set; }
		TMP_Text TextMeshPro
		{
			get
			{
				if (BacklogTMP == null || BacklogTMP.TextMeshProLogText == null)
				{
					return null;
				}
				return BacklogTMP.TextMeshProLogText.TextMeshPro;
			}
		}
		public Color hoverColor = ColorUtil.Red;

		class Log
		{
			public AdvBacklog.AdvBacklogDataInPage DataInPage { get; }
			public int BeginIndex { get; }
			public int Length { get; }
			public int EndIndex => BeginIndex + Length -1; 
			public bool HasVoice => DataInPage != null && !string.IsNullOrEmpty(DataInPage.VoiceFileName);
			List<Color32[]> DefaultColors { get; } = new List<Color32[]>(); 
			public Log(AdvBacklog.AdvBacklogDataInPage dataInPage, int beginIndex)
			{
				DataInPage = dataInPage;
				BeginIndex = beginIndex;
				var textData = new TextData(dataInPage != null ? dataInPage.LogText : "");
				Length = textData.Length;
				
#if UNITY_EDITOR
				if (Length != textData.NoneMetaString.LengthWithSurrogatePairs())
				{
					Length = textData.NoneMetaString.LengthWithSurrogatePairs();
					Debug.LogError("サロゲートペア文字列の長さと一致しない\n"+ $"{BeginIndex}~{EndIndex} Len={Length}");
				}

#endif

				for (int i = BeginIndex; i <= EndIndex; i++)
				{
					DefaultColors.Add(null);
				}
			}
			public bool ContainsTextIndex(int characterIndex)
			{
				return BeginIndex <= characterIndex && characterIndex <= EndIndex;
			}
			public bool TrySetDefaultColors(int characterIndex, Color32 color0, Color32 color1, Color32 color2, Color32 color3)
			{
				int index = characterIndex - BeginIndex;
				if (index < 0 || index >= DefaultColors.Count) return false;
				if (DefaultColors[index] != null) return false;
				DefaultColors[index] = new Color32[4]{color0, color1, color2, color3};
				return true;
			}
			public bool TryGetDefaultColors(int characterIndex, out Color32[] colors)
			{
				int index = characterIndex - BeginIndex;
				if (index < 0 || index >= DefaultColors.Count)
				{
					colors = null;
					return false;
				}
				if (DefaultColors[index] == null)
				{
					colors = null;
					return false;
				}
				colors = DefaultColors[index];
				return true;
			}
		}
		List<Log> LogList { get; } = new List<Log>();

		bool IsEntered { get; set; }
		Log CurrentTarget { get; set; }
		Camera Camera { get; set; }
			
		public void InitAsBackLog(AdvUguiBacklogTMP advUguiBacklogTMP)
		{
			Clear();
			BacklogTMP = advUguiBacklogTMP;
			TextMeshProNovelText logText = BacklogTMP != null ? BacklogTMP.TextMeshProLogText : null;
			TMP_Text textMeshPro = logText != null ? logText.TextMeshPro : null;
			if (BacklogTMP == null || BacklogTMP.Data == null || logText == null || textMeshPro == null)
			{
				enabled = false;
				return;
			}

			enabled = true;
			if(!textMeshPro.raycastTarget)
			{
				textMeshPro.raycastTarget = true;
			}
			logText.UpdateIfChanged();
			textMeshPro.ForceMeshUpdate();
			LogList.Clear();
			int textIndex = 0;
			foreach (var dataInPage in BacklogTMP.Data.DataList)
			{
				var log = new Log(dataInPage, textIndex);
				LogList.Add(log);
				textIndex = log.EndIndex+1;
			}
		}

		public void Clear()
		{
			ChangeCurrentTarget(null);
			LogList.Clear();
			IsEntered = false;
			CurrentTarget = null;
			Camera = null;
			BacklogTMP = null;
			enabled = false;
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if(BacklogTMP==null) return;
			var target = HitTest(eventData.position,eventData.pressEventCamera);
			if (target != null)
			{
				if (target.HasVoice)
				{
					BacklogTMP.OnClicked(target.DataInPage);
				}
				return;
			}

			target = HitTestNearestVoice(eventData.position,eventData.pressEventCamera);
			if (target != null)
			{
				BacklogTMP.OnClicked(target.DataInPage);
				return;
			}

			BacklogTMP.OnClickedMainVoice();
		}

		public void OnPointerDown(PointerEventData eventData) { }

		//当たり判定に入ったとき
		public void OnPointerEnter(PointerEventData eventData)
		{
			if(BacklogTMP==null) return;
			IsEntered = true;
			Camera = eventData.enterEventCamera != null ? eventData.enterEventCamera : eventData.pressEventCamera;
			ChangeCurrentTarget(HitTest(eventData.position,Camera));
		}

		//当たり判定から出た
		public void OnPointerExit(PointerEventData eventData)
		{
			if(BacklogTMP==null) return;
			IsEntered = false;
			ChangeCurrentTarget(null);
		}

		void Update()
		{
			if(!IsEntered) return;
			// 現在のマウス位置を取得する
			var mousePosition = InputUtil.GetMousePosition();
			ChangeCurrentTarget(HitTest(mousePosition,Camera));
		}

		Log HitTest(Vector2 screenPoint, Camera cam)
		{
			TMP_Text textMeshPro = TextMeshPro;
			if (textMeshPro == null || LogList.Count <= 0) return null;

			int characterIndex = TMP_TextUtilities.FindIntersectingCharacter(textMeshPro, screenPoint, cam, true);
			if (characterIndex < 0) return null;

			return FindLogByCharacterIndex(characterIndex);
		}

		Log HitTestNearestVoice(Vector2 screenPoint, Camera cam)
		{
			TMP_Text textMeshPro = TextMeshPro;
			if (textMeshPro == null || LogList.Count <= 0) return null;

			int characterIndex = TMP_TextUtilities.FindNearestCharacter(textMeshPro, screenPoint, cam, true);
			if (characterIndex < 0) return null;

			Log target = FindLogByCharacterIndex(characterIndex);
			if (target != null && target.HasVoice)
			{
				return target;
			}

			return FindNearestVoiceLog(characterIndex);
		}

		Log FindLogByCharacterIndex(int characterIndex)
		{
			foreach (var log in LogList)
			{
				if (log.ContainsTextIndex(characterIndex))
				{
					return log;
				}
			}
			return null;
		}

		Log FindNearestVoiceLog(int characterIndex)
		{
			Log nearest = null;
			int nearestDistance = int.MaxValue;
			foreach (var log in LogList)
			{
				if (!log.HasVoice)
				{
					continue;
				}

				int distance = 0;
				if (characterIndex < log.BeginIndex)
				{
					distance = log.BeginIndex - characterIndex;
				}
				else if (characterIndex > log.EndIndex)
				{
					distance = characterIndex - log.EndIndex;
				}

				if (distance < nearestDistance)
				{
					nearest = log;
					nearestDistance = distance;
				}
			}
			return nearest;
		}

		void ChangeCurrentTarget(Log target)
		{
			if (CurrentTarget == target) return;
			if (CurrentTarget != null)
			{
				ResetEffectColor(CurrentTarget);
			}
			CurrentTarget = target;
			if (CurrentTarget != null)
			{
				if (CurrentTarget.HasVoice)
				{
					ChangeEffectColor(CurrentTarget,hoverColor);
				}
				else
				{
					ResetEffectColor(CurrentTarget);
				}
			}
		}
		
		void ResetEffectColor(Log log)
		{
			TMP_Text textMeshPro = TextMeshPro;
			if (textMeshPro == null || log == null) return;

			for(int i = log.BeginIndex; i <= log.EndIndex; ++i)
			{
				if (log.TryGetDefaultColors(i, out Color32[] colors))
				{
					ChangeColor(log, textMeshPro,i,colors);
				}
			}
			// メッシュを再構築して変更を反映する
			textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}

		static Color32[] CacheColors { get; } = new Color32[4];

		void ChangeEffectColor(Log log, Color color)
		{
//			Debug.Log($"{log.BeginIndex}~{log.EndIndex} in {TextMeshPro.textInfo.characterCount}");
			TMP_Text textMeshPro = TextMeshPro;
			if (textMeshPro == null || log == null) return;
			
			for (int i = 0; i < 4; i++)
			{
				CacheColors[i] = color;
			}
			for(int i = log.BeginIndex; i <= log.EndIndex; ++i)
			{
				ChangeColor(log, textMeshPro,i,CacheColors);
			}
			// メッシュを再構築して変更を反映する
			textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}
		void ChangeColor(Log log, TMP_Text textMeshPro, int index, Color32[] colors)
		{
			if (log == null || textMeshPro == null || colors == null || colors.Length < 4) return;

			// インデックスがテキストの範囲内であることを確認する
			var textInfo = textMeshPro.textInfo;
			if (index < 0 || index >= textInfo.characterCount) return;

			var characterInfo = textInfo.characterInfo[index];
            
			if (!characterInfo.isVisible)
				return;

         
			// 現在の文字の Material と 頂点 の位置を取得
			var materialIndex = characterInfo.materialReferenceIndex;
			var vIndex = characterInfo.vertexIndex;
			if (materialIndex < 0 || materialIndex >= textInfo.meshInfo.Length) return;
            
			var colors32 = textInfo.meshInfo[materialIndex].colors32;
			if (colors32 == null || vIndex < 0 || vIndex + 3 >= colors32.Length) return;

			log.TrySetDefaultColors(index, colors32[vIndex + 0], colors32[vIndex + 1], colors32[vIndex + 2], colors32[vIndex + 3]);
			for (var i = 0; i < 4; i++)
			{
				colors32[vIndex + i] = colors[i];
			}
		}
	}
}
