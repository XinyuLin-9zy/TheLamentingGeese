// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// CGギャラリー画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSceneGallery")]
	public class UtageUguiSceneGallery : UguiView
	{
		/// カテゴリつきのグリッドビュー(ページ切り替え機能付き)
		[UnityEngine.Serialization.FormerlySerializedAs("categoryGirdPage")]
		public UguiCategoryGridPage categoryGridPage;

		/// カテゴリつきのグリッドビュー(ページ切り替え機能なし)
		public UguiCategoryPanel categoryPanel;

		/// <summary>
		/// ギャラリー選択画面
		/// </summary>
		public UtageUguiGallery Gallery
		{
			get { return this.GetComponentCacheFindIfMissing(ref gallery); }
		}

		public UtageUguiGallery gallery;

		/// <summary>
		/// メインゲーム画面
		/// </summary>
		public UtageUguiMainGame mainGame;

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] AdvEngine engine;

		//ガイドメッセージの表示。設定してないときは表示しない
		public SystemUiGuideMessage guideMessage;

		protected bool isInit = false;

		/// <summary>アイテムのリスト</summary>
		protected List<AdvSceneGallerySettingData> itemDataList = new List<AdvSceneGallerySettingData>();

		protected virtual void OnEnable()
		{
			ResetPageContents();
			if (status != Status.Opening)
			{
				OnOpen();
			}
		}

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			this.ChangeBgm();
			ResetPageContents();
			StartCoroutine(CoWaitOpen());
		}

		/// <summary>
		/// クローズしたときに呼ばれる
		/// </summary>
		protected virtual void OnClose()
		{
			isInit = false;
		}

		protected virtual void ResetPageContents()
		{
			if (categoryGridPage != null)
			{
				categoryGridPage.Clear();
			}
			else if (categoryPanel != null)
			{
				categoryPanel.Clear();
			}
		}

		//ロード待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			isInit = false;
			while (Engine.IsWaitBootLoading)
			{
				yield return null;
			}

			if (categoryGridPage != null)
			{
				categoryGridPage.Init(
					Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateCategoryList().ToArray(),
					OpenCurrentCategory);
			}
			else if (categoryPanel != null)
			{
				categoryPanel.Init(
					Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateCategoryList().ToArray(),
					OpenCurrentCategory);
			}

			isInit = true;
		}

		protected virtual void Update()
		{
			//右クリックで戻る
			if (isInit && InputUtil.IsInputGuiClose())
			{
				Gallery.Back();
			}
		}


		/// <summary>
		/// 現在のカテゴリのページを開く
		/// </summary>
		protected virtual void OpenCurrentCategory(UguiCategoryGridPage categoryGridPage)
		{
			itemDataList =
				Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateGalleryDataList(categoryGridPage.CurrentCategory);
			categoryGridPage.OpenCurrentCategory(itemDataList.Count, CreateItem);
		}

		/// <summary>
		/// リストビューのアイテムが作成されるときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作成されたアイテムのGameObject</param>
		/// <param name="index">作成されたアイテムのインデックス</param>
		protected virtual void CreateItem(GameObject go, int index)
		{
			AdvSceneGallerySettingData data = itemDataList[index];
			UtageUguiSceneGalleryItem item = go.GetComponent<UtageUguiSceneGalleryItem>();
			if (item == null) return;
			item.Init(data, Engine.SystemSaveData);
		}

		/// <summary>
		/// 各アイテムが押された
		/// </summary>
		/// <param name="button">押されたアイテム</param>
		protected virtual void OnTap(UtageUguiSceneGalleryItem item)
		{
			OnClickedButton(item);
		}

		// 現在のカテゴリのページを開く
		// 宴4以降の新しいやり方
		protected virtual void OpenCurrentCategory(UguiCategoryPanel panel)
		{
			itemDataList =
				Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateGalleryDataList(
					panel.CurrentCategory);
			panel.OpenCurrentCategory(itemDataList.Count,
					(GameObject go, int index) =>
					{
						AdvSceneGallerySettingData data = itemDataList[index];
						UtageUguiSceneGalleryItem item = go.GetComponent<UtageUguiSceneGalleryItem>();
						item.Init(data, Engine.SystemSaveData);
					});
		}

		//UtageUguiSceneGalleryItemがクリックされたときに、プログラムから呼ばれる
		//宴4以降の新しいやり方
		public virtual void OnClickedButton(UtageUguiSceneGalleryItem item)
		{
			if (item == null || item.Data == null || Engine == null || Engine.SystemSaveData == null || Engine.SystemSaveData.GalleryData == null)
			{
				return;
			}

			bool isOpened = Engine.SystemSaveData.GalleryData.CheckSceneLabels(item.Data.ScenarioLabel);
			if (isOpened)
			{
				//正常に開く
				if (gallery != null) gallery.Close();
				if (mainGame != null) mainGame.OpenSceneGallery(item.Data.ScenarioLabel);
			}
			else
			{
				//開けない
				//ガイドメッセージの表示
				if (guideMessage != null)
				{
					guideMessage.Open(LanguageSystemText.LocalizeText(SystemText.UtageGuideMessageSceneGalleryNotOpened));
				}
			}
		}
	}
}
