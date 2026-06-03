// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

namespace Utage
{

    /// <summary>
    /// CGギャラリーの各ボタンのUIのサンプル
    /// </summary>
    [AddComponentMenu("Utage/TemplateUI/UtageUguiCgGalleryItem")]
    public class UtageUguiCgGalleryItem : MonoBehaviour
    {
        public AdvUguiLoadGraphicFile texture;
        [HideIfTMP] public Text count;
        [HideIfLegacyText] public TextMeshProUGUI countTmp;
        [SerializeField] bool keepTextureActive;    //テクスチャのアクティブのオンオフを切り替えるか

        [SerializeField] string formatCount = "{0,2}/{1,2}";
        [SerializeField] bool showCountText = false;
        [SerializeField] GameObject lockOverlayRoot;
        [SerializeField] Text lockLabel;
        [SerializeField] TextMeshProUGUI lockLabelTmp;
        
        //初期化時に呼ばれるイベント
        public UnityEvent OnInit => onInit;
        [SerializeField] UnityEvent onInit = new();
        
        public AdvCgGalleryData Data
        {
            get { return data; }
        }

        AdvCgGalleryData data;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="data">セーブデータ</param>
        /// <param name="index">インデックス</param>
        public virtual void Init(AdvCgGalleryData data, Action<UtageUguiCgGalleryItem> ButtonClickedEvent)
        {
            Init(data);
            
            //コールバックを登録する（宴3までの古いやり方）
            UnityEngine.UI.Button button = EnsureButton();
            if (button != null && ButtonClickedEvent != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ButtonClickedEvent(this));
                button.interactable = data != null && data.IsOpened;
            }
        }

        //初期化
        //クリックイベントを登録しない場合
        public virtual void Init(AdvCgGalleryData data)
        {
            this.data = data;
            EnsureRuntimeReferences();
            NormalizeRuntimeLayout();
            BindDefaultClick();
            if (data == null)
            {
                SetLockState(true);
                SetCountText("");
                SetCountVisible(false);
                OnInit.Invoke();
                return;
            }

            bool isOpen = data.IsOpened;
            if (isOpen)
            {
                SetLockState(false);
                if(!keepTextureActive && texture != null) texture.gameObject.SetActive(true);
                if (texture != null)
                {
                    texture.LoadTextureFile(data.ThumbnailPath);
                    NormalizeLoadedTextureColor();
                }
                SetCountVisible(showCountText);
                SetCountText(showCountText ? string.Format(formatCount, data.NumOpen, data.NumTotal) : "");
            }
            else
            {
                SetLockState(true);
                if (!keepTextureActive && texture != null) texture.gameObject.SetActive(false);
                SetCountText("");
                SetCountVisible(false);
            }
            OnInit.Invoke();
        }

        //クリックイベントを登録しない場合はこちら経由で
        //プレハブ上で、Buttonコンポーネントのインスペクターから登録しておく想定
        public virtual void OnClicked()
        {
            this.GetComponentInParent<UtageUguiCgGallery>().OnClickedButton(this);
        }

        
        public virtual void SetCountText(string text)
        {
            TextComponentWrapper.SetText(count, countTmp, text);
            if (string.IsNullOrEmpty(text))
            {
                SetCountVisible(false);
            }
            else
            {
                ActivateTextObject(count, countTmp);
            }
        }

