using UnityEngine;

namespace ZeroPlay
{
	public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T ins;

		public static T Ins => null;

		protected virtual void Awake()
		{
		}
	}
}
