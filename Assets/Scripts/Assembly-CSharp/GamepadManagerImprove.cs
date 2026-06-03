using System.Collections.Generic;
using ZeroPlay;

public class GamepadManagerImprove : Singleton<GamepadManagerImprove>
{
	private long index;

	private readonly List<GamepadRoot> rootList;

	private GamepadRoot activatedRoot;

	public int CurLayerCount => 0;

	public GamepadRoot ActivatedRoot => null;

	public void RegisterRoot(GamepadRoot root)
	{
	}

	public void RemoveRegisterByRoot(GamepadRoot root)
	{
	}

	public void RemoveRegisterByIndex(long targetIndex)
	{
	}

	private void ReSelectGamepadRoot()
	{
	}
}
