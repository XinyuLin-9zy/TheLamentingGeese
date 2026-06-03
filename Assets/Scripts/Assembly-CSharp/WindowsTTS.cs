using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsTTS : MonoBehaviour
{
	public static WindowsTTS instance;

	private static bool m_UseNVDA;

	private static float m_NVDAIsSpeakingTimer;

	[PreserveSig]
	public static extern void Initialize();

	[PreserveSig]
	public static extern void DestroySpeech();

	[PreserveSig]
	public static extern void StopSpeech();

	[PreserveSig]
	public static extern void AddToSpeechQueue(string s);

	[PreserveSig]
	public static extern bool IsVoiceSpeaking();

	[PreserveSig]
	internal static extern int nvdaController_testIfRunning();

	[PreserveSig]
	internal static extern int nvdaController_speakText(string text);

	[PreserveSig]
	internal static extern int nvdaController_cancelSpeech();

	private void Awake()
	{
	}

	public static bool IsScreenReaderDetected()
	{
		return false;
	}

	private void Start()
	{
	}

	public static void Speak(string msg)
	{
	}

	public static void Stop()
	{
	}

	public static bool IsSpeaking()
	{
		return false;
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
	}
}
