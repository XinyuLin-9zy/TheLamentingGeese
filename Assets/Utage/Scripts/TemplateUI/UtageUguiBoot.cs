// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// タイトル表示のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiBoot")]
	public class UtageUguiBoot : UguiView
	{
		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;


		public UguiFadeTextureStream fadeTextureStream;

		public UtageUguiTitle title;
		public UtageUguiLoadWait loadWait;

		public bool isWaitBoot;
		public bool isWaitDownLoad;
		public bool isWaitSplashScreen = true;

		const float CodexSplashWaitTimeout = 8f;
		RectTransform codexBootCover;

		protected virtual void Awake()
		{
			EnsureBootCover();
			SetBootCoverVisible(true);
		}

		///最初の画面なので自分でオープンする
		public virtual void Start()
		{
			EnsureBootCover();
			SetBootCoverVisible(true);
			if (title != null) title.gameObject.SetActive(false);
			StartCoroutine(CoUpdate());
		}

		protected virtual void OnDisable()
		{
			SetBootCoverVisible(false);
		}

		///
		protected virtual IEnumerator CoUpdate()
		{
#if UNITY_5_3_OR_NEWER
			if (ShouldWaitSplashScreen())
			{
				float startTime = Time.realtimeSinceStartup;
				while (!WrapperUnityVersion.IsFinishedSplashScreen())
				{
					if (Time.realtimeSinceStartup - startTime > CodexSplashWaitTimeout)
					{
						Debug.LogWarning("Skip waiting for Unity splash screen timeout.");
						break;
					}
					yield return null;
				}
			}
#endif
			//BGMなどを鳴らすために追加
			Open();

			if (fadeTextureStream)
			{
				fadeTextureStream.gameObject.SetActive(true);
				fadeTextureStream.Play();
				while (fadeTextureStream.IsPlaying) yield return null;
			}

			if (isWaitBoot)
			{
				while (Engine.IsWaitBootLoading) yield return null;
			}

			SetBootCoverVisible(false);
			if (ShouldOpenLoadWaitOnBoot())
			{
				loadWait.PrepareBootLoadingVisuals();
				loadWait.OpenOnBoot();
			}
			else
			{
				if (title != null) title.Open();
			}
			yield return null;
			this.Close();
		}

		protected virtual bool ShouldOpenLoadWaitOnBoot()
		{
			if (loadWait == null) return false;
			return isWaitDownLoad || loadWait.ShouldOpenOnBootAutomatically();
		}

		protected virtual bool ShouldWaitSplashScreen()
		{
			if (!isWaitSplashScreen) return false;
#if UNITY_EDITOR
			return false;
#else
			return true;
#endif
		}

		protected virtual void EnsureBootCover()
		{
			if (codexBootCover != null) return;

			RectTransform parent = transform.parent as RectTransform;
			if (parent == null) parent = transform as RectTransform;
			if (parent == null) return;

			Transform existing = parent.Find("CodexBootCover");
			if (existing != null)
			{
				codexBootCover = existing as RectTransform;
			}
			else
			{
				GameObject gameObject = new GameObject("CodexBootCover", typeof(RectTransform), typeof(Image));
				codexBootCover = gameObject.GetComponent<RectTransform>();
				codexBootCover.SetParent(parent, false);
			}

			Image image = codexBootCover.GetComponent<Image>();
			if (image == null) image = codexBootCover.gameObject.AddComponent<Image>();
			image.color = new Color(0.02f, 0.018f, 0.017f, 1f);
			image.raycastTarget = true;
			codexBootCover.anchorMin = Vector2.zero;
			codexBootCover.anchorMax = Vector2.one;
			codexBootCover.pivot = new Vector2(0.5f, 0.5f);
			codexBootCover.anchoredPosition = Vector2.zero;
			codexBootCover.sizeDelta = Vector2.zero;
			codexBootCover.SetAsLastSibling();
		}

		protected virtual void SetBootCoverVisible(bool visible)
		{
			if (codexBootCover == null) return;
			codexBootCover.gameObject.SetActive(visible);
			if (visible)
			{
				codexBootCover.SetAsLastSibling();
			}
		}

	}
}
