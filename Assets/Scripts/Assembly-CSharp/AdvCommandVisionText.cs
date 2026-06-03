using Utage;

public class AdvCommandVisionText : AdvCommand
{
	private const float AutoWaitMinSeconds = 0.5f;

	private float autoWaitTimer;

	private bool hasStartedWaiting;

	public AdvCommandVisionText(StringGridRow row)
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
