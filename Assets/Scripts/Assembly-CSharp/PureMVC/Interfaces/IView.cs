using System;

namespace PureMVC.Interfaces
{
	public interface IView
	{
		void RegisterObserver(string notificationName, IObserver observer);

		void RemoveObserver(string notificationName, object notifyContext);

		void NotifyObservers(INotification notification);

		void RegisterMediator(IMediator mediator);

		void AddRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod);

		void RemoveRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod);

		IMediator RetrieveMediator(string mediatorName);

		IMediator RemoveMediator(string mediatorName);

		bool HasMediator(string mediatorName);
	}
}
