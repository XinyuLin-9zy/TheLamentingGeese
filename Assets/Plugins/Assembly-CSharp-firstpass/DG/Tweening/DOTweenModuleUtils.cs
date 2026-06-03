using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Scripting;

namespace DG.Tweening
{
	public static class DOTweenModuleUtils
	{
		public static class Physics
		{
			public static void SetOrientationOnPath(PathOptions options, Tween t, Quaternion newRot, Transform trans)
			{
			}

			public static bool HasRigidbody2D(Component target)
			{
				return false;
			}

			[Preserve]
			public static bool HasRigidbody(Component target)
			{
				return false;
			}

			[Preserve]
			public static TweenerCore<Vector3, Path, PathOptions> CreateDOTweenPathTween(MonoBehaviour target, bool tweenRigidbody, bool isLocal, Path path, float duration, PathMode pathMode)
			{
				return null;
			}
		}

		private static bool _initialized;

		[Preserve]
		public static void Init()
		{
		}

		[Preserve]
		private static void Preserver()
		{
		}
	}
}
