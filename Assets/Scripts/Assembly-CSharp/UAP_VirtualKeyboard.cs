using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UAP_VirtualKeyboard : MonoBehaviour
{
	[Serializable]
	public class UAPKeyboardLayout
	{
		public SystemLanguage m_SystemLanguage;

		public string m_Letters;

		public string m_BottomKeys;
	}

	public enum EKeyboardMode
	{
		Default = 0,
		Password = 1
	}

	private enum EKeyboardPage
	{
		Letters = 0,
		Numbers = 1,
		Symbols = 2
	}

	private const string PrefabPath = "UAP Virtual Keyboard";

	[Header("Layouts")]
	public UAPKeyboardLayout m_NumbersLayout;

	public UAPKeyboardLayout m_SymbolsLayout;

	public List<UAPKeyboardLayout> m_SupportedKeyboardLayouts;

	[Header("References")]
	public Transform m_SecondButtonRow;

	public List<Button> m_LetterButtons;

	public Text m_PreviewText;

	public UAP_BaseElement m_PreviewTextAccessible;

	public Button m_LanguageButton;

	public Button m_EmailAtButton;

	public Button m_ShiftKey;

	public Image m_ShiftSymbol;

	public Button m_SwitchKey;

	public Button m_Done;

	public Button m_Cancel;

	public Button m_LeftOfSpace;

	public Button m_RightOfSpace;

	public Button m_ReturnKey;

	private string m_OriginalText;

	private bool m_AllowMutliLine;

	private EKeyboardMode m_KeyboardMode;

	private bool m_StartCapitalized;

	private SystemLanguage m_PreferredLanguage;

	private EKeyboardPage m_CurrentKeyboardPage;

	private UAPKeyboardLayout m_ActiveLetterLayout;

	private bool m_ShiftModeActive;

	private string m_EditedText;

	private string m_PasswordedText;

	private SystemLanguage m_CurrentLanguage;

	private float m_CursorBlinkDuration;

	private float m_CursorBlinkTimer;

	private static UAP_VirtualKeyboard Instance;

	private static UnityAction<string, bool> m_OnFinishListener;

	private static UnityAction<string> m_OnChangeListener;

	private void Update()
	{
	}

	private void OnTextUpdated()
	{
	}

	private void SetKeyboardLayoutForLanguage(SystemLanguage language)
	{
	}

	private void SetLetterButtonsFromString(string letters, string bottomKeys)
	{
	}

	public void OnShiftKeyPressed()
	{
	}

	public void OnToggleKeyPressed()
	{
	}

	public void OnLanguageKeyPressed()
	{
	}

	private void SetLettersLayout()
	{
	}

	private void SetNumbersLayout()
	{
	}

	private void SetSymbolsLayout()
	{
	}

	private void AddLetter(string letter)
	{
	}

	public void OnLetterKeyPressed(Button button)
	{
	}

	public void OnSpacePressed()
	{
	}

	public void OnBackSpacePressed()
	{
	}

	public void OnReturnPressed()
	{
	}

	public void OnClearTextPressed()
	{
	}

	private void AutoSetShiftMode()
	{
	}

	public void OnDonePressed()
	{
	}

	public void OnCancelPressed()
	{
	}

	private void OnApplicationFocus(bool focus)
	{
	}

	private void InitializeKeyboard(string prefilledText, EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
	}

	private void CloseKeyboardOverlay()
	{
	}

	private void OnDestroy()
	{
	}

	public static void SetOnFinishListener(UnityAction<string, bool> callback)
	{
	}

	public static void SetOnChangeListener(UnityAction<string> callback)
	{
	}

	public static void CloseKeyboard()
	{
	}

	public static UAP_VirtualKeyboard ShowOnscreenKeyboard(string prefilledText = "", EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
		return null;
	}

	public static void ClearAllListeners()
	{
	}

	public static bool IsOpen()
	{
		return false;
	}

	public static bool SupportsSystemLanguage()
	{
		return false;
	}
}
