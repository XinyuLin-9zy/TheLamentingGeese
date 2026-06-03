public class Custom_TTS_Template : UAP_CustomTTS
{
	protected override void Initialize()
	{
	}

	protected override TTSInitializationState GetInitializationStatus()
	{
		return default(TTSInitializationState);
	}

	protected override void SpeakText(string textToSay, float speakRate)
	{
	}

	protected override void StopSpeaking()
	{
	}

	protected override bool IsCurrentlySpeaking()
	{
		return false;
	}
}
