using Utage;

public class ShowPopBoxCommand : AdvCommand
{
	public ShowPopBoxCommand(StringGridRow row)
		: base(row)
	{
	}

	public override void DoCommand(AdvEngine engine)
	{
	}

	public override bool Wait(AdvEngine engine)
	{
		return false;
	}
}
