using Utage;

public class ShortcutInstructionCommand : AdvCommand
{
	private bool isOpen;

	public ShortcutInstructionCommand(StringGridRow row)
		: base(row)
	{
	}

	public override void DoCommand(AdvEngine engine)
	{
	}
}
