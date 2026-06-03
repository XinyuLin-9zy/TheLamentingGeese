// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Utage;
using System;
using TMPro;

namespace Utage
{

	/// <summary>
	/// セーブロード用のUIのサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiSaveLoadItem")]
	public class UtageUguiSaveLoadItem : MonoBehaviour
	{
		/// <summary>本文</summary>
		[HideIfTMP] public Text text;

		[HideIfLegacyText] public TextMeshProNovelText textTmp;

		/// <summary>セーブ番号</summary>
		[HideIfTMP] public Text no;

		/// セーブ番号のテキスト表示のフォーマット
		[SerializeField] public string formatNo = "No.{0,3}";
		[HideIfLegacyText] public TextMeshProUGUI noTmp;

		/// セーブ番号のテキスト表示のフォーマット(オートセーブ時)
		[SerializeField] public string formatNoAuto = "Auto";

		/// <summary>日付</summary>
		[HideIfTMP] public Text date;

		[HideIfLegacyText] public TextMeshProUGUI dateTmp;

		/// <summary>スクショ</summary>
		public RawImage captureImage;

		[SerializeField] protected Image image_bg;
		[SerializeField] protected Image image_save;
		[SerializeField] protected Image image_load;
		[SerializeField] protected Image image_noData;
		[SerializeField] protected Sprite sprite_autoArchive;
		[SerializeField] GameObject noDataRoot;

		/// <summary>オートセーブ用のテクスチャ</summary>
		public Texture2D autoSaveIcon;

		/// <summary>未セーブだった場合に表示するテキスト</summary>
		public string textEmpty = "Empty";

		//リフレッシュ時に呼ばれるイベント
		public UnityEvent OnRefresh => onRefresh;
		[SerializeField] UnityEvent onRefresh = new();

		protected UnityEngine.UI.Button button;

		public AdvSaveData Data
		{
			get { return data; }
		}

		protected AdvSaveData data;

		public int Index
		{
			get { return index; }
		}

		protected int index;

		protected Color defaultColor;
		protected Sprite defaultBgSprite;

		static readonly Color EmptyCaptureColor = new Color(1f, 1f, 1f, 0f);

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">セーブデータ</param>
		/// <param name="index">インデックス</param>
		/// <param name="isSave">セーブ画面用ならtrue、ロード画面用ならfalse</param>
		public virtual void Init(AdvSaveData data, Action<UtageUguiSaveLoadItem> ButtonClickedEvent, int index,
			bool isSave)
		{
			this.data = data;
			this.index = index;
			EnsureRuntimeReferences();
			if (this.button != null)
			{
				this.button.onClick.RemoveAllListeners();
				if (ButtonClickedEvent != null)
				{
					this.button.onClick.AddListener(() => ButtonClickedEvent(this));
				}
			}
			Refresh(isSave);
		}

		//初期化
		//クリックイベントを登録しない場合
		public virtual void Init(AdvSaveData data, int index, bool isSave)
		{
			this.data = data;
			this.index = index;
			EnsureRuntimeReferences();
			if (this.button != null)
			{
				this.button.onClick.RemoveAllListeners();
				this.button.onClick.AddListener(OnClicked);
			}
			Refresh(isSave);
		}

		public virtual void Refresh(bool isSave)
		{
			EnsureRuntimeReferences();
			if (data == null) return;

			SetTextNo(string.Format(formatNo, index));
			bool showNoDataOverlay = !data.IsSaved && ShouldShowNoDataOverlay();
			UpdateModeImages(isSave, showNoDataOverlay);
			UpdateBackgroundForData();
			SetNoDataVisible(showNoDataOverlay);
			if (data.IsSaved)
			{
				if (data.Type == AdvSaveData.SaveDataType.Auto || data.Texture == null)
				{
					if (data.Type == AdvSaveData.SaveDataType.Auto && autoSaveIcon != null)
					{
						//オートセーブ用のテクスチャ
						SetCaptureImage(autoSaveIcon, Color.white);
					}
					else if(this.TryGetComponent(out UtageUguiSaveLoadItemThumbnail thumbnail) && thumbnail.TrySetThumbnail(data))
					{
						//サムネイル画像がセーブデータ内のパラメーターに設定されている場合
						if (captureImage != null)
						{
							captureImage.color = Color.white;
							captureImage.enabled = true;
						}
					}
					else
					{
						//テクスチャがない
						SetCaptureImage(null, EmptyCaptureColor);
					}
				}
				else
				{
					SetCaptureImage(data.Texture, Color.white);
				}

				SetText(data.Title);
				SetTextDate(UtageToolKit.DateToStringJp(data.Date));
				if (button != null)
				{
					button.interactable = true;
				}
			}
			else
			{
				SetCaptureImage(null, EmptyCaptureColor);
				SetText(showNoDataOverlay ? "" : textEmpty);
				SetTextDate("");
				if (button != null)
				{
					button.interactable = isSave;
				}
			}


			//オートセーブデータ
			if (data.Type == AdvSaveData.SaveDataType.Auto)
			{
				SetTextNo(string.Format(formatNoAuto));
				//セーブはできない
				if (isSave)
				{
					if (button != null)
					{
						button.interactable = false;
					}
				}
			}
			OnRefresh.Invoke();
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (button == null)
			{
				button = GetComponent<Button>();
				if (button == null)
				{
					button = gameObject.AddComponent<Button>();
				}
			}

			if (button.targetGraphic == null)
			{
				button.targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
			}

			if (text == null && textTmp == null)
			{
				text = FindComponentByName<Text>("text") ?? FindComponentByName<Text>("Uguitext");
				textTmp = FindComponentByName<TextMeshProNovelText>("text") ?? FindComponentByName<TextMeshProNovelText>("Uguitext");
			}
			if (no == null && noTmp == null)
			{
				no = FindComponentByName<Text>("no");
				noTmp = FindComponentByName<TextMeshProUGUI>("no");
			}
			if (date == null && dateTmp == null)
			{
				date = FindComponentByName<Text>("date");
				dateTmp = FindComponentByName<TextMeshProUGUI>("date");
			}
			if (captureImage == null)
			{
				captureImage = FindComponentByName<RawImage>("CaptureImage") ?? GetComponentInChildren<RawImage>(true);
			}
			if (image_bg == null)
			{
				image_bg = GetComponent<Image>();
			}
			if (image_bg != null && defaultBgSprite == null)
			{
				defaultBgSprite = image_bg.sprite;
			}
			if (image_save == null)
			{
				image_save = FindComponentByName<Image>("Image_Save");
			}
			if (image_load == null)
			{
				image_load = FindComponentByName<Image>("Image_Load");
			}
			if (image_noData == null)
			{
				image_noData = FindComponentByName<Image>("Image_NoData");
			}
			if (noDataRoot == null)
			{
				Transform noData = FindChildRecursive(transform, "Image_NoData")
					?? FindChildRecursive(transform, "NoData")
					?? FindChildRecursive(transform, "Empty");
				if (noData != null) noDataRoot = noData.gameObject;
			}
		}

