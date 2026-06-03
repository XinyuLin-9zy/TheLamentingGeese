using Config;
using PureMVC.Interfaces;
using UnityEngine;
using Utage;

public class SteamAchievementManager : MonoBehaviour, IMediator
{
	public AdvEngine Engine;

	public SteamAchievementData data;

	private SteamAchievementData runningData;

	public string MediatorName => null;

	public object ViewComponent { get; set; }

	private void Start()
	{
	}

	private void CheckAllAchievement()
	{
	}

	public void ResetAchievement()
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
