// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// セーブロード画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSaveLoad")]
	public class UtageUguiSaveLoad : UguiView
	{
		[SerializeField] protected UguiGridPage gridPage;

		/// <summary>
		/// リストビューアイテムのリスト
		/// </summary>
		protected List<AdvSaveData> itemDataList;

		/// <summary>ADVエンジン</summary>
		public virtual AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;

		/// <summary>メイン画面</summary>
		public UtageUguiMainGame mainGame;

		/// <summary>タイトル表記（セーブ画面かロード画面か）</summary>
		public GameObject saveRoot;

		/// <summary>タイトル表記（セーブ画面かロード画面か）</summary>
		public GameObject loadRoot;

		//ガイドメッセージの表示。設定してないときは表示しない
		public SystemUiGuideMessage guideMessage;
		//上書きセーブの確認ダイアログの表示。設定してないときは表示しない
		public SystemUiDialog2Button dialog;

		//ロード後、画面を閉じるまでの待機時間
		public float waitTimeOnLoad = 0;

		//セーブ画面か、ロード画面かの区別
		public bool IsSave => isSave;
		protected bool isSave;

		protected bool isInit = false;
		protected int lastPage;
		protected bool runtimeBindingsInitialized;


		/// <summary>
		/// セーブ画面を開く
		/// </summary>
		/// <param name="prev">前の画面</param>
		public virtual void OpenSave(UguiView prev)
		{
			EnsureRuntimeReferences();
			isSave = true;
			if (saveRoot != null) saveRoot.SetActive(true);
			if (loadRoot != null) loadRoot.SetActive(false);
			Open(prev);
		}

		/// <summary>
		/// ロード画面を開く
		/// </summary>
		/// <param name="prev">前の画面</param>
		public virtual void OpenLoad(UguiView prev)
		{
			EnsureRuntimeReferences();
			isSave = false;
			if (saveRoot != null) saveRoot.SetActive(false);
			if (loadRoot != null) loadRoot.SetActive(true);
			Open(prev);
		}

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			isInit = false;
			if (this.gridPage == null)
			{
				Debug.LogError("[UtageUguiSaveLoad] GridPage is missing. Save/Load list cannot be opened.", this);
				return;
			}
			this.gridPage.ClearItems();
			StartCoroutine(CoWaitOpen());
		}

		/// <summary>
		/// クローズしたときに呼ばれる
		/// </summary>
		protected virtual void OnClose()
		{
			if (gridPage == null) return;
			lastPage = gridPage.CurrentPage;
			this.gridPage.ClearItems();
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (gridPage == null) gridPage = GetComponentInChildren<UguiGridPage>(true);
			if (mainGame == null) mainGame = FindSceneObject<UtageUguiMainGame>();
			if (saveRoot == null) saveRoot = FindChildRecursive(transform, "Save")?.gameObject;
			if (loadRoot == null) loadRoot = FindChildRecursive(transform, "Load")?.gameObject;
			if (guideMessage == null) guideMessage = FindSceneObject<SystemUiGuideMessage>();
			if (guideMessage == null) guideMessage = CreateRuntimeGuideMessage();

			if (runtimeBindingsInitialized) return;
			BindBackButton("ButtonBack");
			BindBackButton("CloseButton");
			BindBackButton("Back");
			runtimeBindingsInitialized = true;
		}

		protected virtual T FindSceneObject<T>() where T : Component
		{
			foreach (T item in Resources.FindObjectsOfTypeAll<T>())
			{
				if (item != null && item.gameObject.scene.IsValid())
				{
					return item;
				}
			}
			return null;
		}

		protected virtual SystemUiGuideMessage CreateRuntimeGuideMessage()
		{
			Transform parent = null;
			SystemUi systemUi = SystemUi.GetInstance();
			if (systemUi != null)
			{
				parent = systemUi.transform;
			}
			if (parent == null)
			{
				Canvas canvas = FindSceneObject<Canvas>();
				if (canvas != null) parent = canvas.transform;
			}
			if (parent == null) return null;

			Transform existing = FindChildRecursive(parent, "__RuntimeGuideMessage");
			if (existing != null)
			{
				return existing.GetComponent<SystemUiGuideMessage>();
			}

			GameObject root = new GameObject("__RuntimeGuideMessage", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(SystemUiGuideMessage));
			root.layer = parent.gameObject.layer;
			root.transform.SetParent(parent, false);

			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.anchorMin = new Vector2(0.5f, 1f);
			rootRect.anchorMax = new Vector2(0.5f, 1f);
			rootRect.pivot = new Vector2(0.5f, 1f);
			rootRect.anchoredPosition = new Vector2(0f, -120f);
			rootRect.sizeDelta = new Vector2(720f, 64f);

			Image background = root.GetComponent<Image>();
			background.color = new Color(0f, 0f, 0f, 0.72f);

			GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
			textGo.layer = root.layer;
			textGo.transform.SetParent(root.transform, false);

			RectTransform textRect = textGo.GetComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = new Vector2(24f, 8f);
			textRect.offsetMax = new Vector2(-24f, -8f);

			Text text = textGo.GetComponent<Text>();
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.white;
			text.fontSize = 28;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Truncate;
			text.font = Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, text.fontSize);

			CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
			SystemUiGuideMessage message = root.GetComponent<SystemUiGuideMessage>();
			SetGuideMessageField(message, "text", text);
			SetGuideMessageField(message, "canvasGroup", canvasGroup);
			root.SetActive(false);
			return message;
		}

		protected static void SetGuideMessageField(SystemUiGuideMessage message, string name, object value)
		{
			FieldInfo field = typeof(SystemUiGuideMessage).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null) field.SetValue(message, value);
		}

		protected virtual void BindBackButton(string buttonName)
		{
			Transform target = FindChildRecursive(transform, buttonName);
			if (target == null) return;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
				button.targetGraphic = target.GetComponent<Graphic>();
			}
			else if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>();
			}

			button.onClick.RemoveListener(Back);
			button.onClick.AddListener(Back);
		}

		protected static Transform FindChildRecursive(Transform root, string name)
		{
			if (root == null) return null;
			if (root.name == name) return root;
			foreach (Transform child in root)
			{
				Transform found = FindChildRecursive(child, name);
				if (found != null) return found;
			}
			return null;
		}

		//起動待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			if (Engine == null)
			{
				Debug.LogError("[UtageUguiSaveLoad] AdvEngine is missing. Save/Load list cannot be opened.", this);
				yield break;
			}

			while (Engine.IsWaitBootLoading)
			{
				yield return null;
			}

			AdvSaveManager saveManager = Engine.SaveManager;
			if (saveManager == null)
			{
				Debug.LogError("[UtageUguiSaveLoad] SaveManager is missing. Save/Load list cannot be opened.", this);
				yield break;
			}
			saveManager.ReadAllSaveData();
			List<AdvSaveData> list = new List<AdvSaveData>();
			if (saveManager.IsAutoSave) list.Add(saveManager.AutoSaveData);
			list.AddRange(saveManager.SaveDataList);
			this.itemDataList = list;
			if (gridPage == null) yield break;
			gridPage.Init(itemDataList.Count, CallBackCreateItem);
			gridPage.CreateItems(lastPage);
			isInit = true;
		}


		/// <summary>
		/// リストビューのアイテムが作成されるときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作成されたアイテムのGameObject</param>
		/// <param name="index">作成されたアイテムのインデックス</param>
		protected virtual void CallBackCreateItem(GameObject go, int index)
		{
			if (go == null || itemDataList == null || index < 0 || index >= itemDataList.Count)
			{
				Debug.LogWarning("[UtageUguiSaveLoad] Ignored invalid save/load item callback.", this);
				return;
			}

			UtageUguiSaveLoadItem item = go.GetComponent<UtageUguiSaveLoadItem>();
			if (item == null)
			{
				item = go.AddComponent<UtageUguiSaveLoadItem>();
			}
			AdvSaveData data = itemDataList[index];
			item.Init(data, OnClicked, index, isSave);
		}

		protected virtual void Update()
		{
			//右クリックで戻る
			if (isInit && InputUtil.IsInputGuiClose())
			{
				Back();
			}
		}


		/// <summary>
		/// 各アイテムが押された
		/// </summary>
		/// <param name="button">押されたアイテム</param>
		public virtual void OnTap(UtageUguiSaveLoadItem item)
		{
			if (!TryGetClickedSaveData(item, out AdvSaveData data)) return;
			AdvEngine advEngine = Engine;
			if (advEngine == null) return;

			if (isSave)
			{
				//セーブ画面なら、セーブ処理
				advEngine.WriteSaveData(data);
				item.Refresh(true);
			}
			else
			{
				//ロード画面
				if (data.IsSaved)
				{
					//セーブ済みのデータならこの画面は閉じてロードをする
					if (waitTimeOnLoad <= 0)
					{
						Close();
						if (mainGame != null) mainGame.OpenLoadGame(data);
					}
					else
					{
						if (mainGame != null) mainGame.OpenLoadGame(data);
						StartCoroutine(CoWaitOnLoad(item));
					}
				}
			}
		}

		protected virtual bool TryGetClickedSaveData(UtageUguiSaveLoadItem item, out AdvSaveData data)
		{
			data = null;
			if (item == null || item.Data == null)
			{
				Debug.LogWarning("[UtageUguiSaveLoad] Ignored save/load click with missing item data.", this);
				return false;
			}
			data = item.Data;
			return true;
		}

		
		protected virtual IEnumerator CoWaitOnLoad(UtageUguiSaveLoadItem item)
		{
			this.StoreAndChangeCanvasGroupInput(false);
			yield return new WaitForSeconds(waitTimeOnLoad);
			this.RestoreCanvasGroupInput();
			Close();
		}

		// 各アイテムが押された
		// 宴4以降 
		public virtual void OnClicked(UtageUguiSaveLoadItem item)
		{
			if (!TryGetClickedSaveData(item, out AdvSaveData data)) return;
			AdvEngine advEngine = Engine;
			if (advEngine == null) return;

			if (isSave)
			{
				//セーブ画面の処理

				if (data.Type == AdvSaveData.SaveDataType.Auto)
				{
					//オートセーブならセーブでできない
					if (guideMessage != null)
					{
						guideMessage.Open(LocalizeSystemTextOrDefault(SystemText.UtageGuideMessageSaveFailedAutoSave, "自动存档无法覆盖"));
					}
				}
				else
				{
					void WriteSaveData()
					{
						//セーブ画面なら、セーブ処理
						advEngine.WriteSaveData(data);
						item.Refresh(true);
					}

					if (data.IsSaved)
					{
						//既にセーブされている
						if (dialog != null)
						{
							//上書き確認ダイアログを表示
							dialog.OpenYesNo(
								LanguageSystemText.LocalizeText(SystemText.UtageDialogMessageSaveConfirm), WriteSaveData,
								() => { });
						}
						else
						{
							WriteSaveData();
						}
					}
					else
					{
						WriteSaveData();
					}

				}
			}
			else
			{
				//ロード画面の処理
				if (data.IsSaved)
				{
					//セーブ済みのデータならこの画面は閉じてロードをする
					if (waitTimeOnLoad <= 0)
					{
						Close();
						if (mainGame != null) mainGame.OpenLoadGame(data);
					}
					else
					{
						if (mainGame != null) mainGame.OpenLoadGame(data);
						StartCoroutine(CoWaitOnLoad(item));
					}
				}
				else
				{
					//セーブされていないデータなら、エラーメッセージを表示する
					if (guideMessage!=null)
					{
						guideMessage.Open(LocalizeSystemTextOrDefault(SystemText.UtageGuideMessageLoadFailedNotSaved, "没有可读取的存档"));
					}
				}
			}
		}

		protected virtual string LocalizeSystemTextOrDefault(SystemText type, string fallback)
		{
			LanguageManagerBase language = LanguageManagerBase.Instance;
			if (language != null && language.TryLocalizeText(type.ToString(), out string text))
			{
				return text;
			}
			return fallback;
		}
	}
}
