namespace PureMVC.Interfaces
{
	public interface IMediator
	{
		string MediatorName { get; }

		object ViewComponent { get; set; }

		string[] ListNotificationInterests();

		void HandleNotification(INotification notification);

		void OnRegister();

		void OnRemove();
	}
}
