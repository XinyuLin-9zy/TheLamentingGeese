using System;
using System.Collections.Concurrent;
using PureMVC.Interfaces;

namespace PureMVC.Core
{
	public class Controller : IController
	{
		protected IView view;

		protected readonly ConcurrentDictionary<string, ICommand> commandMap;

		protected static IController instance;

		protected const string Singleton_MSG = "Controller Singleton already constructed!";

		protected virtual void InitializeController()
		{
		}

		public static IController GetInstance(Func<IController> controllerFunc)
		{
			return null;
		}

		public virtual void ExecuteCommand(INotification notification)
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
	}
}
