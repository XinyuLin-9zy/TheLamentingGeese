using System.Collections.Generic;
using System.IO;
using Utage;

public class AdvSteamAchievementSaveData : IBinaryIO
{
	private const int VERSION = 0;

	public List<string> unlockAchievementKeys;

	public string SaveKey => null;

	public void AddAchievementKey(string achievementKey)
	{
	}

	public void RemoveAllAchievementKey()
	{
	}

	public bool CheckAchievementKey(string achievementKey)
	{
		return false;
	}

	public void OnWrite(BinaryWriter writer)
	{
	}

	public void OnRead(BinaryReader reader)
	{
	}
}
