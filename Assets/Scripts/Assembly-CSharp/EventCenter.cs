using System;
using PureMVC;
using PureMVC.Interfaces;
using ZeroPlay;

public sealed class EventCenter : Singleton<EventCenter>
{
	private readonly MyView conditionView;

	private readonly MyView view;

	private void NotifyObservers(INotification notification)
	{
	}

	public IObserver RegisterMethod(string notificationName, Action<INotification> method)
	{
		return null;
	}

	public void RemoveMethod(string notificationName, IObserver observer)
	{
	}

	public void RemoveMethod(string notificationName, Action<INotification> method)
	{
	}

	public void RegisterMediator(IMediator mediator)
	{
	}

	public void AddRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod)
	{
	}

	public void RemoveRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod)
	{
	}

	public IMediator RemoveMediator(string mediatorName)
	{
		return null;
	}

	public IMediator RemoveMediator(IMediator mediator)
	{
		return null;
	}

	public void RegisterCondition(IMediator mediator)
	{
	}

	public IMediator RemoveCondition(string mediatorName)
	{
		return null;
	}

	public void SendNotification(string notificationName)
	{
	}

	public void SendNotification(string notificationName, object body)
	{
	}

	public void SendNotification(string notificationName, object body, object body1)
	{
	}

	public void SendNotification(string notificationName, object body, object body1, object body2)
	{
	}

	public void SendNotification(string notificationName, object body, object body1, object body2, object body3)
	{
	}

	public void SendNotification(string notificationName, object body, object body1, object body2, object body3, object body4)
	{
	}

	public void SendNotification(string notificationName, object body, object body1, object body2, object body3, object body4, object body5)
	{
	}
}
