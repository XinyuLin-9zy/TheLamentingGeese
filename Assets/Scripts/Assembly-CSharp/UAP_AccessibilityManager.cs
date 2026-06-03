using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/Core/UAP Manager")]
public class UAP_AccessibilityManager : MonoBehaviour
{
	public delegate void OnPauseToggleCallbackFunc();

	public delegate void OnTapEvent();

	public delegate void OnSwipeEvent();

	public delegate void OnAccessibilityModeChanged(bool enabled);

	public enum ESDirection
	{
		EUp = 0,
		EDown = 1,
		ELeft = 2,
		ERight = 3
	}

	public static string PluginVersion;

	public static float PluginVersionAsFloat;

	public bool m_DefaultState;

	public bool m_AutoTurnOnIfScreenReaderDetected;

	public bool m_RecheckAutoEnablingOnResume;

	public bool m_SaveEnabledState;

	public bool m_HandleUI;

	private bool m_BlockInput;

	public bool m_HandleMagicGestures;

	public bool m_ExploreByTouch;

	private float m_ExploreByTouchDelay;

	public bool m_ReadDisabledInteractables;

	public bool m_CyclicMenus;

	public bool m_AllowBuiltInVirtualKeyboard;

	public bool m_DebugOutput;

	private float m_HintDelay;

	private float m_DisabledDelay;

	private float m_ValueDelay;

	private float m_TypeDelay;

	public bool m_WindowsUseMouseSwipes;

	private Canvas m_CanvasRoot;

	public bool m_EditorOverride;

	public bool m_EditorEnabledState;

	[Header("WebGL")]
	public bool m_WebGLTTS;

	public string m_GoogleTTSAPIKey;

	[Header("Windows")]
	public bool m_WindowsTTS;

	public int m_WindowsTTSVolume;

	public bool m_WindowsUseKeys;

	private KeyCode m_NextElementKey;

	private KeyCode m_PreviousElementKey;

	private KeyCode m_NextContainerKey;

	private KeyCode m_PreviousContainerKey;

	private bool m_UseTabAndShiftTabForContainerJumping;

	private KeyCode m_InteractKey;

	private KeyCode m_SliderIncrementKey;

	private KeyCode m_SliderDecrementKey;

	private KeyCode m_DropDownPreviousKey;

	private KeyCode m_DropDownNextKey;

	private KeyCode m_AbortKey;

	private KeyCode m_DownKey;

	private KeyCode m_UpKey;

	private KeyCode m_RightKey;

	private KeyCode m_LeftKey;

	[Header("Android")]
	public bool m_AndroidTTS;

	public bool m_AndroidUseUpAndDownForElements;

	[Header("iOS")]
	public bool m_iOSTTS;

	[Header("Mac OS")]
	public bool m_MacOSTTS;

	[Header("Sound Effects")]
	public AudioClip m_UINavigationClick;

	public AudioClip m_UIInteract;

	public AudioClip m_UIFocusEnter;

	public AudioClip m_UIFocusLeave;

	public AudioClip m_UIBoundsReached;

	public AudioClip m_UIPopUpOpen;

	public AudioClip m_UIPopUpClose;

	[Header("Types")]
	private AudioClip m_DisabledAsAudio;

	private AudioClip m_ButtonAsAudio;

	private AudioClip m_ToggleAsAudio;

	private AudioClip m_SliderAsAudio;

	private AudioClip m_TextEditAsAudio;

	private AudioClip m_DropDownAsAudio;

	[Header("Localization")]
	private static string m_CurrentLanguage;

	private static Dictionary<string, string> m_CurrentLocalizationTable;

	[Header("Other Resources")]
	public AudioSource m_AudioPlayer;

	public AudioSource m_SFXPlayer;

	public RectTransform m_Frame;

	public GameObject m_FrameTemplate;

	public GameObject m_TouchBlocker;

	public Text m_DebugOutputLabel;

	public bool m_AllowVoiceOverGlobal;

	public bool m_DetectVoiceOverAtRuntime;

	private UAP_AudioQueue m_AudioQueue;

	private static UAP_AccessibilityManager instance;

	private static bool isDestroyed;

	private static bool m_IsInitialized;

	private static bool m_IsEnabled;

	private static bool m_Paused;

	private List<AccessibleUIGroupRoot> m_ActiveContainers;

	private List<List<AccessibleUIGroupRoot>> m_SuspendedContainers;

