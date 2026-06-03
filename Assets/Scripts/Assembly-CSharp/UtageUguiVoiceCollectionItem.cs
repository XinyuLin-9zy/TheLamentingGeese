using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using TMPro;

namespace Utage.TemplateUI.Gallery
{
    public class UtageUguiVoiceCollectionItem : MonoBehaviour
    {
        [HideIfTMP] public Text title;
        [HideIfLegacyText] public TextMeshProUGUI titleTmp;

        [SerializeField] protected Sprite activeSprite;
        [SerializeField] protected Sprite normalSprite;
        [SerializeField] protected Sprite unlockSprite;
        [SerializeField] protected Button microphone;
        [SerializeField] protected AccessibleButton accessibleLabelText;
        [SerializeField] protected AccessibleButton accessibleLabelVoice;
        [SerializeField] protected Toggle toggle;
        [SerializeField] protected AdvEngine engine;

        public UnityEvent OnInit => onInit;
        [SerializeField] UnityEvent onInit = new UnityEvent();

        public AdvBacklog Data => data;

        protected AdvBacklog data;
        protected Image backgroundImage;
        protected Button button;
        protected Selectable selectable;

        public virtual void Init(AdvBacklog data, Action<UtageUguiVoiceCollectionItem> playClickedEvent, Action<UtageUguiVoiceCollectionItem> removeClickedEvent, int index)
        {
            this.data = data;
            EnsureRuntimeReferences();
            NormalizeLayout();
            SetTextTitle(GetDisplayTitle(data));
            NormalizeTitleLayout();
            BindPlayInteraction(playClickedEvent);
            BindRemoveInteraction(removeClickedEvent);
            ApplyVisualState(false);
            OnInit.Invoke();
        }

        public virtual void SetTextTitle(string text)
        {
            if (title != null) title.gameObject.SetActive(true);
            if (titleTmp != null) titleTmp.gameObject.SetActive(true);
            TextComponentWrapper.SetText(title, titleTmp, text);
        }

        protected virtual void EnsureRuntimeReferences()
        {
            if (title == null && titleTmp == null)
            {
                title = FindComponentByName<Text>("title")
                    ?? FindComponentByName<Text>("Title")
                    ?? FindComponentByName<Text>("text")
                    ?? FindComponentByName<Text>("Text");
                titleTmp = FindComponentByName<TextMeshProUGUI>("title")
                    ?? FindComponentByName<TextMeshProUGUI>("Title")
                    ?? FindComponentByName<TextMeshProUGUI>("text")
                    ?? FindComponentByName<TextMeshProUGUI>("Text");
            }
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
            }
            if (microphone == null)
            {
                Transform delete = FindChildRecursive(transform, "Delete")
                    ?? FindChildRecursive(transform, "delete")
                    ?? FindChildRecursive(transform, "Microphone")
                    ?? FindChildRecursive(transform, "Voice");
                if (delete != null)
                {
                    microphone = delete.GetComponent<Button>() ?? delete.GetComponentInChildren<Button>(true);
                }
            }
            if (engine == null)
            {
                engine = GetComponentInParent<AdvEngine>(true);
            }

            button = GetComponent<Button>();
            selectable = toggle != null ? toggle : button != null ? button : GetComponent<Selectable>();
            backgroundImage = GetComponent<Image>() ?? GetComponentInChildren<Image>(true);

            Graphic titleGraphic = titleTmp != null ? (Graphic)titleTmp : title;
            if (titleGraphic != null)
            {
                titleGraphic.raycastTarget = false;
            }

            if (selectable != null)
            {
                Graphic targetGraphic = EnsureTargetGraphic(selectable, backgroundImage);
                backgroundImage = targetGraphic as Image ?? backgroundImage;
                ApplySelectableSpriteState(selectable);
            }
        }

