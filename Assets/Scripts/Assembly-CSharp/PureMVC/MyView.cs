using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PureMVC.Interfaces;

namespace PureMVC
{
	public class MyView : IView
	{
		protected readonly ConcurrentDictionary<string, IMediator> mediatorMap;

		protected readonly ConcurrentDictionary<string, IList<IObserver>> observerMap;

		public virtual IObserver RegisterMethod(string notificationName, Action<INotification> method)
		{
			return null;
		}

		public virtual void RemoveMethod(string notificationName, IObserver observer)
		{
		}

		public virtual void RemoveMethod(string notificationName, Action<INotification> method)
		{
		}

		public virtual void RegisterObserver(string notificationName, IObserver observer)
		{
		}

		public virtual void RemoveObserver(string notificationName, object notifyContext)
		{
		}

		public virtual void RegisterMediator(IMediator mediator)
		{
		}

		public virtual void AddRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod)
		{
		}

		public virtual void RemoveRegisterInRunning(IMediator mediator, string[] interests, Action<INotification> addedNotifyMethod)
		{
		}

		public virtual IMediator RetrieveMediator(string mediatorName)
		{
			return null;
		}

		public virtual IMediator RemoveMediator(string mediatorName)
		{
			return null;
		}

		public virtual bool HasMediator(string mediatorName)
		{
			return false;
		}

		public virtual void NotifyObservers(INotification notification)
		{
		}
	}
}
