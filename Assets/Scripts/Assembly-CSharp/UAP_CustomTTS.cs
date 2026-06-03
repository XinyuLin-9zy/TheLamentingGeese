using UnityEngine;

public class UAP_CustomTTS : MonoBehaviour
{
	public enum TTSInitializationState
	{
		NotInitialized = 0,
		InProgress = 1,
		Initialized = 2
	}

	protected static UAP_CustomTTS Instance;

	protected virtual void Initialize()
	{
	}

	protected virtual TTSInitializationState GetInitializationStatus()
	{
		return default(TTSInitializationState);
	}

	protected virtual void SpeakText(string textToSay, float speakRate)
	{
	}

	protected virtual void StopSpeaking()
	{
	}

	protected virtual bool IsCurrentlySpeaking()
	{
		return false;
	}

	public static void InitializeCustomTTS<T>()
	{
	}

	public static void Speak(string textToSay, float speakRate)
	{
	}

	public static void Stop()
	{
	}

	public static bool IsSpeaking()
	{
		return false;
	}

	public static TTSInitializationState IsInitialized()
	{
		return default(TTSInitializationState);
	}

	private void OnDestroy()
	{
	}
}
