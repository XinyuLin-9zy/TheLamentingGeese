using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Accessibility/UI/Accessible UI Group Root")]
public class AccessibleUIGroupRoot : MonoBehaviour
{
	public enum EUIElement
	{
		EUndefined = 0,
		EButton = 1,
		ELabel = 2,
		EToggle = 3,
		ESlider = 4,
		ETextEdit = 5,
		EDropDown = 6
	}

	public class Accessible_UIElement
	{
		public EUIElement m_Type;

		public UAP_BaseElement m_Object;

		public Vector2 m_Pos;

		public int m_PositionOrder;

		public int m_SecondaryOrder;

		public bool AllowsVoiceOver()
		{
			return false;
		}

		public bool ReadType()
		{
			return false;
		}

		public void CalculatePositionOrder(UAP_BaseElement uiElement, int backupIndex)
		{
		}

		public Accessible_UIElement(UAP_BaseElement item, EUIElement type, int index)
		{
		}
	}

	public bool m_PopUp;

	public bool m_AllowExternalJoining;

	public bool m_AutoRead;

	public int m_Priority;

	public string m_ContainerName;

	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey;

	public bool m_2DNavigation;

	public bool m_ConstrainToContainerUp;

	public bool m_ConstrainToContainerDown;

	public bool m_ConstrainToContainerLeft;

	public bool m_ConstrainToContainerRight;

	public bool m_AllowTouchExplore;

	private bool m_HasStarted;

	private bool m_RefreshNextFrame;

	private bool m_ActivateContainerNextFrame;

	[Tooltip("This causes a 2 frame delay before the interface is accessible, but solves issues with screens that perform automatic UI elements ordering at start - such as any dynamically built UI, expanding scroll views, horizontal grids etc")]
	public bool m_DoubleCheckUIElementsPositions;

	private bool m_NeedsRefreshBeforeActivation;

	private List<Accessible_UIElement> m_AllElements;

	private UAP_BaseElement m_CurrentStartItem;

	private int m_CurrentItemIndex;

	public bool IsConstrainedToContainer(UAP_AccessibilityManager.ESDirection direction)
	{
		return false;
	}

	public static void GetAbsoluteAnchors(RectTransform t, out Vector2 anchorMin, out Vector2 anchorMax, out Vector2 centerPos, bool stopAtScrollView = false)
	{
		anchorMin = default(Vector2);
		anchorMax = default(Vector2);
		centerPos = default(Vector2);
	}

	public void CheckForRegister(UAP_BaseElement item)
	{
	}

	public void SetAsStartItem(UAP_BaseElement item)
	{
	}

	public void UnRegister(UAP_BaseElement item)
	{
	}

	public void RefreshContainer()
	{
	}

	private void ActivateContainer_Internal()
	{
	}

	private void Register_Item(UAP_BaseElement item)
	{
	}

	private void OnEnable()
	{
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	public void ResetToStart()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}

	public Accessible_UIElement GetCurrentElement(bool rollOverAllowed)
	{
		return null;
	}

	private int FindFirstActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		return 0;
	}

	private int FindPreviousActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		return 0;
	}

	public bool IncrementCurrentItem(bool rollOverAllowed)
	{
		return false;
	}

	public bool DecrementCurrentItem(bool rollOverAllowed)
	{
		return false;
	}

	public bool MoveFocus2D(UAP_AccessibilityManager.ESDirection direction)
	{
		return false;
	}

	private void Update()
	{
	}

	public void RefreshNextUpdate()
	{
	}

	public void JumpToFirst()
	{
	}

	public void JumpToLast()
	{
	}

	public void SetActiveElementIndex(int index, bool rollOverAllowed)
	{
	}

	public int GetCurrentElementIndex()
	{
		return 0;
	}

	public List<Accessible_UIElement> GetElements()
	{
		return null;
	}

	public bool SelectItem(UAP_BaseElement element, bool forceRepeatItem = false)
	{
		return false;
	}

	public string GetContainerName(bool useGameObjectNameIfNone = false)
	{
		return null;
	}

	public bool IsNameLocalizationKey()
	{
		return false;
	}
}
