using System;

namespace PureMVC.Interfaces
{
	public interface IObserver
	{
		Action<INotification> NotifyMethod { get; set; }

		object NotifyContext { set; }

		void NotifyObserver(INotification notification);

		bool CompareNotifyContext(object obj);
	}
}
