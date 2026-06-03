using System.Collections.Generic;
using DG.Tweening;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class UI_PlotView : UguiView, IMediator
{
	public Button btn_unlockAll;

	public Button btn_resetSteamAchievement;

	public Button btn_resetPlotProcess;

	[SerializeField]
	protected AdvEngine engine;

	public UtageUguiMainGame mainGame;

	public Button btnBack;

	public Text text_processTitle;

	public Text text_processValue;

	private LayoutGroup[] layoutGroups;

	private ContentSizeFitter[] sizeFitters;

	private List<Image> chapterElements;

	public Scrollbar scrollBar;

	public Camera uiCamera;

	private GamepadElement[] gamepadElements;

	private UI_PlotChapterElement[] plotChapterElements;

	public UAP_BaseElement enterAccessibility;

	private GamepadRoot gamepadRoot;

	private Tween _tween;

	private float contentLength;

	private bool inTitle;

	private string chapterName;

	public AdvEngine Engine => null;

	private GamepadElement[] GamepadElements => null;

	private GamepadRoot GamepadRoot => null;

	public string MediatorName => null;

	public object ViewComponent { get; set; }

	public void UnlockAllPlotMap()
	{
	}

	public void ResetSteamAchievement()
	{
	}

	public void ResetPlotProcess()
	{
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnSelectLabel(GamepadElement gamepadElement)
	{
	}

	private void FocusTarget()
	{
	}

	private void RefreshUI()
	{
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
	}

	public void ShowMap(bool inTitle, string chapterName = "")
	{
	}

	public string[] ListNotificationInterests()
	{
		return null;
	}

	public void HandleNotification(INotification notification)
	{
	}

	public void OnRegister()
	{
	}

	public void OnRemove()
	{
	}
}
