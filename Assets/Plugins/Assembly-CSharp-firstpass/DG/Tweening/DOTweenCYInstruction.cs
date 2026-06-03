using UnityEngine;

namespace DG.Tweening
{
	public static class DOTweenCYInstruction
	{
		public class WaitForCompletion : CustomYieldInstruction
		{
			private readonly Tween t;

			public override bool keepWaiting => false;

			public WaitForCompletion(Tween tween)
			{
			}
		}

		public class WaitForRewind : CustomYieldInstruction
		{
			private readonly Tween t;

			public override bool keepWaiting => false;

			public WaitForRewind(Tween tween)
			{
			}
		}

		public class WaitForKill : CustomYieldInstruction
		{
			private readonly Tween t;

			public override bool keepWaiting => false;

			public WaitForKill(Tween tween)
			{
			}
		}

		public class WaitForElapsedLoops : CustomYieldInstruction
		{
			private readonly Tween t;

			private readonly int elapsedLoops;

			public override bool keepWaiting => false;

			public WaitForElapsedLoops(Tween tween, int elapsedLoops)
			{
			}
		}

		public class WaitForPosition : CustomYieldInstruction
		{
			private readonly Tween t;

			private readonly float position;

			public override bool keepWaiting => false;

			public WaitForPosition(Tween tween, float position)
			{
			}
		}

		public class WaitForStart : CustomYieldInstruction
		{
			private readonly Tween t;

			public override bool keepWaiting => false;

			public WaitForStart(Tween tween)
			{
			}
		}
	}
}