	private List<int> m_SuspendedActiveContainerIndex;

	private AccessibleUIGroupRoot.Accessible_UIElement m_CurrentItem;

	private AccessibleUIGroupRoot.Accessible_UIElement m_PreviousItem;

	private static List<AccessibleUIGroupRoot> m_ContainersToActivate;

	private int m_ActiveContainerIndex;

	private bool m_ReadItemNextUpdate;

	private bool m_CurrentElementHasSoleFocus;

	private int m_LastUpdateTouchCount;

	private OnPauseToggleCallbackFunc m_OnPauseToggleCallbacks;

	private OnTapEvent m_OnTwoFingerSingleTapCallbacks;

	private OnTapEvent m_OnThreeFingerSingleTapCallbacks;

	private OnTapEvent m_OnThreeFingerDoubleTapCallbacks;

	private OnTapEvent m_OnTwoFingerSwipeUpHandler;

	private OnTapEvent m_OnTwoFingerSwipeDownHandler;

	private OnTapEvent m_OnTwoFingerSwipeLeftHandler;

	private OnTapEvent m_OnTwoFingerSwipeRightHandler;

	private OnAccessibilityModeChanged m_OnAccessibilityModeChanged;

	private OnTapEvent m_OnBackCallbacks;

	private bool m_SwipeActive;

	private int m_SwipeTouchCount;

	private bool m_SwipeWaitForLift;

	private Vector2 m_SwipeStartPos;

	private Vector2 m_SwipeCurrPos;

	private float m_SwipeDeltaTime;

	private float m_DoubleTap_LastTapTime;

	private const float m_DoubleTapTime = 0.2f;

	private bool m_DoubleTapFoundThisFrame;

	private float m_MagicTap_LastTapTime;

	private int m_MagicTap_TouchCountHelper;

	private bool m_WaitingForMagicTap;

	private int m_TripleTap_Count;

	private float m_TripleTap_LastTapTime;

	private int m_TripleTap_TouchCountHelper;

	private bool m_WaitingForThreeFingerTap;

	private bool m_ExploreByTouch_IsActive;

	private float m_ExploreByTouch_WaitTimer;

	private Vector3 m_ExploreByTouch_StartPosition;

	private float m_ExploreByTouch_SingleTapWaitTimer;

	private Vector3 m_ExploreByTouch_SingleTapStartPosition;

	private bool m_ContinuousReading;

	private bool m_ContinuousReading_WaitInputClear;

	private static bool sIsEuropeanLanguage;

	private bool m_TouchExplore_Active;

	private float m_TouchExplore_CheckVelocityTimer;

	private float m_TouchExplore_WaitForDoubleTapToExpireTimer;

	private Vector3 m_TouchExplore_CheckStartPosition;

	private const float m_TouchExplore_CheckDuration = 0.15f;

	private float m_ScrubPhaseMaxDuration;

	private float m_ScrubScreenFractionMinswipeWidth;

	private float m_ScrubPhaseTimeoutTimer;

	private bool m_ScrubWaitForLift;

	private int m_ScrubPhaseIndex;

	private Vector3 m_ScrubPhaseStartPoint;

	private Vector3 m_ScrubPhaseLastPoint;

	public bool WindowsUseExploreByTouch => false;

	public static string GoogleTTSAPIKey => null;