		protected virtual T FindComponentByName<T>(string targetName) where T : Component
		{
			Transform target = FindChildRecursive(transform, targetName);
			if (target == null) return null;
			return target.GetComponent<T>() ?? target.GetComponentInChildren<T>(true);
		}

		protected static Transform FindChildRecursive(Transform root, string targetName)
		{
			if (root == null) return null;
			if (root.name == targetName) return root;
			foreach (Transform child in root)
			{
				Transform found = FindChildRecursive(child, targetName);
				if (found != null) return found;
			}
			return null;
		}

		protected virtual void UpdateModeImages(bool isSave, bool showNoDataOverlay)
		{
			if (image_save != null)
			{
				image_save.gameObject.SetActive(isSave);
			}
			if (image_load != null)
			{
				image_load.gameObject.SetActive(!isSave);
			}
			if (image_noData != null)
			{
				image_noData.gameObject.SetActive(showNoDataOverlay);
			}
		}

		protected virtual void UpdateBackgroundForData()
		{
			if (image_bg == null || data == null) return;

			if (data.Type == AdvSaveData.SaveDataType.Auto && sprite_autoArchive != null)
			{
				image_bg.sprite = sprite_autoArchive;
			}
			else if (defaultBgSprite != null)
			{
				image_bg.sprite = defaultBgSprite;
			}
		}

		protected virtual void SetCaptureImage(Texture texture, Color color)
		{
			if (captureImage == null) return;
			captureImage.texture = texture;
			captureImage.color = color;
			captureImage.enabled = texture != null || color.a > 0.001f;
			SetCaptureFrameVisible(captureImage.enabled);
		}

		protected virtual void SetNoDataVisible(bool visible)
		{
			if (noDataRoot != null)
			{
				noDataRoot.SetActive(visible);
			}
		}

		protected virtual bool ShouldShowNoDataOverlay()
		{
			return noDataRoot != null;
		}

		protected virtual void SetCaptureFrameVisible(bool visible)
		{
			if (captureImage == null) return;

			Transform parent = captureImage.transform.parent;
			if (parent == null || parent == transform) return;

			foreach (Image image in parent.GetComponentsInChildren<Image>(true))
			{
				if (image == null) continue;
				if (noDataRoot != null && image.transform.IsChildOf(noDataRoot.transform)) continue;
				if (image.transform.IsChildOf(captureImage.transform)) continue;
				if (!IsLikelyCapturePlaceholder(image)) continue;

				Color color = image.color;
				color.a = visible ? Mathf.Max(color.a, 1f) : 0f;
				image.color = color;
				image.raycastTarget = visible && image.raycastTarget;
			}
		}

		protected virtual bool IsLikelyCapturePlaceholder(Image image)
		{
			if (image == null) return false;
			if (image.GetComponent<Button>() != null) return false;

			string imageName = image.name ?? "";
			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";
			if (image.sprite == null) return true;
			if (imageName.IndexOf("Capture", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (imageName.IndexOf("Thumbnail", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (imageName.IndexOf("Screenshot", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (spriteName.IndexOf("pic", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (spriteName.IndexOf("capture", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (spriteName.IndexOf("thumbnail", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (spriteName.IndexOf("screenshot", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (spriteName.IndexOf("UISprite", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			return false;
		}

		protected virtual void OnDestroy()
		{
			if (captureImage != null && captureImage.texture != null)
			{
				captureImage.texture = null;
			}
		}
		
		public virtual void SetText(string str)
		{
			NovelTextComponentWrapper.SetText(text, textTmp, str);
		}

		public virtual void SetTextDate(string str)
		{
			TextComponentWrapper.SetText(date, dateTmp, str);
		}

		public virtual void SetTextNo(string str)
		{
			TextComponentWrapper.SetText(no, noTmp, str);
		}

		public void OnClicked()
		{
			UtageUguiSaveLoad saveLoad = this.GetComponentInParent<UtageUguiSaveLoad>();
			if (saveLoad != null)
			{
				saveLoad.OnClicked(this);
			}
		}
	}
}
