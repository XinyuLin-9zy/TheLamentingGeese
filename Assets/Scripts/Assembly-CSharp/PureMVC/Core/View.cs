using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PureMVC.Interfaces;

namespace PureMVC.Core
{
	public class View : IView
	{
		protected readonly ConcurrentDictionary<string, IMediator> mediatorMap;

		protected readonly ConcurrentDictionary<string, IList<IObserver>> observerMap;

		protected static IView instance;

		protected const string Singleton_MSG = "View Singleton already constructed!";

		protected virtual void InitializeView()
		{
		}

		public static IView GetInstance(Func<IView> viewFunc)
		{
			return null;
		}

		public virtual void RegisterObserver(string notificationName, IObserver observer)
		{
		}

		public virtual void NotifyObservers(INotification notification)
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
	}
}
