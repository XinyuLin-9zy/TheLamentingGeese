using System;
using PureMVC.Interfaces;

namespace PureMVC.Patterns.Facade
{
	public class Facade : IFacade, INotifier
	{
		protected IController controller;

		protected IModel model;

		protected IView view;

		protected static IFacade instance;

		protected const string Singleton_MSG = "Facade Singleton already constructed!";

		protected virtual void InitializeFacade()
		{
		}

		public static IFacade GetInstance(Func<IFacade> facadeFunc)
		{
			return null;
		}

		protected virtual void InitializeController()
		{
		}

		protected virtual void InitializeModel()
		{
		}

		protected virtual void InitializeView()
		{
		}

		public virtual void RegisterCommand(string notificationName, ICommand commandFunc)
		{
		}

		public virtual void RemoveCommand(string notificationName)
		{
		}

		public virtual bool HasCommand(string notificationName)
		{
			return false;
		}

		public virtual void RegisterProxy(IProxy proxy)
		{
		}

		public virtual IProxy RetrieveProxy(string proxyName)
		{
			return null;
		}

		public virtual IProxy RemoveProxy(string proxyName)
		{
			return null;
		}

		public virtual bool HasProxy(string proxyName)
		{
			return false;
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

		public void SendNotification(string notificationName, object body = null, string type = null)
		{
		}

		public virtual void NotifyObservers(INotification notification)
		{
		}
	}
}
