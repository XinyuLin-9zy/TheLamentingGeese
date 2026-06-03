using PureMVC.Interfaces;

namespace PureMVC.Patterns.Observer
{
	public class Notifier : INotifier
	{
		protected IFacade Facade => null;

		public virtual void SendNotification(string notificationName, object body = null, string type = null)
		{
		}
	}
}
