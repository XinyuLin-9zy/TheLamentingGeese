using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Accessibility/Core/UAP Audio Queue")]
[RequireComponent(typeof(AudioSource))]
public class UAP_AudioQueue : MonoBehaviour
{
	public enum EAudioType
	{
		None = 0,
		Pause = 1,
		Element_Text = 2,
		Element_Type = 4,
		Element_Hint = 8,
		App = 0x10,
		Container_Name = 0x20,
		Skippable = 0x40
	}

	public enum EInterrupt
	{
		None = 0,
		Elements = 79,
		All = 127
	}

	public delegate void UAP_GenericCallback();

	private class SAudioEntry
	{
		public string m_TTS_Text;

		public bool m_AllowVoiceOver;

		public AudioClip m_Audio;

		public EAudioType m_AudioType;

		public bool m_IsInterruptible;

		public float m_PauseDuration;

		public UAP_GenericCallback m_CallbackOnDone;
	}

	public int m_CurrentQueueLength;

	public float m_CurrentPauseDuration;

	public string m_CurrentElement;

	public bool m_IsSpeaking;

	private int m_SpeechRate;

	private AudioSource m_AudioPlayer;

	private Queue<SAudioEntry> m_AudioQueue;

	private SAudioEntry m_ActiveEntry;

	private float m_PauseTimer;

	private float m_TTS_SpeakingTimer;

	public void QueueAudio(string textForTTS, EAudioType type, bool allowVoiceOver, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
	}

	public void QueueAudio(AudioClip audioFile, EAudioType type, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
	}

	public void QueuePause(float durationInSecs)
	{
	}

	public void Stop()
	{
	}

	public void StopAllInterruptibles()
	{
	}

	public void InterruptAppAnnouncement()
	{
	}

	private void QueueAudio(SAudioEntry newEntry, EInterrupt interrupts)
	{
	}

	private void InvalidateActiveEntry()
	{
	}

	private void InitializeWindowsTTS()
	{
	}

	public void Initialize()
	{
	}

	private void OnDestroy()
	{
	}

	private void TTS_Speak(string text, bool allowVoiceOver = true)
	{
	}

	private bool TTS_IsSpeaking()
	{
		return false;
	}

	private void StopAudio(bool includingAndroid = false)
	{
	}

	private void Update()
	{
	}

	public bool IsPlaying()
	{
		return false;
	}

	public bool IsPlayingExceptAppOrSkippable()
	{
		return false;
	}

	public bool IsCompletelyEmpty()
	{
		return false;
	}

	public int GetSpeechRate()
	{
		return 0;
	}

	public int SetSpeechRate(int speechRate)
	{
		return 0;
	}

	private void InitializeCustomTTS()
	{
	}
}
