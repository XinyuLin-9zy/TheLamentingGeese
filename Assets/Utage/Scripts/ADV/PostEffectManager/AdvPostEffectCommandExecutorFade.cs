using System;
using UnityEngine;
using UtageExtensions;

namespace Utage
{
	//ポストエフェクトとしてフェードコマンドを実行するコンポーネント
	public class AdvPostEffectCommandExecutorFade : AdvPostEffectCommandExecutorBase
	{
		public void DoCommand( Camera targetCamera, IAdvCommandFade command,  Action onComplete)
		{
			if (targetCamera == null)
			{
				Debug.LogWarning("Fade target camera is missing. Completing fade command without effect.", this);
				onComplete?.Invoke();
				return;
			}

			(IPostEffectStrength effect, float start, float end) = StartFadeSub(targetCamera, command);
			if (effect == null)
			{
				Debug.LogWarning("Fade effect could not be created. Completing fade command without effect.", this);
				onComplete?.Invoke();
				return;
			}

			if (command.AnimationData==null)
			{
				command.Timer = SetTimer(effect, command.Time,
						(x) => effect.Strength = x.GetCurve(start, end),
						(x) =>
						{
							onComplete();
							if (command.Inverse)
							{
								effect.enabled = false;
//								effect.RemoveComponentMySelf();
							}
						});
			}
			else
			{
				command.AnimationPlayer = SetAnimation(effect, command.AnimationData, onComplete);
			}
		}

		protected virtual (IPostEffectStrength effect, float start, float end) StartFadeSub(Camera targetCamera, IAdvCommandFade command)
		{
			if (string.IsNullOrEmpty(command.RuleImage))
			{
				return RpBridge.DoCommandColorFade(targetCamera, command);
			}
			else
			{
				return RpBridge.DoCommandRuleFade(targetCamera, command);
			}
		}
	}
}
