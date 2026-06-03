using Utage;
using UI;

public class StaffAnimationCommand : AdvCommand
{
	private UI_Staff staff;

	public StaffAnimationCommand(StringGridRow row)
		: base(row)
	{
	}

	public override void DoCommand(AdvEngine engine)
	{
		CustomCommander commander = null;
		if (engine != null)
		{
			commander = engine.GetComponentInChildren<CustomCommander>(true);
		}
		if (commander == null)
		{
			commander = WrapperFindObject.FindObjectOfTypeIncludeInactive<CustomCommander>();
		}
		if (commander == null) return;

		staff = commander.ShowStaffAnimation();
	}

	public override bool Wait(AdvEngine engine)
	{
		return staff != null && staff.IsPlaying;
	}
}
