using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Patterns.Mediator
{
	public class Mediator : Notifier, IMediator, INotifier
	{
		public static string NAME;

		public string MediatorName { get; protected set; }

		public object ViewComponent { get; set; }

		public Mediator(string mediatorName, object viewComponent = null)
		{
		}

		public virtual string[] ListNotificationInterests()
		{
			return null;
		}

		public virtual void HandleNotification(INotification notification)
		{
		}

		public virtual void OnRegister()
		{
		}

		public virtual void OnRemove()
		{
		}
	}
}
