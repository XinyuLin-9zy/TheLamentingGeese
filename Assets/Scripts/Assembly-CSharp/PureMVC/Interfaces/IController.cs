namespace PureMVC.Interfaces
{
	public interface IController
	{
		void RegisterCommand(string notificationName, ICommand commandFunc);

		void ExecuteCommand(INotification notification);

		void RemoveCommand(string notificationName);

		bool HasCommand(string notificationName);
	}
}
