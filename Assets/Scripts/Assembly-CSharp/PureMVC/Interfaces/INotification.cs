namespace PureMVC.Interfaces
{
	public interface INotification
	{
		string Name { get; }

		object Body { get; set; }

		object Body1 { get; set; }

		object Body2 { get; set; }

		object Body3 { get; set; }

		object Body4 { get; set; }

		new string ToString();
	}
}
