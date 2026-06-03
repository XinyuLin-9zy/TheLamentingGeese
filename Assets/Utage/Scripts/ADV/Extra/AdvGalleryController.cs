// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System;
using UnityEngine;
using UtageExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace Utage
{
	[AddComponentMenu("Utage/ADV/Extra/AdvGalleryController")]
	public class AdvGalleryController : MonoBehaviour
	{
		const string DefaultSceneGalleryParamKey = "scene_gallery";

		AdvEngine Engine => this.GetComponentCache(ref engine);
		AdvEngine engine;
		
		//シーン回想を実行中か
		public bool IsPlayingSceneGallery { get; protected set; }
		
		//シーン回想開始時に呼ばれるイベント
		public UnityEvent OnStartSceneGalleryOnSceneGallery => onStartSceneGallery;
		[SerializeField] UnityEvent onStartSceneGallery = new UnityEvent();

		//シーン回想実行中のフラグを設定するパラメーター名
		[SerializeField] string paramKeyPlayingSceneGallery = "";
		public string ParamKeyPlayingSceneGallery
		{
			get => paramKeyPlayingSceneGallery;
			set => paramKeyPlayingSceneGallery = value;
		}


		void Awake()
		{
			Engine.ScenarioPlayer.OnBeginScenarioAfterParametersInitialized.AddListener(OnBeginScenarioAfterParametersInitialized);
		}

		//AdvEngineのシナリオ開始時に呼ばれる
		public void StartGame(bool isSceneGallery)
		{
			IsPlayingSceneGallery = isSceneGallery;
		}

		public void OnBeginScenarioAfterParametersInitialized(AdvScenarioPlayer scenarioPlayer)
		{
			string paramKey = ResolvePlayingSceneGalleryParamKey();
			if (!string.IsNullOrEmpty(paramKey) && Engine.Param.ExistParameter(paramKey))
			{
				Engine.Param.SetParameterBoolean(paramKey, IsPlayingSceneGallery);
			}
			if(IsPlayingSceneGallery)
			{
				onStartSceneGallery.Invoke();
			}
		}

		string ResolvePlayingSceneGalleryParamKey()
		{
			if (!string.IsNullOrEmpty(paramKeyPlayingSceneGallery))
			{
				return paramKeyPlayingSceneGallery;
			}
			if (Engine != null && Engine.Param != null && Engine.Param.ExistParameter(DefaultSceneGalleryParamKey))
			{
				return DefaultSceneGalleryParamKey;
			}
			return string.Empty;
		}
	}
}