        protected virtual void NormalizeLayout()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            RectTransform parentRect = rectTransform.parent as RectTransform;
            float targetWidth = parentRect != null && parentRect.rect.width > 0 ? Mathf.Min(parentRect.rect.width, 720f) : 632f;
            if (targetWidth > 0 && Mathf.Abs(rectTransform.rect.width - targetWidth) > 8f)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }

            float currentHeight = rectTransform.rect.height > 0 ? rectTransform.rect.height : rectTransform.sizeDelta.y;
            if (currentHeight < 84f || currentHeight > 120f)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 101f);
            }
        }

        protected virtual string GetDisplayTitle(AdvBacklog backlog)
        {
            if (backlog == null) return "";

            string speaker = StripTags(backlog.MainCharacterNameText);
            string body = StripTags(backlog.Text).Replace("\r", " ").Replace("\n", " ").Trim();
            if (body.Length > 72)
            {
                body = body.Substring(0, 72) + "...";
            }

            if (string.IsNullOrEmpty(speaker)) return body;
            if (string.IsNullOrEmpty(body)) return speaker;
            return speaker + "  " + body;
        }

        protected virtual void BindPlayInteraction(Action<UtageUguiVoiceCollectionItem> playClickedEvent)
        {
            if (toggle != null)
            {
                EnsureTargetGraphic(toggle, backgroundImage);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.interactable = data != null && !string.IsNullOrEmpty(data.MainVoiceFileName);
                toggle.SetIsOnWithoutNotify(false);
                toggle.onValueChanged.AddListener(isOn =>
                {
                    ApplyVisualState(isOn);
                    if (!isOn) return;
                    playClickedEvent?.Invoke(this);
                    toggle.SetIsOnWithoutNotify(false);
                    ApplyVisualState(false);
                });
                return;
            }

            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
                selectable = button;
                ApplySelectableSpriteState(button);
            }

            EnsureTargetGraphic(button, backgroundImage);
            button.interactable = data != null && !string.IsNullOrEmpty(data.MainVoiceFileName);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => playClickedEvent?.Invoke(this));
        }

        protected virtual void BindRemoveInteraction(Action<UtageUguiVoiceCollectionItem> removeClickedEvent)
        {
            if (microphone == null) return;

            if (microphone.targetGraphic == null)
            {
                microphone.targetGraphic = microphone.GetComponent<Graphic>() ?? microphone.GetComponentInChildren<Graphic>(true);
            }
            microphone.interactable = data != null;
            microphone.onClick.RemoveAllListeners();
            microphone.onClick.AddListener(() => removeClickedEvent?.Invoke(this));
        }

        protected virtual void ApplySelectableSpriteState(Selectable currentSelectable)
        {
            if (currentSelectable == null) return;
            if (activeSprite != null)
            {
                currentSelectable.transition = Selectable.Transition.SpriteSwap;
            }

            SpriteState spriteState = currentSelectable.spriteState;
            if (activeSprite != null)
            {
                if (spriteState.highlightedSprite == null) spriteState.highlightedSprite = activeSprite;
                if (spriteState.pressedSprite == null) spriteState.pressedSprite = activeSprite;
                if (spriteState.selectedSprite == null) spriteState.selectedSprite = activeSprite;
            }
            if (unlockSprite != null)
            {
                if (spriteState.disabledSprite == null) spriteState.disabledSprite = unlockSprite;
            }
            currentSelectable.spriteState = spriteState;
        }

        protected virtual void ApplyVisualState(bool isActive)
        {
            if (backgroundImage != null)
            {
                if (isActive && activeSprite != null)
                {
                    backgroundImage.sprite = activeSprite;
                }
                else if (normalSprite != null)
                {
                    backgroundImage.sprite = normalSprite;
                }
            }

            Graphic titleGraphic = titleTmp != null ? (Graphic)titleTmp : title;
            if (titleGraphic != null)
            {
                Color color = titleGraphic.color;
                color.a = 1f;
                titleGraphic.color = color;
            }
        }

        protected virtual void NormalizeTitleLayout()
        {
            if (title != null)
            {
                if (title.font == null) title.font = ResolveFont();
                title.alignment = TextAnchor.MiddleLeft;
                title.color = Color.black;
                title.raycastTarget = false;
            }
            if (titleTmp != null)
            {
                titleTmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                titleTmp.color = Color.black;
                titleTmp.raycastTarget = false;
            }
        }

        protected virtual Font ResolveFont()
        {
            Text source = title ?? GetComponentInChildren<Text>(true);
            if (source != null && source.font != null) return source.font;
            return Font.CreateDynamicFontFromOSFont(new[] { "Source Han Serif CN", "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 24);
        }

        protected virtual Graphic EnsureTargetGraphic(Selectable targetSelectable, Graphic preferredGraphic = null)
        {
            if (targetSelectable == null) return null;

            Graphic graphic = preferredGraphic;
            if (graphic == null)
            {
                graphic = targetSelectable.targetGraphic;
            }
            if (graphic == null)
            {
                graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>(true);
            }
            if (graphic == null)
            {
                Image fallbackImage = gameObject.GetComponent<Image>();
                if (fallbackImage == null)
                {
                    fallbackImage = gameObject.AddComponent<Image>();
                    fallbackImage.color = new Color(1f, 1f, 1f, 0f);
                }
                fallbackImage.raycastTarget = true;
                graphic = fallbackImage;
            }

            targetSelectable.targetGraphic = graphic;
            return graphic;
        }

        protected virtual T FindComponentByName<T>(string targetName) where T : Component
        {
            Transform target = FindChildRecursive(transform, targetName);
            if (target == null) return null;
            return target.GetComponent<T>() ?? target.GetComponentInChildren<T>(true);
        }

        protected static string StripTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            text = Regex.Replace(text, "<sound=[^>]*>(.*?)</sound>", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            text = Regex.Replace(text, "<[^>]+>", "");
            return text.Trim();
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
    }
}
