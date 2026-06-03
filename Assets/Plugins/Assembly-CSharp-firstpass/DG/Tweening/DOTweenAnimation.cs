using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening.Core;
using UnityEngine;

namespace DG.Tweening
{
	[AddComponentMenu("DOTween/DOTween Animation")]
	public class DOTweenAnimation : ABSAnimationComponent
	{
		public enum AnimationType
		{
			None = 0,
			Move = 1,
			LocalMove = 2,
			Rotate = 3,
			LocalRotate = 4,
			Scale = 5,
			Color = 6,
			Fade = 7,
			Text = 8,
			PunchPosition = 9,
			PunchRotation = 10,
			PunchScale = 11,
			ShakePosition = 12,
			ShakeRotation = 13,
			ShakeScale = 14,
			CameraAspect = 15,
			CameraBackgroundColor = 16,
			CameraFieldOfView = 17,
			CameraOrthoSize = 18,
			CameraPixelRect = 19,
			CameraRect = 20,
			UIWidthHeight = 21
		}

		public enum TargetType
		{
			Unset = 0,
			Camera = 1,
			CanvasGroup = 2,
			Image = 3,
			Light = 4,
			RectTransform = 5,
			Renderer = 6,
			SpriteRenderer = 7,
			Rigidbody = 8,
			Rigidbody2D = 9,
			Text = 10,
			Transform = 11,
			tk2dBaseSprite = 12,
			tk2dTextMesh = 13,
			TextMeshPro = 14,
			TextMeshProUGUI = 15
		}

		public bool targetIsSelf;

		public GameObject targetGO;

		public bool tweenTargetIsTargetGO;

		public float delay;

		public float duration;

		public Ease easeType;

		public AnimationCurve easeCurve;

		public LoopType loopType;

		public int loops;

		public string id;

		public bool isRelative;

		public bool isFrom;

		public bool isIndependentUpdate;

		public bool autoKill;

		public bool autoGenerate;

		public bool isActive;

		public bool isValid;

		public Component target;

		public AnimationType animationType;

		public TargetType targetType;

		public TargetType forcedTargetType;

		public bool autoPlay;

		public bool useTargetAsV3;

		public float endValueFloat;

		public Vector3 endValueV3;

		public Vector2 endValueV2;

		public Color endValueColor;

		public string endValueString;

		public Rect endValueRect;

		public Transform endValueTransform;

		public bool optionalBool0;

		public bool optionalBool1;

		public float optionalFloat0;

		public int optionalInt0;

		public RotateMode optionalRotationMode;

		public ScrambleMode optionalScrambleMode;

		public ShakeRandomnessMode optionalShakeRandomnessMode;

		public string optionalString;

		private bool _tweenAutoGenerationCalled;

		private int _playCount;

		public static event Action<DOTweenAnimation> OnReset
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		private static void Dispatch_OnReset(DOTweenAnimation anim)
		{
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Reset()
		{
		}

		private void OnDestroy()
		{
		}

		public void RewindThenRecreateTween()
		{
		}

		public void RewindThenRecreateTweenAndPlay()
		{
		}

		public void RecreateTween()
		{
		}

		public void RecreateTweenAndPlay()
		{
		}

		public void CreateTween(bool regenerateIfExists = false, bool andPlay = true)
		{
		}

		public List<Tween> GetTweens()
		{
			return null;
		}

		public void SetAnimationTarget(Component tweenTarget, bool useTweenTargetGameObjectForGroupOperations = true)
		{
		}

		public override void DOPlay()
		{
		}

		public override void DOPlayBackwards()
		{
		}

		public override void DOPlayForward()
		{
		}

		public override void DOPause()
		{
		}

		public override void DOTogglePause()
		{
		}

		public override void DORewind()
		{
		}

		public override void DORestart()
		{
		}

		public override void DORestart(bool fromHere)
		{
		}

		public override void DOComplete()
		{
		}

		public override void DOKill()
		{
		}

		public void DOPlayById(string id)
		{
		}

		public void DOPlayAllById(string id)
		{
		}

		public void DOPauseAllById(string id)
		{
		}

		public void DOPlayBackwardsById(string id)
		{
		}

		public void DOPlayBackwardsAllById(string id)
		{
		}

		public void DOPlayForwardById(string id)
		{
		}

		public void DOPlayForwardAllById(string id)
		{
		}

		public void DOPlayNext()
		{
		}

		public void DORewindAndPlayNext()
		{
		}

		public void DORewindAllById(string id)
		{
		}

		public void DORestartById(string id)
		{
		}

		public void DORestartAllById(string id)
		{
		}

		public void DOKillById(string id)
		{
		}

		public void DOKillAllById(string id)
		{
		}

		public static TargetType TypeToDOTargetType(Type t)
		{
			return default(TargetType);
		}

		public Tween CreateEditorPreview()
		{
			return null;
		}

		private GameObject GetTweenGO()
		{
			return null;
		}

		private GameObject GetTweenTarget()
		{
			return null;
		}

		private void ReEvaluateRelativeTween()
		{
		}
	}
}
