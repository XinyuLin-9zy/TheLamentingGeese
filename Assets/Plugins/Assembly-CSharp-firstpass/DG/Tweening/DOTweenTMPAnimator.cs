using System;
using System.Collections.Generic;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace DG.Tweening
{
	public class DOTweenTMPAnimator : IDisposable
	{
		private struct CharVertices
		{
			public Vector3 bottomLeft;

			public Vector3 topLeft;

			public Vector3 topRight;

			public Vector3 bottomRight;

			public CharVertices(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
			{
				this.bottomLeft = default(Vector3);
				this.topLeft = default(Vector3);
				this.topRight = default(Vector3);
				this.bottomRight = default(Vector3);
			}
		}

		private struct CharTransform
		{
			public int charIndex;

			public Vector3 offset;

			public Quaternion rotation;

			public Vector3 scale;

			private Vector3 _topLeftShift;

			private Vector3 _topRightShift;

			private Vector3 _bottomLeftShift;

			private Vector3 _bottomRightShift;

			private Vector3 _charMidBaselineOffset;

			private int _matIndex;

			private int _firstVertexIndex;

			private TMP_MeshInfo _meshInfo;

			public bool isVisible { get; private set; }

			public CharTransform(int charIndex, TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos)
			{
				this.charIndex = 0;
				isVisible = false;
				offset = default(Vector3);
				rotation = default(Quaternion);
				scale = default(Vector3);
				_topLeftShift = default(Vector3);
				_topRightShift = default(Vector3);
				_bottomLeftShift = default(Vector3);
				_bottomRightShift = default(Vector3);
				_charMidBaselineOffset = default(Vector3);
				_matIndex = 0;
				_firstVertexIndex = 0;
				_meshInfo = default(TMP_MeshInfo);
			}

			public void Refresh(TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos)
			{
			}

			public void ResetAll(TMP_Text target, TMP_MeshInfo[] meshInfos, TMP_MeshInfo[] cachedMeshInfos)
			{
			}

			public void ResetTransformationData()
			{
			}

			public void ResetGeometry(TMP_Text target, TMP_MeshInfo[] cachedMeshInfos)
			{
			}

			public void ResetColors(TMP_Text target, TMP_MeshInfo[] meshInfos)
			{
			}

			public Color32 GetColor(TMP_MeshInfo[] meshInfos)
			{
				return default(Color32);
			}

			public CharVertices GetVertices()
			{
				return default(CharVertices);
			}

			public void UpdateAlpha(TMP_Text target, Color alphaColor, TMP_MeshInfo[] meshInfos, bool apply = true)
			{
			}

			public void UpdateColor(TMP_Text target, Color32 color, TMP_MeshInfo[] meshInfos, bool apply = true)
			{
			}

			public void UpdateGeometry(TMP_Text target, Vector3 offset, Quaternion rotation, Vector3 scale, TMP_MeshInfo[] cachedMeshInfos, bool apply = true)
			{
			}

			public void ShiftVertices(TMP_Text target, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
			{
			}

			public void ResetVerticesShift(TMP_Text target)
			{
			}
		}

		private static readonly Dictionary<TMP_Text, DOTweenTMPAnimator> _targetToAnimator;

		private readonly List<CharTransform> _charTransforms;

		private TMP_MeshInfo[] _cachedMeshInfos;

		private bool _ignoreTextChangedEvent;

		public TMP_Text target { get; private set; }

		public TMP_TextInfo textInfo { get; private set; }

		public DOTweenTMPAnimator(TMP_Text target)
		{
		}

		public static void DisposeInstanceFor(TMP_Text target)
		{
		}

		public void Dispose()
		{
		}

		public void Refresh()
		{
		}

		public void Reset()
		{
		}

		private void OnTextChanged(UnityEngine.Object obj)
		{
		}

		private bool ValidateChar(int charIndex, bool isTween = true)
		{
			return false;
		}

		private bool ValidateSpan(int fromCharIndex, int toCharIndex, out int firstVisibleCharIndex, out int lastVisibleCharIndex)
		{
			firstVisibleCharIndex = default(int);
			lastVisibleCharIndex = default(int);
			return false;
		}

		public void SkewSpanX(int fromCharIndex, int toCharIndex, float skewFactor, bool skewTop = true)
		{
		}

		public void SkewSpanY(int fromCharIndex, int toCharIndex, float skewFactor, TMPSkewSpanMode mode = TMPSkewSpanMode.Default, bool skewRight = true)
		{
		}

		public Color GetCharColor(int charIndex)
		{
			return default(Color);
		}

		public Vector3 GetCharOffset(int charIndex)
		{
			return default(Vector3);
		}

		public Vector3 GetCharRotation(int charIndex)
		{
			return default(Vector3);
		}

		public Vector3 GetCharScale(int charIndex)
		{
			return default(Vector3);
		}

		public void SetCharColor(int charIndex, Color32 color)
		{
		}

		public void SetCharOffset(int charIndex, Vector3 offset)
		{
		}

		public void SetCharRotation(int charIndex, Vector3 rotation)
		{
		}

		public void SetCharScale(int charIndex, Vector3 scale)
		{
		}

		public void ShiftCharVertices(int charIndex, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
		{
		}

		public float SkewCharX(int charIndex, float skewFactor, bool skewTop = true)
		{
			return 0f;
		}

		public float SkewCharY(int charIndex, float skewFactor, bool skewRight = true, bool fixedSkew = false)
		{
			return 0f;
		}

		public void ResetVerticesShift(int charIndex)
		{
		}

		public TweenerCore<Color, Color, ColorOptions> DOFadeChar(int charIndex, float endValue, float duration)
		{
			return null;
		}

		public TweenerCore<Color, Color, ColorOptions> DOColorChar(int charIndex, Color endValue, float duration)
		{
			return null;
		}

		public TweenerCore<Vector3, Vector3, VectorOptions> DOOffsetChar(int charIndex, Vector3 endValue, float duration)
		{
			return null;
		}

		public TweenerCore<Quaternion, Vector3, QuaternionOptions> DORotateChar(int charIndex, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
		{
			return null;
		}

		public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, float endValue, float duration)
		{
			return null;
		}

		public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, Vector3 endValue, float duration)
		{
			return null;
		}

		public Tweener DOPunchCharOffset(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
		{
			return null;
		}

		public Tweener DOPunchCharRotation(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
		{
			return null;
		}

		public Tweener DOPunchCharScale(int charIndex, float punch, float duration, int vibrato = 10, float elasticity = 1f)
		{
			return null;
		}

		public Tweener DOPunchCharScale(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
		{
			return null;
		}

		public Tweener DOShakeCharOffset(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
		{
			return null;
		}

		public Tweener DOShakeCharOffset(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
		{
			return null;
		}

		public Tweener DOShakeCharRotation(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
		{
			return null;
		}

		public Tweener DOShakeCharScale(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
		{
			return null;
		}

		public Tweener DOShakeCharScale(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
		{
			return null;
		}
	}
}
