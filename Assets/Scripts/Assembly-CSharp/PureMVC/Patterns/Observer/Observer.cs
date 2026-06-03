using System;
using PureMVC.Interfaces;

namespace PureMVC.Patterns.Observer
{
	public class Observer : IObserver
	{
		public Action<INotification> NotifyMethod { get; set; }

		public object NotifyContext { get; set; }

		public Observer(Action<INotification> notifyMethod)
		{
		}

		public Observer(Action<INotification> notifyMethod, object notifyContext)
		{
		}

		public virtual void NotifyObserver(INotification Notification)
		{
		}

		public virtual bool CompareNotifyContext(object obj)
		{
			return false;
		}
	}
}
