using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MacOSTTS : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CSpeakText_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CSpeakText_003Ed__4(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	public static MacOSTTS instance;

	private bool m_IsSpeaking;

	private void Start()
	{
	}

	public void Speak(string msg)
	{
	}

	[IteratorStateMachine(typeof(_003CSpeakText_003Ed__4))]
	private IEnumerator SpeakText(string textToSpeak)
	{
		return null;
	}

	public void Stop()
	{
	}

	public bool IsSpeaking()
	{
		return false;
	}

	private void OnDestroy()
	{
	}
}