        protected virtual void EnsureRuntimeReferences()
        {
            if (texture == null)
            {
                texture = GetComponentInChildren<AdvUguiLoadGraphicFile>(true);
            }
            if (texture != null)
            {
                if (texture.OnLoadEnd == null) texture.OnLoadEnd = new UnityEvent();
                texture.OnLoadEnd.RemoveListener(OnTextureLoadEnd);
                texture.OnLoadEnd.AddListener(OnTextureLoadEnd);
            }
            if (count == null && countTmp == null)
            {
                count = FindComponentByName<Text>("Count");
                countTmp = FindComponentByName<TextMeshProUGUI>("Count");
                if (count == null && countTmp == null)
                {
                    count = FindComponentByName<Text>("Text") ?? FindComponentByName<Text>("text");
                    countTmp = FindComponentByName<TextMeshProUGUI>("Text") ?? FindComponentByName<TextMeshProUGUI>("text");
                }
            }
            if (lockOverlayRoot == null)
            {
                Transform root = FindChildRecursive(transform, "__RuntimeLockOverlay") ?? FindChildRecursive(transform, "LockOverlay");
                if (root != null) lockOverlayRoot = root.gameObject;
            }
            if (lockLabel == null && lockLabelTmp == null && lockOverlayRoot != null)
            {
                lockLabel = lockOverlayRoot.GetComponentInChildren<Text>(true);
                lockLabelTmp = lockOverlayRoot.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        protected virtual UnityEngine.UI.Button EnsureButton()
        {
            UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<UnityEngine.UI.Button>();
            }
            if (button.targetGraphic == null)
            {
                button.targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
            }
            return button;
        }

        protected virtual void BindDefaultClick()
        {
            UnityEngine.UI.Button button = EnsureButton();
            if (button == null) return;

            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }

        protected virtual void SetLockState(bool locked)
        {
            // The imported card art already contains the grey locked label.
            // Keep any legacy overlay disabled so it does not duplicate "未解锁".
            if (lockOverlayRoot != null)
            {
                lockOverlayRoot.SetActive(false);
            }
            SetLockLabel("");
        }

        protected virtual void NormalizeRuntimeLayout()
        {
            RectTransform countRect = ResolveCountRect();
            if (countRect != null && (countRect.sizeDelta.x == 0f || countRect.sizeDelta.y == 0f))
            {
                countRect.anchorMin = new Vector2(0.5f, 0f);
                countRect.anchorMax = new Vector2(0.5f, 0f);
                countRect.pivot = new Vector2(0.5f, 0f);
                countRect.anchoredPosition = Vector2.zero;
                countRect.sizeDelta = new Vector2(328f, 30f);
                countRect.localScale = Vector3.one;
            }

            SanitizeNullSpriteImages(transform);
            NormalizeLoadedTextureColor();
        }

        protected virtual void OnTextureLoadEnd()
        {
            NormalizeLoadedTextureColor();
        }

        protected virtual void NormalizeLoadedTextureColor()
        {
            if (texture == null) return;

            RawImage rawImage = texture.GetComponent<RawImage>();
            if (rawImage == null) return;

            rawImage.enabled = true;
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
        }

        protected virtual RectTransform ResolveCountRect()
        {
            if (countTmp != null) return countTmp.transform as RectTransform;
            return count != null ? count.transform as RectTransform : null;
        }

        protected virtual void SetLockLabel(string text)
        {
            TextComponentWrapper.SetText(lockLabel, lockLabelTmp, text);
        }

        protected virtual void SetCountVisible(bool visible)
        {
            if (count != null) count.gameObject.SetActive(visible);
            if (countTmp != null) countTmp.gameObject.SetActive(visible);
        }

        protected virtual GameObject CreateLockOverlay()
        {
            GameObject overlay = new GameObject("__RuntimeLockOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(transform, false);

            RectTransform rect = overlay.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            Image bg = overlay.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            bg.raycastTarget = false;

            GameObject labelObject = new GameObject("LockLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(overlay.transform, false);
            Text label = labelObject.GetComponent<Text>();
            label.text = "未解锁";
            label.font = ResolveFont();
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            RectTransform labelRect = labelObject.transform as RectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;

            overlay.SetActive(false);
            return overlay;
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

        protected virtual Font ResolveFont()
        {
            Text source = count ?? lockLabel ?? GetComponentInChildren<Text>(true);
            if (source != null && source.font != null) return source.font;
            return Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 24);
        }

        protected virtual void ActivateTextObject(Text legacyText, TextMeshProUGUI textMeshPro)
        {
            if (legacyText != null) legacyText.gameObject.SetActive(true);
            if (textMeshPro != null) textMeshPro.gameObject.SetActive(true);
        }

        protected virtual void SanitizeNullSpriteImages(Transform root)
        {
            if (root == null) return;

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image == null) continue;
                if (image.sprite != null) continue;

                image.enabled = false;
                image.raycastTarget = false;
            }
        }

    }
}