	private UAP_AccessibilityManager()
	{
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private static void Initialize()
	{
	}

	private void LoadLocalization()
	{
	}

	public static void SetLanguage(string language)
	{
	}

	private static void InitNGUI()
	{
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode sceneLoadMode)
	{
	}

	private static void CreateNGUIItemFrame()
	{
	}

	public static bool ShouldUseBuiltInKeyboard()
	{
		return false;
	}

	private static bool ShouldAutoEnable()
	{
		return false;
	}

	private static void SavePluginEnabledState()
	{
	}

	private static void ReadItem(AccessibleUIGroupRoot.Accessible_UIElement element, bool quickOnly = false)
	{
	}

	private static void UpdateElementFrame(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
	}

	private static void HideElementFrame()
	{
	}

	private static Canvas GetOwningCanvas(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		return null;
	}

	private static RectTransform GetElementRect(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		return null;
	}

	private bool IsPositionOverElement(Vector2 fingerPos, AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		return false;
	}

	private void SayAudio(AudioClip clip, string altText, UAP_AudioQueue.EAudioType type, bool allowVoiceOver, UAP_AudioQueue.EInterrupt interrupts)
	{
	}

	private void SayAudio(AudioClip clip, string altText, UAP_AudioQueue.EAudioType type, bool allowVoiceOver, UAP_AudioQueue.UAP_GenericCallback callbackOnDone = null, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.None)
	{
	}

	private void SayPause(float durationInSec)
	{
	}

	private void ReadDisabledState()
	{
	}

	private void ReadType()
	{
	}

	private void ReadValue(bool allowPause = true, bool interrupt = false)
	{
	}

	private void ReadHint()
	{
	}

	private static void SpeakElement_Text(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
	}

	public static void PauseAccessibility(bool pause, bool forceRepeatCurrentItem = false)
	{
	}

	private static void StopContinuousReading()
	{
	}

	private void EnableTouchBlocker(bool enable)
	{
	}

	public static void BlockInput(bool block, bool stopSpeakingOnBlock = true)
	{
	}

	public static void ElementRemoved(AccessibleUIGroupRoot.Accessible_UIElement element)
	{
	}

	public static void ActivateContainer(AccessibleUIGroupRoot container, bool activate)
	{
	}

	private AccessibleUIGroupRoot GetActivePopup()
	{
		return null;
	}

	private void ActivateContainer_Internal(AccessibleUIGroupRoot container, bool activate, bool readCurrentItem = true)
	{
	}

	public static void ResetCurrentContainerFocus()
	{
	}

	private void SelectNothing(UAP_BaseElement.EHighlightSource selectionSource)
	{
	}

	private void UpdateCurrentItem(UAP_BaseElement.EHighlightSource selectionSource, bool makeSureItemIsSelected = false)
	{
	}

	public static bool IsEnabled()
	{
		return false;
	}

	public new static string GetInstanceID()
	{
		return null;
	}

	public static bool IsActive()
	{
		return false;
	}

	public static bool IsCurrentPlatformSupported()
	{
		return false;
	}

	public static void EnableMagicGestures(bool enable)
	{
	}

	public static bool IsMagicGesturesEnabled()
	{
		return false;
	}

	public static void EnableAccessibility(bool enable, bool readNotification = false)
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void UpdateMagicTapDetection()
	{
	}

	private void UpdateThreeFingerTapDetection()
	{
	}

	public static void ToggleAccessibility()
	{
	}

	private void ToggleAccessibility_Internal()
	{
	}

	private void Update()
	{
	}

	private void UpdateContinuousReading()
	{
	}

	private void HandlePauseGestures()
	{
	}

	private bool TouchExploreMinDistanceReach(float movedDistance, float moveTime)
	{
		return false;
	}

	private void UpdateExploreByTouch2()
	{
	}

	private void UpdateExploreByTouch()
	{
	}

	private void ExploreByTouch_SelectElementUnderFinger(Vector3 fingerPos)
	{
	}

	private void ReadContainerName()
	{
	}

	private void CancelExploreByTouch()
	{
	}

	private void UpdateDoubleTapDetection()
	{
	}

	public static void FinishCurrentInteraction()
	{
	}

	public static bool GetSpeakDisabledInteractables()
	{
		return false;
	}

	private void LeaveFocussedItem()
	{
	}

	private void CancelFocussedItem()
	{
	}

	private bool IsActiveContainer2DNavigation()
	{
		return false;
	}

	private void UpdateKeyboardInput()
	{
	}

	private void InteractWithElement(AccessibleUIGroupRoot.Accessible_UIElement item)
	{
	}

	private void ReadDropdownItemIndex(ref AccessibleUIGroupRoot.Accessible_UIElement item)
	{
	}

	private void UpdateContainerActivations()
	{
	}

	private void CancelTaps()
	{
	}

	public void OnSwipe(ESDirection dir, int fingerCount)
	{
	}

	public static void SetFocusToTopOfPage()
	{
	}

	private void SetFocusToTopOfPage_Internal()
	{
	}

	private void StartReadingfromTop()
	{
	}

	public static void ReadFromCurrent()
	{
	}

	private void StartReadingFromCurrentElement()
	{
	}

	public static void ReadFromTop()
	{
	}

	private void Navigate2DUIElement(ESDirection direction)
	{
	}

	private void DecrementUIElement()
	{
	}

	private bool DecrementContainer(bool resetToStartItem = false)
	{
		return false;
	}

	private void IncrementUIElement()
	{
	}

	private void PlaySFX(AudioClip clip)
	{
	}

	private bool IncrementContainer(bool resetToStartItem = false)
	{
		return false;
	}

	private void UpdateScrubDetection()
	{
	}

	private void AbortScrubDetection()
	{
	}

	private void UpdateSwipeDetection()
	{
	}

	private Vector3 GetTouchPosition()
	{
		return default(Vector3);
	}

	private int GetTouchCount()
	{
		return 0;
	}

	public static void Say(string textToSay, bool canBeInterrupted = true, bool allowVoiceOver = true, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.Elements)
	{
	}

	public static void SaySkippable(string textToSay)
	{
	}

	public static void SayAs(string textToSay, UAP_AudioQueue.EAudioType sayAs, UAP_AudioQueue.UAP_GenericCallback callback = null)
	{
	}

	private void Say_Internal(string textToSay, bool canBeInterrupted = true, bool allowVoiceOver = true, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.Elements)
	{
	}

	public static bool IsSpeaking()
	{
		return false;
	}

	private void OnApplicationPause(bool paused)
	{
	}

	private void StartScreenOver()
	{
	}

	public static bool IsTalkBackEnabledAndTouchExploreActive()
	{
		return false;
	}

	public static bool IsTalkBackEnabled()
	{
		return false;
	}

	public static void RegisterOnPauseToggledCallback(OnPauseToggleCallbackFunc func)
	{
	}

	public static void UnregisterOnPauseToggledCallback(OnPauseToggleCallbackFunc func)
	{
	}

	public static void RegisterOnBackCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnBackCallback(OnTapEvent func)
	{
	}

	public static void RegisterOnTwoFingerSingleTapCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnTwoFingerSingleTapCallback(OnTapEvent func)
	{
	}

	public static void RegisterOnThreeFingerSingleTapCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnThreeFingerSingleTapCallback(OnTapEvent func)
	{
	}

	public static void RegisterOnThreeFingerDoubleTapCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnThreeFingerDoubleTapCallback(OnTapEvent func)
	{
	}

	public static void RegisterAccessibilityModeChangeCallback(OnAccessibilityModeChanged func)
	{
	}

	public static void UnregisterAccessibilityModeChangeCallback(OnAccessibilityModeChanged func)
	{
	}

	public static void SetTwoFingerSwipeUpHandler(OnTapEvent func)
	{
	}

	public static void ResetTwoFingerSwipeUpHandler()
	{
	}

	public static void RegisterOnTwoFingerSwipeLeftCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnTwoFingerSwipeLeftCallback(OnTapEvent func)
	{
	}

	public static void RegisterOnTwoFingerSwipeRightCallback(OnTapEvent func)
	{
	}

	public static void UnregisterOnTwoFingerSwipeRightCallback(OnTapEvent func)
	{
	}

	public static void SetTwoFingerSwipeDownHandler(OnTapEvent func)
	{
	}

	public static void ResetTwoFingerSwipeDownHandler()
	{
	}

	public static bool SelectElement(GameObject element, bool forceRepeatItem = false)
	{
		return false;
	}

	public static bool MakeActiveContainer(AccessibleUIGroupRoot container, bool forceRepeatActiveItem = false)
	{
		return false;
	}

	public static GameObject GetCurrentFocusObject()
	{
		return null;
	}

	private bool HandleUI()
	{
		return false;
	}

	public static bool UseAndroidTTS()
	{
		return false;
	}

	public static bool UseiOSTTS()
	{
		return false;
	}

	public static bool UseWindowsTTS()
	{
		return false;
	}

	public static bool UseMacOSTTS()
	{
		return false;
	}

	public static bool UseWebGLTTS()
	{
		return false;
	}

	public static void RecalculateUIElementsOrder(GameObject parent = null)
	{
	}

	public static int GetSpeechRate()
	{
		return 0;
	}

	public static int SetSpeechRate(int speechRate)
	{
		return 0;
	}

	public static void StopSpeaking()
	{
	}

	public static string Localize(string key)
	{
		return null;
	}

	public static string Localize_Internal(string key)
	{
		return null;
	}

	public static bool IsVoiceOverAllowed()
	{
		return false;
	}

	public static string FormatNumberToCurrentLocale(ulong intNumber)
	{
		return null;
	}

	public static string FormatNumberToCurrentLocale(double floatNumber)
	{
		return null;
	}

	private static void DetectEuropeanLanguage()
	{
	}

	private static void Log(string message)
	{
	}
}
