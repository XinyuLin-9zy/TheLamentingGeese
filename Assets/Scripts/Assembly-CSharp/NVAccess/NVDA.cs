using System.Runtime.InteropServices;

namespace NVAccess
{
	internal static class NVDA
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
		public delegate int OnSsmlMarkReached(string mark);

		public static bool IsRunning => false;

		[PreserveSig]
		private static extern int nvdaController_brailleMessage(string message);

		[PreserveSig]
		private static extern int nvdaController_cancelSpeech();

		[PreserveSig]
		private static extern int nvdaController_speakText(string text);

		[PreserveSig]
		private static extern int nvdaController_testIfRunning();

		[PreserveSig]
		private static extern int nvdaController_getProcessId(out uint processId);

		[PreserveSig]
		private static extern int nvdaController_speakSsml(string ssml, SymbolLevel symbolLevel = SymbolLevel.Unchanged, SpeechPriority priority = SpeechPriority.Normal, bool asynchronous = true);

		[PreserveSig]
		private static extern int nvdaController_setOnSsmlMarkReachedCallback(OnSsmlMarkReached callback);

		public static void Braille(string message)
		{
		}

		public static void CancelSpeech()
		{
		}

		public static void Speak(string text, bool interrupt = true)
		{
		}

		public static void SpeakSsml(string ssml, SymbolLevel symbolLevel = SymbolLevel.Unchanged, SpeechPriority priority = SpeechPriority.Normal, bool asynchronous = true, OnSsmlMarkReached callback = null)
		{
		}

		public static uint GetProcessId()
		{
			return 0u;
		}
	}
}
