using System.Collections.Generic;
using UnityEngine;

namespace Config
{
	[CreateAssetMenu(fileName = "ExtraKeyData", menuName = "番外/良穗", order = 0)]
	public class ExtraKeyData : ScriptableObject
	{
		public List<string> keys;
	}
}
